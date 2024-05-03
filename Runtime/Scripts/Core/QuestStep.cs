using jeanf.EventSystem;
using jeanf.propertyDrawer ;
using jeanf.tooltip;
using UnityEngine;
using UnityEngine.Playables;
using System;
namespace jeanf.questsystem
{
    public abstract class QuestStep : MonoBehaviour, IDebugBehaviour
    {

        [field: Space(10)][field: ReadOnly][SerializeField]  string stepId;
        string questId;
        public string StepId { get { return stepId; } }
        public string QuestId { get { return questId; } }
        private float questStepProgress = 0;
        [SerializeField] QuestStepStatus stepStatus;

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

        [Header("Game Objects to Trigger")]
        [SerializeField] QuestStep[] gameObjectsToTriggerOnStart;
        [SerializeField] QuestStep[] gameObjectsToTriggerOnEnd;
        [SerializeField] QuestRequirementSO[] questRequirementSOList;

        public void InitializeQuestStep(string questId)
        {
            stepStatus = QuestStepStatus.InProgress;
            questStepSender.Invoke(this);
            this.questId = questId;
            if (sendQuestStepTooltip != null)
            {
                DisplayActiveQuestStep();
            }
            if (gameObjectsToTriggerOnStart != null)
            {
                foreach (QuestStep questStep in gameObjectsToTriggerOnStart)
                {
                    if (questStep.ValidateRequirements())
                    {
                        Instantiate(questStep, questStep.transform.position, Quaternion.identity);
                        questStep.InitializeQuestStep(this.questId);
                    }
                }
            }
            if (isUsingIntroTimeline && timeline)
            {
                if(isDebug) Debug.Log($"sending trigger to timeline: {timeline.name}, triggerValue: true");
                _timelineTriggerEventChannelSo.RaiseEvent(timeline, true);
            }
            

        }

        protected void FinishQuestStep()
        {
            stepStatus = QuestStepStatus.Finished;
            questStepSender.Invoke(this);
            if (sendQuestStepTooltip != null)
            {
                sendQuestStepTooltip.RaiseEvent(string.Empty);
            }
            if(this.gameObject) Destroy(this.gameObject);

            if (gameObjectsToTriggerOnEnd != null)
            {
                foreach (QuestStep questStep in gameObjectsToTriggerOnEnd)
                {
                    if (questStep.ValidateRequirements())
                    {
                        Instantiate(questStep, questStep.transform.position, Quaternion.identity);
                        questStep.InitializeQuestStep(this.questId);
                    }
                    Debug.Log(questStep.ValidateRequirements());
                }
            }

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
        InProgress,
        Finished
    }
}