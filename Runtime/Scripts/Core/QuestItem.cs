using System;
using System.Collections.Generic;
using jeanf.EventSystem;
using UnityEngine;
using jeanf.propertyDrawer;
using jeanf.validationTools;
using UnityEditor;
using System.Linq;


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

        [Tooltip("Visual feedback for the quest state")] [Header("Quest")] 
        [SerializeField] [Validation("A reference to a questSO is required")] private QuestSO questSO;
        private Dictionary<string, QuestStep> stepMap = new Dictionary<string, QuestStep>();
        private Dictionary<string, QuestStep> activeSteps = new Dictionary<string, QuestStep>();
        private Dictionary<string, QuestStep> completedSteps = new Dictionary<string, QuestStep>();
        private List<QuestStep> rootSteps = new List<QuestStep>();


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
        [SerializeField] [Validation("A reference to the QuestInitialCheck SO is required")] private StringEventChannelSO QuestInitialCheck;

        [Header("Broadcasting on:")] [SerializeField] [Validation("A reference to the QuestRequirementCheck SO is required")]
        private StringEventChannelSO requirementCheck;
        [SerializeField][Validation("A reference to the LoadRequiredScenesEventChannel SO is required")]  StringListEventChannelSO loadRequiredScenesEventChannel;
        [SerializeField] IntEventChannelSO unlockDoorsEventChannel;

        #region Awake/Enable/Disable
        private void Awake()
        {
            questId = questSO.id;
            for (int i = 0; i < questSO.rootSteps.Length; i++)
            {
                Debug.Log($"id on awake {questSO.rootSteps[i].StepId}, added to {this.name}'s dictionary", this);
                AddStepToStepMap(questSO.rootSteps[i]);
                rootSteps.Add(questSO.rootSteps[i]);
            }
        }

        private void OnEnable()
        {
            Subscribe();

            Init(questId);
        }
        private void OnDisable() => Unsubscribe();
        private void OnDestroy() => Unsubscribe();

        private void Subscribe()
        {
            QuestInitialCheck.OnEventRaised += Init;
            QuestProgress.OnEventRaised += UpdateProgress;
            GameEventsManager.instance.questEvents.onQuestStateChange += QuestStateChange;
            GameEventsManager.instance.inputEvents.onSubmitPressed += UpdateState;
            QuestStep.sendNextStepId += InstantiateQuestStep;
            //QuestStep.stepCompleted += DestroyQuestStep;
            QuestStep.stepActive += UpdateStepStatus;
            QuestStep.childStep += AddStepToStepMap;

        }

        private void Unsubscribe()
        {
            QuestInitialCheck.OnEventRaised -= Init;
            QuestProgress.OnEventRaised -= UpdateProgress;
            GameEventsManager.instance.questEvents.onQuestStateChange -= QuestStateChange;
            GameEventsManager.instance.inputEvents.onSubmitPressed -= UpdateState;
            QuestStep.sendNextStepId -= InstantiateQuestStep;
            //QuestStep.stepCompleted -= DestroyQuestStep;
            QuestStep.stepActive -= UpdateStepStatus;
            QuestStep.childStep -= AddStepToStepMap;


        }
        #endregion

        #region Instantiations & Loading
        public void InstantiateQuestStep(string id)
        {
            if (!stepMap.ContainsKey(id)) return;
            if (stepMap[id].stepStatus != QuestStepStatus.Inactive) return;
            if (activeSteps.ContainsKey(id)) return;

            Instantiate(stepMap[id], this.transform, true);
            //if(!activeSteps.ContainsKey(id)) activeSteps.Add(id,Instantiate(stepMap[id], this.transform, true));
        }

        private void LoadDependencies()
        {
            loadRequiredScenesEventChannel.RaiseEvent(questSO.ScenesToLoad);

            foreach (int roomToUnlock in questSO.roomsToUnlock)
            {
                unlockDoorsEventChannel.RaiseEvent(roomToUnlock);
            }
        }
        #endregion

        public void UpdateStepStatus(string id, QuestStepStatus status)
        {
            switch (status)
            {
                case QuestStepStatus.Completed when activeSteps.ContainsKey(id):
                    // put step in completed list
                    activeSteps.Remove(id);
                    completedSteps.Add(id, stepMap[id]);
                    break;
                case QuestStepStatus.Active when !activeSteps.ContainsKey(id):
                    // put step in active list
                    activeSteps.Add(id, stepMap[id]);
                    break;
                case QuestStepStatus.Inactive:
                    // do nothing
                default:
                    // do nothing
                    return;
            }
        }

        private void AddStepToStepMap(QuestStep step)
        {
            if (!stepMap.ContainsKey(step.StepId))
            {
                stepMap.Add(step.StepId, step);
            }
            if (completedSteps.ContainsKey(step.StepId))
            {
                completedSteps.Remove(step.StepId);
            }
        }
        #region quest process
        private void Init(string id)
        {
            activeSteps.Clear();
            activeSteps.TrimExcess();
            completedSteps.Clear();
            completedSteps.TrimExcess();

            Debug.Log($"Quest [{id}]: _startQuestOnEnable value is: [{_startQuestOnEnable}]");
            if (!_startQuestOnEnable || id != questId) return;
            clearToStart = true;
            currentQuestState = QuestState.CAN_START;
            requirementCheck.RaiseEvent(questId);
            UpdateState();

            foreach (QuestStep step in rootSteps)
            {
                InstantiateQuestStep(step.StepId);
            }

            LoadDependencies();
        }

        private void UpdateState()
        {
            if (isDebug) Debug.Log($"Updating State...");
            if (!clearToStart) return;
            if (isDebug) Debug.Log($"All is clear, continuing ...");

            switch (currentQuestState)
            {
                case QuestState.CAN_START:
                {
                    if (isDebug) Debug.Log($"Starting quest: {questId}");
                    GameEventsManager.instance.questEvents.StartQuest(questId);
                    break;
                }
                case QuestState.CAN_FINISH:
                {
                    if (isDebug) Debug.Log($"Finishing quest: {questId}");
                    GameEventsManager.instance.questEvents.FinishQuest(questId);
                    break;
                }
                case QuestState.REQUIREMENTS_NOT_MET:
                    if(_startQuestOnEnable)
                    {
                        if(isDebug) Debug.Log($"forcing start of quest: {questId}");
                        GameEventsManager.instance.questEvents.StartQuest(questId);
                    }
                    break;
                case QuestState.IN_PROGRESS:
                    break;
                case QuestState.FINISHED:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateProgress(string id, float progress)
        {
            if (id != questId) return;
            this.progress = progress;
            if (isDebug) Debug.Log($"questid [{id}] progress = {progress * 100}%");
        }

        private void QuestStateChange(Quest quest)
        {
            // only update the quest state if this point has the corresponding quest
            if (quest.questSO.id.Equals(questId))
            {
                currentQuestState = quest.state;
                UpdateState();
            }
        }
        #endregion

        #region validation tools

        #if UNITY_EDITOR
        public void OnValidate()
        {
            ValidityCheck();
        }


        

        public void LogActiveSteps()
        {
            Debug.Log($"There is {activeSteps.Count} active steps at the moment.");
            foreach (var step in activeSteps.Keys)
            {
                Debug.Log($"active step: {step}");
            }
        }
        #endif

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
                if (isDebug) Debug.Log($"{searching} {_}/QuestInitialCheck in {searchLocation} ", this);
                QuestInitialCheck = Resources.Load<StringEventChannelSO>($"{_}/QuestInitialCheck");
                if (QuestInitialCheck == null)
                {
                    errorMessages.Add($"{_}/QuestInitialCheck is not in {searchLocation} {readInstructions}");
                    validityCheck = false;
                    invalidObjects.Add(QuestInitialCheck);
                }

            }
            
            if (questSO == null)
            {
                if (isDebug) Debug.Log($"There is no questSO in the questItem");
                validityCheck = false;
                invalidObjects.Add(questSO);
            }


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

            if (loadRequiredScenesEventChannel == null)
            {
                if (isDebug) Debug.Log($"{searching} {_}/loadRequiredScenesEventChannel in {searchLocation} ", this);
                loadRequiredScenesEventChannel = Resources.Load<StringListEventChannelSO>($"{_}/LoadRequiredScenesEventChannel");
                if (loadRequiredScenesEventChannel == null)
                {
                    errorMessages.Add($"{_}/loadRequiredScenesEventChannel is not in {searchLocation} {readInstructions}");
                    validityCheck = false;
                    invalidObjects.Add(loadRequiredScenesEventChannel);
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

            if (!IsValid) return;
            if (IsValid && !Application.isPlaying) return;

            for (var i = 0; i < invalidObjects.Count; i++)
            {
                Debug.LogError($"Error: {errorMessages[i]} ", this.gameObject);
            }
        }
        #endregion
    }
    
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(QuestItem))]
    public class BoolEventOnClickEditor : Editor {
        override public void  OnInspectorGUI () {
            DrawDefaultInspector();
            GUILayout.Space(10);
            var eventToSend = (QuestItem) target;
            if(GUILayout.Button("Log active steps", GUILayout.Height(30))) {
                eventToSend.LogActiveSteps(); // how do i call this?
            }
            GUILayout.Space(10);
        }
    }
    #endif
}