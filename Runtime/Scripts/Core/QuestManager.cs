using System;
using System.Collections;
using System.Collections.Generic;
using jeanf.EventSystem;
using jeanf.validationTools;
using UnityEngine;
using UnityEngine.Serialization;

namespace jeanf.questsystem
{
    public class QuestManager : MonoBehaviour, IDebugBehaviour, IValidatable
    {
        public bool isDebug
        {
            get => _isDebug;
            set => _isDebug = value;
        }
        public bool IsValid { get; private set; }

        [SerializeField] private bool _isDebug = false;

        [FormerlySerializedAs("loadQuestState")] [Header("Config")] [SerializeField]
        private bool loadSavedQuestState = true;

        [Header("Broadcasting on:")]
        [SerializeField] [Validation("A reference to the questStatusUpdateChannel is required.")] private StringEventChannelSO questStatusUpdateChannel;
        [SerializeField] [Validation("A reference to the questProgress is required.")] private StringFloatEventChannelSO questProgress;
        [SerializeField] [Validation("A reference to the questInitialCheck channel is required.")] private StringEventChannelSO QuestInitialCheck;

        [Header("Listening on:")] [SerializeField] [Validation("A reference to the questStatusUpdateRequested is required.")] private StringEventChannelSO questStatusUpdateRequested;

        private Dictionary<string, Quest> questMap;

        // quest start requirements
        private int currentPlayerLevel;

        private void Awake()
        {
            questMap = CreateQuestMap();

            foreach (var quest in questMap)
            {
                CheckIfQuestIsAlreadyLoaded(quest.Key);
            }
        }

        private void OnEnable()
        {
            GameEventsManager.instance.questEvents.onStartQuest += StartQuest;
            GameEventsManager.instance.questEvents.onAdvanceQuest += AdvanceQuest;
            GameEventsManager.instance.questEvents.onFinishQuest += FinishQuest;

            GameEventsManager.instance.questEvents.onQuestStepStateChange += QuestStepStateChange;

            GameEventsManager.instance.playerEvents.onPlayerLevelChange += PlayerLevelChange;

            questStatusUpdateRequested.OnEventRaised += ctx => CheckRequirementsMet(questMap[ctx]);
        }

        private void OnDisable()
        {
            GameEventsManager.instance.questEvents.onStartQuest -= StartQuest;
            GameEventsManager.instance.questEvents.onAdvanceQuest -= AdvanceQuest;
            GameEventsManager.instance.questEvents.onFinishQuest -= FinishQuest;

            GameEventsManager.instance.questEvents.onQuestStepStateChange -= QuestStepStateChange;

            GameEventsManager.instance.playerEvents.onPlayerLevelChange -= PlayerLevelChange;
        }

        private void Start()
        {

            foreach (Quest quest in questMap.Values)
            {
                // initialize any loaded quest steps
                if (quest.state == QuestState.IN_PROGRESS)
                {
                    quest.InstantiateCurrentQuestStep(this.transform);
                }

                // broadcast the initial state of all quests on startup
                GameEventsManager.instance.questEvents.QuestStateChange(quest);
            }
        }

        private void CheckIfQuestIsAlreadyLoaded(string id)
        {
            QuestInitialCheck.RaiseEvent(id);
        }

        private void ChangeQuestState(string id, QuestState state)
        {
            Quest quest = GetQuestById(id);
            quest.state = state;
            GameEventsManager.instance.questEvents.QuestStateChange(quest);
        }

        private void PlayerLevelChange(int level)
        {
            currentPlayerLevel = level;
        }

        private bool CheckRequirementsMet(Quest quest)
        {
            // check player level requirements
            var meetsRequirements = !(currentPlayerLevel < quest.info.levelRequirement);

            // check quest prerequisites for completion
            foreach (QuestInfoSO prerequisiteQuestInfo in quest.info.questPrerequisites)
            {
                if (GetQuestById(prerequisiteQuestInfo.id).state != QuestState.FINISHED)
                {
                    meetsRequirements = false;
                }
            }
            
            if(isDebug) Debug.Log($"checking requirements for quest: {quest.info.name}, [{quest.info.id}], meetsRequirements: {meetsRequirements}");

            return meetsRequirements;
        }

        private void Update()
        {
            // loop through ALL quests
            foreach (Quest quest in questMap.Values)
            {
                // if we're now meeting the requirements, switch over to the CAN_START state
                if (quest.state == QuestState.REQUIREMENTS_NOT_MET && CheckRequirementsMet(quest))
                {
                    ChangeQuestState(quest.info.id, QuestState.CAN_START);
                }
            }
        }

        private void StartQuest(string id)
        {
            Quest quest = GetQuestById(id);
            quest.InstantiateCurrentQuestStep(this.transform);
            ChangeQuestState(quest.info.id, QuestState.IN_PROGRESS);
            SaveQuest(quest);
            if (!quest.sendMessageOnInitialization) return;
            quest.messageChannel.RaiseEvent(quest.messageToSendOnInit);
            if(isDebug) Debug.Log($"quest id:{id}  started, a message was attatched to the initialization: {quest.messageToSendOnInit}");
        }

