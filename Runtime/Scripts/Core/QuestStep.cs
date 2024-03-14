using jeanf.EventSystem;
using jeanf.propertyDrawer ;
using jeanf.tooltip;
using UnityEngine;
using UnityEngine.Playables;

namespace jeanf.questsystem
{
    public abstract class QuestStep : MonoBehaviour, IDebugBehaviour
    {
        private bool isFinished = false;
        private string questId;
        private int stepIndex;
        private float questStepProgress = 0;
        
        [Tooltip("This boolean has to be enabled if the quest step has an intro timeline.")]
        public bool isUsingIntroTimeline = false;

        [DrawIf("isUsingIntroTimeline", true, ComparisonType.Equals, DisablingType.DontDraw)]
        [SerializeField] private TimelineTriggerEventChannelSO _timelineTriggerEventChannelSo;
        [DrawIf("isUsingIntroTimeline", true, ComparisonType.Equals, DisablingType.DontDraw)] 
        public PlayableAsset timeline;


        [Header("Event Channels")]
        [SerializeField] private StringEventChannelSO sendQuestStepTooltip;

        [Header("Quest Tooltip")]
        [SerializeField] private QuestTooltipSO questTooltipSO;

        public void InitializeQuestStep(string questId, int stepIndex, string questStepState)
        {
            this.questId = questId;
            this.stepIndex = stepIndex;
            if (questStepState != null && questStepState != "")
            {
                SetQuestStepState(questStepState);
                
            }
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
            if (isFinished) return;
            isFinished = true;
            if(questId != null) GameEventsManager.instance.questEvents.AdvanceQuest(questId);
            if(this.gameObject) Destroy(this.gameObject);


            if (!isUsingIntroTimeline || !timeline) return;
            //if(isDebug) Debug.Log($"sending trigger to timeline: {timeline.name}, triggerValue: false");
            //_timelineTriggerEventChannelSo.RaiseEvent(timeline, false);
        }

        protected void ChangeState(string newState)
        {
            GameEventsManager.instance.questEvents.QuestStepStateChange(questId, stepIndex,
                new QuestStepState(newState));
        }

        protected void DisplayActiveQuestStep()
        {
            if (questTooltipSO != null)
            {
                sendQuestStepTooltip.RaiseEvent(questTooltipSO.Tooltip);
            }
        }
        protected abstract void SetQuestStepState(string state);
        public bool isDebug { get; set; }
    }
}