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
    public class QuestStep : BaseNode, IDebugBehaviour
    {
        public override string        name => "QuestStep";
        public GameObject PrefabToInstantiate;
        public string StepId { get { return stepId; } }
        [field: Space(10)][field: ReadOnly][SerializeField]  string stepId;
        public string QuestId { get { return questId; } }
        string questId;
        
        private float questStepProgress = 0;
        
        [field: ReadOnly] [SerializeField] public QuestStepStatus stepStatus;

        [Tooltip("This boolean has to be enabled if the quest step has an intro timeline.")]
        public bool isUsingIntroTimeline = false;

        [DrawIf("isUsingIntroTimeline", true, ComparisonType.Equals, DisablingType.DontDraw)]
        [SerializeField] private TimelineTriggerEventChannelSO _timelineTriggerEventChannelSo;
        [DrawIf("isUsingIntroTimeline", true, ComparisonType.Equals, DisablingType.DontDraw)] 
        public PlayableAsset timeline;


        [Header("Event Channels")]
        [SerializeField] private StringEventChannelSO sendQuestStepTooltip;
        public static event Action<QuestStep> questStepSender;

        [Header("Quest Tooltip")]
        [SerializeField] private QuestTooltipSO questTooltipSO;

        [SerializeField] QuestRequirementSO[] questRequirementSOList;

        [Input(name = "TriggeredBy", allowMultiple = true)]
        public ConditionalLink input;
        
        [Output(name = "TriggersNext", allowMultiple = true)]
        public ConditionalLink output;

        protected override void Process() => InitializeQuestStep();

        public void InitializeQuestStep()
        {
            InitializeQuestStep(QuestId);
        }

        public void InitializeQuestStep(string questId)
        {
            // failsafe to avoid lauching the same step more than once at a time.
            if(stepStatus != QuestStepStatus.Active) return;
            Debug.Log($"Initializing quest with questId: {questId} and nodeId: {GUID}");
            
            stepStatus = QuestStepStatus.Active;
            questStepSender.Invoke(this);
            this.questId = questId;
            if (sendQuestStepTooltip != null)
            {
                DisplayActiveQuestStep();
            }
            if (isUsingIntroTimeline && timeline)
            {
                if(isDebug) Debug.Log($"sending trigger to timeline: {timeline.name}, triggerValue: true");
                _timelineTriggerEventChannelSo.RaiseEvent(timeline, true);
            }
            

        }

        protected void FinishQuestStep()
        {
            stepStatus = QuestStepStatus.Completed;
            questStepSender.Invoke(this);
            if (sendQuestStepTooltip != null)
            {
                sendQuestStepTooltip.RaiseEvent(string.Empty);
            }
            //if(this.gameObject) Destroy(this.gameObject);
            Debug.LogWarning("Implement prefab destruction");
            Debug.LogWarning("Implement next trigger calls.");

            foreach (var outputPort in outputPorts)
            {
                var ownerGuid = outputPort.owner.GUID;
                Debug.Log($"output port ownerGuid: {ownerGuid}");
                outputPort.PushData();
            }
            /*
            if (gameObjectsToTriggerOnEnd != null)
            {
                foreach (QuestStep questStep in gameObjectsToTriggerOnEnd)
                {
                    if (questStep.ValidateRequirements())
                    {
                        //Instantiate(questStep, questStep.transform.position, Quaternion.identity);
                        Debug.LogWarning("Implement next triger instanciation");
                        questStep.InitializeQuestStep(this.questId);
                    }
                    Debug.Log(questStep.ValidateRequirements());
                }
            }
            */

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
            //UnityEditor.EditorUtility.SetDirty(this);
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