using System;
using jeanf.EventSystem;
using UnityEngine;
using UnityEngine.Serialization;
using jeanf.propertyDrawer;

namespace jeanf.questsystem
{
    public class QuestItem : MonoBehaviour, IDebugBehaviour
    {
        public bool isDebug
        {
            get => _isDebug;
            set => _isDebug = value;
        }

        [SerializeField] private bool _isDebug = false;
        [SerializeField] private bool _startQuestOnEnable = false;

        [Tooltip("Visual feedback for the quest state")] [Header("Quest")] [SerializeField]
        private QuestInfoSO questInfoForPoint;

        [ReadOnly] [Range(0, 1)] [SerializeField]
        private float progress = 0.0f;

        [SerializeField] [ReadOnly]
        private bool clearToStart = false;

        private string questId;
        private QuestState currentQuestState;

        // these events are Located in Assets/Resources/Quests/Channels - it is searched for at Awake time.
        // if they do not exist simply right click in the hierarchy and find >InitializeQuestSystem<
        private StringFloatEventChannelSO QuestProgress;
        private StringEventChannelSO StartQuestEventChannel;

        public void OnValidate()
        {
            const string searching = "attempting to find";
            const string _ = "Quests/Channels"; // search target
            const string searchLocation = "the resources folder";
            const string readInstructions = "please read the package instruction for further help";

            if (QuestProgress == null)
            {
                if (isDebug) Debug.Log($"{searching} {_}/QuestsProgressChannel in {searchLocation} ", this);
                QuestProgress = Resources.Load<StringFloatEventChannelSO>($"{_}/QuestsProgressChannel");
                if (QuestProgress == null)
                    Debug.LogError($"{_}/QuestsProgressChannel is not in {searchLocation} {readInstructions}", this);
            }

            if (StartQuestEventChannel == null)
            {
                if (isDebug) Debug.Log($"{searching} {_}/StartQuestEventChannel in {searchLocation}", this);
                StartQuestEventChannel = Resources.Load<StringEventChannelSO>($"{_}/StartQuestEventChannel");
                if (QuestProgress == null)
                    Debug.LogError($"{_}/StartQuestEventChannel is not {searchLocation} {readInstructions}", this);
            }

            questId = questInfoForPoint.id;
        }

        private void OnEnable() => Subscribe();
        private void OnDisable() => Unsubscribe();
        private void OnDestroy() => Unsubscribe();

        private void Subscribe()
        {
            StartQuestEventChannel.OnEventRaised += RequestQuestStart;
            QuestProgress.OnEventRaised += UpdateProgress;
            GameEventsManager.instance.questEvents.onQuestStateChange += QuestStateChange;
            GameEventsManager.instance.inputEvents.onSubmitPressed += UpdateState;
        }

        private void Unsubscribe()
        {
            StartQuestEventChannel.OnEventRaised -= RequestQuestStart;
            QuestProgress.OnEventRaised -= UpdateProgress;
            GameEventsManager.instance.questEvents.onQuestStateChange -= QuestStateChange;
            GameEventsManager.instance.inputEvents.onSubmitPressed -= UpdateState;
        }

        public void UpdateState()
        {
            if (isDebug) Debug.Log($"Updating State...");
            if (!clearToStart)
            {
                return;
            }

            if (isDebug) Debug.Log($"All is clear, continuing ...");

            // start or finish a quest
            if (currentQuestState.Equals(QuestState.CAN_START))
            {
                if (isDebug) Debug.Log($"Starting quest: {questId}");
                GameEventsManager.instance.questEvents.StartQuest(questId);
            }
            else if (currentQuestState.Equals(QuestState.CAN_FINISH))
            {
                if (isDebug) Debug.Log($"Finishing quest: {questId}");
                GameEventsManager.instance.questEvents.FinishQuest(questId);
            }
        }

        private void UpdateProgress(string id, float progress)
        {
            if (id == questId)
            {
                this.progress = progress;
                if (isDebug) Debug.Log($"questid [{id}] progress = {progress * 100}%");
            }
        }

        private void QuestStateChange(Quest quest)
        {
            // only update the quest state if this point has the corresponding quest
            if (quest.info.id.Equals(questId))
            {
                currentQuestState = quest.state;
                
                // start on enable option
                if (_startQuestOnEnable && quest.state == QuestState.CAN_START) RequestQuestStart(quest.info.id);
            }
        }

        public void AllClear(bool value)
        {
            clearToStart = value;
            UpdateState();
        }

        public void RequestQuestStart(string id)
        {
            if(id!= questId) return;
            AllClear(true);
            if(isDebug) Debug.Log($"Quest start was requested for quest {id}.", this);
        }
    }
}