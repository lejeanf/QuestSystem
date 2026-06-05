using System;
using System.Collections;
using System.Collections.Generic;
using jeanf.EventSystem;
using jeanf.validationTools;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

namespace jeanf.questsystem
{
    public class QuestManager : MonoBehaviour, IDebugBehaviour, IValidatable
    {
        #region variables
        #region interface variables
        public bool isDebug
        {
            get => _isDebug;
            set => _isDebug = value;
        }
        public bool IsValid { get; private set; }

        [SerializeField] private bool _isDebug = false;
        #endregion

        #region event channels
        [Header("Broadcasting on:")]
        [SerializeField] [Validation("A reference to the questStatusUpdateChannel is required.")] private StringEventChannelSO questStatusUpdateChannel;
        [SerializeField] [Validation("A reference to the questProgress is required.")] private StringFloatEventChannelSO questProgress;
        [SerializeField] [Validation("A reference to the questInitialCheck channel is required.")] private StringEventChannelSO QuestInitialCheck;
        [Header("Listening on:")] [SerializeField] [Validation("A reference to the questStatusUpdateRequested is required.")] private StringEventChannelSO questStatusUpdateRequested;
        #endregion

        #region other variables
        [FormerlySerializedAs("loadQuestState")] [Header("Config")] [SerializeField]
        private bool loadSavedQuestState = true;
        private Dictionary<string, Quest> questMap;
        private readonly Queue<string> _pendingStartQuestIds = new Queue<string>();
        private readonly Queue<string> _pendingFinishQuestIds = new Queue<string>();
        private int currentPlayerLevel;
        
        // Label strings to load for scriptable objects
        [Header("Addressables group to load:")]
        [Tooltip("This group should contain all the scriptable objects that define your quests.")]
        public List<string> _keys = new List<string>() { "Quests" };
        private AsyncOperationHandle<IList<QuestSO>> _questAssetsHandle;

        #endregion
        #endregion

        #region Methods
        #region Standard Unity Methods
       
        private void OnEnable()
        {
            GameEventsManager.instance.questEvents.onStartQuest += StartQuest;
            GameEventsManager.instance.questEvents.onFinishQuest += FinishQuest;       
            GameEventsManager.instance.playerEvents.onPlayerLevelChange += PlayerLevelChange;
            questStatusUpdateRequested.OnEventRaised += OnQuestStatusUpdateRequested;
            //GameEventsManager.instance.questEvents.onQuestStepStateChange += QuestStepStateChange;
        }
        private void OnDisable() => Unsubscribe();
        private void OnDestroy() => Unsubscribe();
        private void Unsubscribe()
        {
            GameEventsManager.instance.questEvents.onStartQuest -= StartQuest;
            GameEventsManager.instance.questEvents.onFinishQuest -= FinishQuest;
            GameEventsManager.instance.playerEvents.onPlayerLevelChange -= PlayerLevelChange;
            questStatusUpdateRequested.OnEventRaised -= OnQuestStatusUpdateRequested;
            if (_questAssetsHandle.IsValid())
                Addressables.Release(_questAssetsHandle);
            QuestCatalogue.Reset();
            _pendingStartQuestIds.Clear();
            _pendingFinishQuestIds.Clear();
            //GameEventsManager.instance.questEvents.onQuestStepStateChange -= QuestStepStateChange;
        }

        private void OnQuestStatusUpdateRequested(string questId)
        {
            if (questMap == null || !questMap.TryGetValue(questId, out var quest))
                return;
            CheckRequirementsMet(quest);
        }
     
        private async Awaitable Start()
        {
            QuestCatalogue.BeginLoad();
            try
            {
                questMap = await CreateQuestMap();
                QuestCatalogue.MarkReady();
                FlushPendingQuestOperations();

                foreach (var quest in questMap)
                {
                    CheckIfQuestIsAlreadyLoaded(quest.Key);
                    GameEventsManager.instance.questEvents.QuestStateChange(quest.Value);
                }
            }
            catch (Exception e)
            {
                QuestCatalogue.MarkFailed(e);
                Debug.LogError($"[QuestManager] Error loading quest catalogue: {e.Message}");
            }
        }
        