        private void AdvanceQuest(string id)
        {
            Quest quest = GetQuestById(id);

            // move on to the next step
            quest.MoveToNextStep();
            if (isDebug)
                Debug.Log(
                    $"[{quest.info.id}]quest state: {quest.state} - {quest.currentStep} over {quest.info.questStepPrefabs.Length} steps done",
                    this);
            questStatusUpdateChannel.RaiseEvent(
                $"[{quest.info.id}]quest state: {quest.state} - {quest.currentStep} over {quest.info.questStepPrefabs.Length} steps done");

            
            // if there are more steps, instantiate the next one
            if (quest.CurrentStepExists())
            {
                UpdateProgress(quest);
                quest.InstantiateCurrentQuestStep(this.transform);
            }
            // if there are no more steps, then we've finished all of them for this quest
            else
            {
                ChangeQuestState(quest.info.id, QuestState.CAN_FINISH);
            }

            SaveQuest(quest);
        }

        private void UpdateProgress(Quest quest)
        {
            var progress = (float)quest.currentStep / quest.info.questStepPrefabs.Length;
            if (quest.info.id == null) Debug.Log("C'est null");;
            if (isDebug) Debug.Log($"[{quest.info.id}] progress: {progress * 100}%", this);
            questProgress.RaiseEvent(quest.info.id, progress);
        }

        private void FinishQuest(string id)
        {
            Quest quest = GetQuestById(id);
            UpdateProgress(quest);
            ClaimRewards(quest);
            ChangeQuestState(quest.info.id, QuestState.FINISHED);
            questStatusUpdateChannel.RaiseEvent($"[{quest.info.id}] quest is finished.");
            questProgress.RaiseEvent(quest.info.id, 1);
            SaveQuest(quest);
            if (!quest.sendMessageOnFinish) return;
        }

        private void ClaimRewards(Quest quest)
        {
            GameEventsManager.instance.scenarioEvents.ScenarioUnlocked(quest.info.unlockedScenario);
        }

        private void QuestStepStateChange(string id, int stepIndex, QuestStepState questStepState)
        {
            Quest quest = GetQuestById(id);
            quest.StoreQuestStepState(questStepState, stepIndex);
            ChangeQuestState(id, quest.state);
        }

        private Dictionary<string, Quest> CreateQuestMap()
        {
            // loads all QuestInfoSO Scriptable Objects under the Assets/Resources/Quests folder
            QuestInfoSO[] allQuests = Resources.LoadAll<QuestInfoSO>("Quests");
            // Create the quest map
            Dictionary<string, Quest> questMap = new Dictionary<string, Quest>();
            foreach (QuestInfoSO questInfo in allQuests)
            {
                var id = questInfo.id;
                if (questMap.ContainsKey(id))
                {
                    Debug.LogWarning("Duplicate ID found when creating quest map: " + questInfo.id);
                }
                else
                {
                    questMap.Add(id, LoadQuest(questInfo));
                }
                if(isDebug) Debug.Log($"Adding {questInfo.name} to the questmap, its id is: {questInfo.id}");
            }

            return questMap;
        }

        private Quest GetQuestById(string id)
        {
            Quest quest = questMap[id];
            if (quest == null)
            {
                Debug.LogError("ID not found in the Quest Map: " + id);
            }

            return quest;
        }

        private void OnApplicationQuit()
        {
            foreach (Quest quest in questMap.Values)
            {
                SaveQuest(quest);
            }
        }

        private void SaveQuest(Quest quest)
        {
            try
            {
                QuestData questData = quest.GetQuestData();
                // serialize using JsonUtility, but use whatever you want here (like JSON.NET)
                string serializedData = JsonUtility.ToJson(questData);
                Debug.Log($"saved data {serializedData}");
                // saving to PlayerPrefs is just a quick example for this tutorial video,
                // you probably don't want to save this info there long-term.
                // instead, use an actual Save & Load system and write to a file, the cloud, etc..
                PlayerPrefs.SetString(quest.info.id, serializedData);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to save quest with id " + quest.info.id + ": " + e);
            }
        }

        private Quest LoadQuest(QuestInfoSO questInfo)
        {
            Quest quest = null;
            try
            {
                // load quest from saved data
                if (PlayerPrefs.HasKey(questInfo.id) && loadSavedQuestState)
                {
                    string serializedData = PlayerPrefs.GetString(questInfo.id);
                    QuestData questData = JsonUtility.FromJson<QuestData>(serializedData);
                    quest = new Quest(questInfo, questData.state, questData.questStepIndex, questData.questStepStates);
                }
                // otherwise, initialize a new quest
                else
                {
                    quest = new Quest(questInfo);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to load quest with id " + quest.info.id + ": " + e);
            }

            return quest;
        }
        private void ValididtyCheck()
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
                if (isDebug) Debug.Log($"{searching} {_}/QuestInitialCheck in {searchLocation}", this);
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
                if (isDebug) Debug.Log($"{searching} {_}/QuestStatusUpdate in {searchLocation}", this);
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
                if (isDebug) Debug.Log($"{searching} {_}/QuestsProgressChannel in {searchLocation}", this);
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
                if (isDebug) Debug.Log($"{searching} {_}/QuestRequirementCheck in {searchLocation}", this);
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
                Debug.LogError($"Error: {errorMessages[i]} " , this.gameObject);
            }
        }
        public void OnValidate()
        {
            #if UNITY_EDITOR
            ValididtyCheck();
            #endif
        }
    }
}