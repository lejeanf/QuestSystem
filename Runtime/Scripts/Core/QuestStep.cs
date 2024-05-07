using jeanf.EventSystem;
using jeanf.propertyDrawer ;
using jeanf.tooltip;
using UnityEngine;
using UnityEngine.Playables;
using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using NodeGraphProcessor.Examples;

namespace jeanf.questsystem
{
    [System.Serializable, NodeMenuItem("questSystem/QuestStep")]
    public class QuestStep : MonoBehaviour, IDebugBehaviour
    {

        public GameObject PrefabToInstantiate;
        [field: Space(10)][field: ReadOnly][SerializeField] string stepId;
        public string StepId { get { return stepId; } }

        string questId;
        public string QuestId { get { return questId; } }

        
        private float questStepProgress = 0;
        
        [field: ReadOnly] [SerializeField] public QuestStepStatus stepStatus;

        [Tooltip("This boolean has to be enabled if the quest step has an intro timeline.")]
        public bool isUsingIntroTimeline = false;

        [DrawIf("isUsingIntroTimeline", true, ComparisonType.Equals, DisablingType.DontDraw)]
        [SerializeField] private TimelineTriggerEventChannelSO _timelineTriggerEventChannelSo;
        [DrawIf("isUsingIntroTimeline", true, ComparisonType.Equals, DisablingType.DontDraw)] 
        public PlayableAsset timeline;

        public List<QuestStep> questStepsToTrigger = new List<QuestStep>();
        public delegate void SendNextStepId(string id);
        public static SendNextStepId sendNextStepId;
        public bool isRootStep;

        [Header("Event Channels")]
        [SerializeField] private StringEventChannelSO sendQuestStepTooltip;

        public delegate void StepCompleted(string id);
        public static StepCompleted stepCompleted;
        [Header("Quest Tooltip")]
        [SerializeField] private QuestTooltipSO questTooltipSO;

        [SerializeField] QuestRequirementSO[] questRequirementSOList;


        private void OnEnable()
        {
            InitializeQuestStep();
        }


        public void InitializeQuestStep()
        {
            // failsafe to avoid lauching the same step more than once at a time.
            if (stepStatus != QuestStepStatus.Active) return;
            Debug.Log($"Initializing quest with questId: {questId}");

            stepStatus = QuestStepStatus.Active;


            if (sendQuestStepTooltip != null)
            {
                DisplayActiveQuestStep();
            }
            if (isUsingIntroTimeline && timeline)
            {
                if (isDebug) Debug.Log($"sending trigger to timeline: {timeline.name}, triggerValue: true");
                _timelineTriggerEventChannelSo.RaiseEvent(timeline, true);
            }
        }



        protected void FinishQuestStep()
        {
            stepStatus = QuestStepStatus.Completed;
  
            if (sendQuestStepTooltip != null)
            {
                sendQuestStepTooltip.RaiseEvent(string.Empty);
            }

            Debug.LogWarning("Implement prefab destruction");
            Debug.LogWarning("Implement next trigger calls.");

            

            foreach(QuestStep questStep in questStepsToTrigger)
            {
                sendNextStepId?.Invoke(questStep.stepId);
            }

            stepCompleted?.Invoke(stepId);

            if (!isUsingIntroTimeline || !timeline) return;
            //if(isDebug) Debug.Log($"sending trigger to timeline: {timeline.name}, triggerValue: false");
            //_timelineTriggerEventChannelSo.RaiseEvent(timeline, false);

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

        public bool isDebug { get; set; }

        public QuestStepStatus GetStatus()
        {
            return stepStatus;
        }

        public bool ValidateRequirements()
        {
            foreach(QuestRequirementSO requirement in questRequirementSOList)
            {
                if (!requirement.ValidateFulfilled())
                {
                    return false;
                }
            }
            return true;
        }
        
    }

    public enum QuestStepStatus
    {
        Inactive,
        Active,
        Completed
    }
}