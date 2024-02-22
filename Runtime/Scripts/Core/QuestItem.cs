using System;
using System.Collections.Generic;
using jeanf.EventSystem;
using UnityEngine;
using UnityEngine.Serialization;
using jeanf.propertyDrawer;
using jeanf.validationTools;

namespace jeanf.questsystem
{
    public class QuestItem : MonoBehaviour, IDebugBehaviour, IValidatable
    {
        public bool isDebug
        {
            get => _isDebug;
            set => _isDebug = value;
        }
        public bool IsValid { get; private set; }

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
        [Header("Listening on:")] 
        [SerializeField] [Validation("A reference to the QuestProgress SO is required")] private StringFloatEventChannelSO QuestProgress;
        [SerializeField] [Validation("A reference to the StartQuestEventChannel SO is required")] private StringEventChannelSO StartQuestEventChannel;

        [Header("Broadcasting on:")] [SerializeField] [Validation("A reference to the QuestRequirementCheck SO is required")]
        private StringEventChannelSO requirementCheck;

        public void OnValidate()
        {
            #if UNITY_EDITOR
            ValididtyCheck();
            #endif
            
            questId = questInfoForPoint.id;
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
            
            if (QuestProgress == null)
            {
                if (isDebug) Debug.Log($"{searching} {_}/QuestsProgressChannel in {searchLocation} ", this);
                QuestProgress = Resources.Load<StringFloatEventChannelSO>($"{_}/QuestsProgressChannel");
                if (QuestProgress == null)
                {
                    errorMessages.Add($"{_}/QuestsProgressChannel is not in {searchLocation} {readInstructions}");
                    validityCheck = false;
                    invalidObjects.Add(QuestProgress);
                }

            }

            if (StartQuestEventChannel == null)
            {
                if (isDebug) Debug.Log($"{searching} {_}/StartQuestEventChannel in {searchLocation}", this);
                StartQuestEventChannel = Resources.Load<StringEventChannelSO>($"{_}/StartQuestEventChannel");
                if (StartQuestEventChannel == null)
                {
                    errorMessages.Add($"{_}/StartQuestEventChannel is not {searchLocation} {readInstructions}");
                    validityCheck = false;
                    invalidObjects.Add(StartQuestEventChannel);
                }
            }


            if (requirementCheck == null)
            {
                if (isDebug) Debug.Log($"{searching} {_}/QuestRequirementCheck in {searchLocation}", this);
                requirementCheck = Resources.Load<StringEventChannelSO>($"{_}/QuestRequirementCheck");
                if (requirementCheck == null)
                {
                    errorMessages.Add($"{_}/QuestRequirementCheck is not {searchLocation} {readInstructions}");
                    validityCheck = false;
                    invalidObjects.Add(requirementCheck);
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

        private void OnEnable()
        {
            Subscribe();

            requirementCheck.RaiseEvent(questId);
            if (!_startQuestOnEnable) return;
            RequestQuestStart(questId);
        }

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

            if (currentQuestState.Equals(QuestState.REQUIREMENTS_NOT_MET) && _startQuestOnEnable)
            {
                if(isDebug) Debug.Log($"forcing start of quest: {questId}");
                GameEventsManager.instance.questEvents.StartQuest(questId);
            }

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
            }
        }

        public void AllClear(bool value)
        {
            clearToStart = value;
            currentQuestState = QuestState.CAN_START;
            requirementCheck.RaiseEvent(questId);
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