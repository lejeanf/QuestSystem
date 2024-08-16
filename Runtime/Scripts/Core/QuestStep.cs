using jeanf.EventSystem;
using jeanf.propertyDrawer;
using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;
using GraphProcessor;
using jeanf.validationTools;

namespace jeanf.questsystem
{
    [System.Serializable, NodeMenuItem("questSystem/QuestStep"), DefaultExecutionOrder(1)]
    public class QuestStep : MonoBehaviour, IDebugBehaviour
    {
        [field: Space(10)][field: ReadOnly][SerializeField] string stepId;
        public string StepId { get { return stepId; } }

        string questId;
        public string QuestId { get { return questId; } }

        
        [field: ReadOnly] [SerializeField] public QuestStepStatus stepStatus;

        [Tooltip("This boolean has to be enabled if the quest step has an intro timeline.")]
        public bool isUsingIntroTimeline = false;

        [DrawIf("isUsingIntroTimeline", true, ComparisonType.Equals, DisablingType.DontDraw)]
        [SerializeField] private TimelineTriggerEventChannelSO _timelineTriggerEventChannelSo;
        [DrawIf("isUsingIntroTimeline", true, ComparisonType.Equals, DisablingType.DontDraw)] 
        public PlayableAsset timeline;

        [Header("Quest Step Progression events & Variables")]
        public List<QuestStep> questStepsToTrigger = new List<QuestStep>();
        public delegate void SendNextStepId(string id);
        public static SendNextStepId sendNextStepId;

        public delegate void StepCompleted(string id);
        public static StepCompleted stepCompleted;
        public delegate void StepActive(string id, QuestStepStatus stepStatus);
        public static StepActive stepActive;
        public delegate void ChildStep(QuestStep step);
        public static ChildStep childStep;
        
        [Header("Quest Tooltip")]
        [SerializeField] private QuestTooltipSO questTooltipSO;

        [Header("Event Channels")]
        [SerializeField] private StringEventChannelSO sendQuestStepTooltip;




        public void OnEnable()
        {
            InitializeQuestStep();
            Debug.Log("Played questStep's enable");
        }


        public void InitializeQuestStep()
        {
            // failsafe to avoid lauching the same step more than once at a time.
            if (stepStatus == QuestStepStatus.Active) return;
            
            if(isDebug) Debug.Log($"Initializing questStep [{stepId}] with for quest with questId: [{questId}]");

            stepStatus = QuestStepStatus.Active;
            stepActive?.Invoke(stepId, stepStatus);

            
            if (sendQuestStepTooltip != null)
            {
                DisplayActiveQuestStep();
            }
            if (isUsingIntroTimeline && timeline)
            {
                if(isDebug) Debug.Log($"sending trigger to timeline: {timeline.name}, triggerValue: true");
                _timelineTriggerEventChannelSo.RaiseEvent(timeline, true);
            }

            if(isDebug) Debug.Log($"Step with id [{stepId}] has {questStepsToTrigger.Count} childSteps");
            foreach(QuestStep questStep in questStepsToTrigger)
            {
                if(isDebug) Debug.Log($"sending childstep to initialization: {questStep.name}, stepId: [{questStep.stepId}]");
                childStep?.Invoke(questStep);
            }
        }



        protected void FinishQuestStep()
        {
            if(isDebug) Debug.Log($" ---- Step with id: {stepId} finished. Changing status to completed", this);
            stepStatus = QuestStepStatus.Completed;
  
            if (sendQuestStepTooltip != null)
            {
                if(isDebug) Debug.Log($" ---- Step with id: {stepId} finished. Sending tooltip", this);
                sendQuestStepTooltip.RaiseEvent(string.Empty);
            }
;

            foreach(QuestStep questStep in questStepsToTrigger)
            {
                if(isDebug) Debug.Log($" ---- Step with id: {stepId} finished. Requesting to start next step: {questStep.stepId}", this);
                sendNextStepId?.Invoke(questStep.stepId);
            }
            if(isDebug) Debug.Log($" ---- Step with id: {stepId} finished. Sending stepCompleted Event (delegate) with argument: {stepId}", this);
            stepCompleted?.Invoke(stepId);
            if(isDebug) Debug.Log($" ---- Step with id: {stepId} finished. Sending stepActive Event (delegate) with arguments: {stepId}, {stepStatus} ", this);
            stepActive?.Invoke(stepId, stepStatus);

            if(isDebug) Debug.Log($" ---- Step with id: {stepId} finished. Destroying the gameobject with name {this.name}", this);
            Destroy(this.gameObject);
        }


        protected void DisplayActiveQuestStep()
        {
            if (questTooltipSO != null)
            {
                sendQuestStepTooltip.RaiseEvent(questTooltipSO.Tooltip);
            }
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (stepId == string.Empty || stepId == null) GenerateId();
        }

        public void GenerateId()
        {
            stepId = $"{System.Guid.NewGuid()}";
            UnityEditor.EditorUtility.SetDirty(this);
        }

        #endif


        public bool isDebug { get => _isDebug; set => _isDebug = value; }
        private bool _isDebug = true;

        public QuestStepStatus GetStatus()
        {
            return stepStatus;
        }
    }

    public enum QuestStepStatus
    {
        Inactive,
        Active,
        Completed
    }
}