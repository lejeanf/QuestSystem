using System;
using System.Collections;
using System.Collections.Generic;
using jeanf.EventSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace jeanf.questsystem
{
    public class QuestManager : MonoBehaviour, IDebugBehaviour
    {
        public bool isDebug
        {
            get => _isDebug;
            set => _isDebug = value;
        }

        [SerializeField] private bool _isDebug = false;

        [FormerlySerializedAs("loadQuestState")] [Header("Config")] [SerializeField]
        private bool loadSavedQuestState = true;

        [Header("Broadcasting on:")] [SerializeField]
        private StringEventChannelSO questStatusUpdateChannel;
        private StringFloatEventChannelSO questProgress;

        private Dictionary<string, Quest> questMap;

        // quest start requirements
        private int currentPlayerLevel;

        private void Awake()
        {
            questMap = CreateQuestMap();
        }

        private void OnEnable()
        {
            GameEventsManager.instance.questEvents.onStartQuest += StartQuest;
            GameEventsManager.instance.questEvents.onAdvanceQuest += AdvanceQuest;
            GameEventsManager.instance.questEvents.onFinishQuest += FinishQuest;

            GameEventsManager.instance.questEvents.onQuestStepStateChange += QuestStepStateChange;

            GameEventsManager.instance.playerEvents.onPlayerLevelChange += PlayerLevelChange;
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
            Dictionary<string, Quest> idToQuestMap = new Dictionary<string, Quest>();
            foreach (QuestInfoSO questInfo in allQuests)
            {
                if (idToQuestMap.ContainsKey(questInfo.id))
                {
                    Debug.LogWarning("Duplicate ID found when creating quest map: " + questInfo.id);
                }

                idToQuestMap.Add(questInfo.id, LoadQuest(questInfo));
            }

            return idToQuestMap;
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
    }
}