        private void Update()
        {
            if (!QuestCatalogue.IsReady || questMap == null)
                return;

            // loop through ALL quests
            foreach (Quest quest in questMap.Values)
            {
                // if we're now meeting the requirements, switch over to the CAN_START state
                if (quest.state == QuestState.REQUIREMENTS_NOT_MET && CheckRequirementsMet(quest))
                {
                    ChangeQuestState(quest.questSO.id, QuestState.CAN_START);
                }
            }
        }
        private void OnApplicationQuit()
        {
            if (questMap == null)
                return;
            foreach (Quest quest in questMap.Values)
            {
                SaveQuest(quest);
            }
        }
        #endregion

        #region Quest Checks and getters

        private static async Awaitable AwaitAsyncOperation<T>(AsyncOperationHandle<T> handle)
        {
            while (!handle.IsDone)
                await Awaitable.NextFrameAsync();

            if (handle.Status != AsyncOperationStatus.Succeeded)
                throw new Exception("QuestManager failed to load Addressable assets.");
        }

        private async Awaitable<IList<QuestSO>> LoadQuestAssets(List<string> labels)
        {
            _questAssetsHandle = Addressables.LoadAssetsAsync<QuestSO>(
                labels,
                addressable =>
                {
                    if (isDebug) Debug.Log($"[QuestManager] Loaded quest SO: {addressable.name}");
                },
                Addressables.MergeMode.Union,
                true);

            await AwaitAsyncOperation(_questAssetsHandle);
            return _questAssetsHandle.Result;
        }
      
        private async Awaitable<Dictionary<string, Quest>> CreateQuestMap()
        {
            // loads all QuestInfoSO Scriptable Objects under the Assets/Quests folder
            Debug.Log("[QuestManager] Creating quest map");
            IList<QuestSO> allQuests =  new List<QuestSO>();
            try
            {
                allQuests = await LoadQuestAssets(_keys);
                if (isDebug) Debug.Log($"[QuestManager] Successfully loaded {allQuests.Count} assets!");
            }
            catch (Exception e)
            {
                Debug.LogError($"[QuestManager] Error loading assets: {e.Message}");
                throw;
            }
            
            // Create the quest map
            Dictionary<string, Quest> questMap = new Dictionary<string, Quest>();
            foreach (QuestSO questSO in allQuests)
            {
                var id = questSO.id;
                if (questMap.ContainsKey(id))
                {
                    Debug.LogWarning($"[QuestManager] Duplicate ID found when creating quest map: {questSO.id}");
                }
                else
                {
                    questMap.Add(id, LoadQuest(questSO));
                }
                if (isDebug) Debug.Log($"[QuestManager] Adding {questSO.name} to the questmap, its id is: {questSO.id}");
            }
            return questMap;
        }
        private void CheckIfQuestIsAlreadyLoaded(string id)
        {
            QuestInitialCheck.RaiseEvent(id);
        }
        public Quest GetQuestById(string id)
        {
            if (questMap == null || !questMap.TryGetValue(id, out var quest))
            {
                Debug.LogError($"[QuestManager] ID not found in the quest map: {id}");
                return null;
            }

            return quest;
        }

        private void FlushPendingQuestOperations()
        {
            while (_pendingStartQuestIds.Count > 0)
                StartQuestCore(_pendingStartQuestIds.Dequeue());

            while (_pendingFinishQuestIds.Count > 0)
                FinishQuestCore(_pendingFinishQuestIds.Dequeue());
        }

