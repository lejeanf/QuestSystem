using System.Collections;
using System.Collections.Generic;
using jeanf.EventSystem;
using jeanf.propertyDrawer ;
using UnityEngine;
using jeanf.propertyDrawer;
using UnityEngine.Events;

namespace jeanf.questsystem
{
    public abstract class QuestStep : MonoBehaviour
    {
        private bool isFinished = false;
        private string questId;
        private int stepIndex;
        private float questStepProgress = 0;


        public void InitializeQuestStep(string questId, int stepIndex, string questStepState)
        {
            this.questId = questId;
            this.stepIndex = stepIndex;
            if (questStepState != null && questStepState != "")
            {
                SetQuestStepState(questStepState);
            }
        }

        protected void FinishQuestStep()
        {
            if (!isFinished)
            {
                isFinished = true;
                GameEventsManager.instance.questEvents.AdvanceQuest(questId);
                Destroy(this.gameObject);
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