using jeanf.EventSystem;
using jeanf.propertyDrawer ;
using UnityEngine;
using UnityEngine.Playables;

namespace jeanf.questsystem
{
    public abstract class QuestStep : MonoBehaviour
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
        [SerializeField] private PlayableAsset timeline;


        public void InitializeQuestStep(string questId, int stepIndex, string questStepState)
        {
            this.questId = questId;
            this.stepIndex = stepIndex;
            if (questStepState != null && questStepState != "")
            {
                SetQuestStepState(questStepState);
            }

            if (isUsingIntroTimeline && timeline)
            {
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
                    _timelineTriggerEventChannelSo.RaiseEvent(timeline, false);
                }
            }
        }

        protected void ChangeState(string newState)
        {
            GameEventsManager.instance.questEvents.QuestStepStateChange(questId, stepIndex,
                new QuestStepState(newState));
        }

        protected abstract void SetQuestStepState(string state);
    }
}