        private static bool EnqueueUnique(Queue<string> queue, string id)
        {
            foreach (var pending in queue)
            {
                if (pending == id)
                    return false;
            }

            queue.Enqueue(id);
            return true;
        }
        private bool CheckRequirementsMet(Quest quest)
        {
            // check player level requirements
            var meetsRequirements = !(currentPlayerLevel < quest.questSO.levelRequirement);

            // check quest prerequisites for completion
            foreach (QuestSO prerequisiteQuestInfo in quest.questSO.questPrerequisites)
            {
                var prerequisite = GetQuestById(prerequisiteQuestInfo.id);
                if (prerequisite == null || prerequisite.state != QuestState.FINISHED)
                    meetsRequirements = false;
            }

            if (isDebug) Debug.Log($"[QuestManager] checking requirements for quest: {quest.questSO.name}, [{quest.questSO.id}], meetsRequirements: {meetsRequirements}");

            return meetsRequirements;
        }
        private void ChangeQuestState(string id, QuestState state)
        {
            var quest = GetQuestById(id);
            if (quest == null)
                return;

            quest.state = state;
            GameEventsManager.instance.questEvents.QuestStateChange(quest);
        }
        #endregion

        #region main process
        private void StartQuest(string id)
        {
            if (!QuestCatalogue.IsReady || questMap == null)
            {
                if (EnqueueUnique(_pendingStartQuestIds, id) && isDebug)
                    Debug.Log($"[QuestManager] StartQuest deferred until catalogue is ready: {id}");
                return;
            }

            StartQuestCore(id);
        }

        private void StartQuestCore(string id)
        {
            var quest = GetQuestById(id);
            if (quest == null)
                return;

            ChangeQuestState(quest.questSO.id, QuestState.IN_PROGRESS);
            SaveQuest(quest);
            if (!quest.sendMessageOnInitialization) return;
            quest.messageChannel.RaiseEvent(quest.messageToSendOnInit);
            if (isDebug)
                Debug.Log($"[QuestManager] quest id:{id} started, message on init: {quest.messageToSendOnInit}");
        }
        private void UpdateProgress(Quest quest)
        {
            var progress = 0;
            if (quest.questSO.id == null) Debug.LogError("[QuestManager] quest.questSO.id is null");
            if (isDebug) Debug.Log($"[QuestManager] [{quest.questSO.id}] progress: {progress * 100}%", this);
            questProgress.RaiseEvent(quest.questSO.id, progress);
        }
        private void FinishQuest(string id)
        {
            if (!QuestCatalogue.IsReady || questMap == null)
            {
                if (EnqueueUnique(_pendingFinishQuestIds, id) && isDebug)
                    Debug.Log($"[QuestManager] FinishQuest deferred until catalogue is ready: {id}");
                return;
            }

            FinishQuestCore(id);
        }

        private void FinishQuestCore(string id)
        {
            var quest = GetQuestById(id);
            if (quest == null)
                return;

            UpdateProgress(quest);
            ClaimRewards(quest);
            ChangeQuestState(quest.questSO.id, QuestState.FINISHED);
            questStatusUpdateChannel.RaiseEvent(quest.questSO.id);
            questProgress.RaiseEvent(quest.questSO.id, 1);
            SaveQuest(quest);
        }
        #endregion

        #region rewards and progress
        private void ClaimRewards(Quest quest)
        {
            GameEventsManager.instance.scenarioEvents.ScenarioUnlocked(quest.questSO.unlockedScenario);
        }

        private void PlayerLevelChange(int level)
        {
            currentPlayerLevel = level;
        }
        #endregion

