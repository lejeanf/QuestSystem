using jeanf.EventSystem;
using jeanf.propertyDrawer ;
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


        [Header("Events")]
        [SerializeField] private StringEventChannelSO sendQuestStepTooltip;
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
            if (!isFinished)
            {
                isFinished = true;
                GameEventsManager.instance.questEvents.AdvanceQuest(questId);
                Destroy(this.gameObject);
                
                
                if (isUsingIntroTimeline && timeline)
                {
                    if(isDebug) Debug.Log($"sending trigger to timeline: {timeline.name}, triggerValue: false");
                    //_timelineTriggerEventChannelSo.RaiseEvent(timeline, false);
                }
            }
        }

        protected void ChangeState(string newState)
        {
            GameEventsManager.instance.questEvents.QuestStepStateChange(questId, stepIndex,
                new QuestStepState(newState));
        }

        protected void DisplayActiveQuestStep()
        {
            sendQuestStepTooltip.RaiseEvent("questStepToSend");

        }
        protected abstract void SetQuestStepState(string state);
        public bool isDebug { get; set; }
    }
}