        #region saving and loading
        private void SaveQuest(Quest quest)
        {
            try
            {
                //Save; active steps + quest step status for each, completed steps, quest status, progress/playerLevel/?
                QuestData questData = null;
                //quest.GetQuestData();
                // serialize using JsonUtility, but use whatever you want here (like JSON.NET)
                string serializedData = JsonUtility.ToJson(questData);
                //if(isDebug) Debug.Log($"saved data {serializedData}");
                // saving to PlayerPrefs is just a quick example for this tutorial video,
                // you probably don't want to save this info there long-term.
                // instead, use an actual Save & Load system and write to a file, the cloud, etc..
                PlayerPrefs.SetString(quest.questSO.id, serializedData);
            }
            catch (System.Exception e)
            {
                //Debug.LogError("Failed to save quest with id " + quest.questSO.id + ": " + e);
            }
        }
        private Quest LoadQuest(QuestSO questSO)
        {
            if (isDebug) Debug.Log($"[QuestManager] attempting to load quest with id: [{questSO.id}]");
            var quest = new Quest(questSO);
            try
            {
                // load quest from saved data
                if (PlayerPrefs.HasKey(questSO.id) && loadSavedQuestState)
                {
                    var serializedData = PlayerPrefs.GetString(questSO.id);
                    var questData = JsonUtility.FromJson<QuestData>(serializedData);
                    quest = new Quest(questSO, questData.state, questData.questStepIndex, questData.questStepStates); 
                    if (isDebug) Debug.Log($"[QuestManager] loaded previously saved quest with id: [{quest.questSO.id}]");
                }
                // otherwise, initialize a new quest
                else
                {
                    quest = new Quest(questSO);
                    if (isDebug) Debug.Log($"[QuestManager] loaded a fresh instance of quest with id: [{quest.questSO.id}]");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[QuestManager] Failed to load quest with id: [{quest.questSO.id}] - exception: {e}");
            }

            return quest;
        }
        #endregion

        #region Validation Tools
        private void ValidityCheck()
        {
            const string searching = "attempting to find";
            const string _ = "Quests/Channels"; // search target
            const string searchLocation = "the resources folder";
            const string readInstructions = "please read the package instruction for further help";
            
            
            var validityCheck = true;
            var invalidObjects = new List<object>();
            var errorMessages = new List<string>();
            
            if (QuestInitialCheck == null)
            {
                if (isDebug) Debug.Log($"[QuestManager] {searching} {_}/QuestInitialCheck in {searchLocation}", this);
                QuestInitialCheck = Resources.Load<StringEventChannelSO>($"{_}/QuestInitialCheck");
                if (QuestInitialCheck == null)
                {
                    errorMessages.Add($"{_}/QuestInitialCheck is not {searchLocation} {readInstructions}");
                    validityCheck = false;
                    invalidObjects.Add(QuestInitialCheck);
                }
            }
            
            if (questStatusUpdateChannel == null)
            {
                if (isDebug) Debug.Log($"[QuestManager] {searching} {_}/QuestStatusUpdate in {searchLocation}", this);
                questStatusUpdateChannel = Resources.Load<StringEventChannelSO>($"{_}/QuestStatusUpdate");
                if (questStatusUpdateChannel == null)
                {
                    errorMessages.Add($"{_}/QuestStatusUpdate is not {searchLocation} {readInstructions}");
                    validityCheck = false;
                    invalidObjects.Add(questStatusUpdateChannel);
                }
            }
            
            if (questProgress == null)
            {
                if (isDebug) Debug.Log($"[QuestManager] {searching} {_}/QuestsProgressChannel in {searchLocation}", this);
                questProgress = Resources.Load<StringFloatEventChannelSO>($"{_}/QuestsProgressChannel");
                if (questProgress == null)
                {
                    errorMessages.Add($"{_}/QuestsProgressChannel is not {searchLocation} {readInstructions}");
                    validityCheck = false;
                    invalidObjects.Add(questProgress);
                }
            }
            
            if (questStatusUpdateRequested == null)
            {
                if (isDebug) Debug.Log($"[QuestManager] {searching} {_}/QuestRequirementCheck in {searchLocation}", this);
                questStatusUpdateRequested = Resources.Load<StringEventChannelSO>($"{_}/QuestRequirementCheck");
                if (questStatusUpdateRequested == null)
                {
                    errorMessages.Add($"{_}/QuestRequirementCheck is not {searchLocation} {readInstructions}");
                    validityCheck = false;
                    invalidObjects.Add(questStatusUpdateRequested);
                }
            }
            
            IsValid = validityCheck;
            if(!IsValid) return;

            if (IsValid && !Application.isPlaying) return;
            for(var i = 0 ; i < invalidObjects.Count ; i++)
            {
                Debug.LogError($"[QuestManager] Error: {errorMessages[i]} " , this.gameObject);
            }
        }
        public void OnValidate()
        {
            #if UNITY_EDITOR
            ValidityCheck();
            #endif
        }
        #endregion
        #endregion
    }
}