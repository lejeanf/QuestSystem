using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jeanf.questsystem
{
    public class MultiQuestStep : QuestStep
    {
        [SerializeField] List<QuestStep> steps = new List<QuestStep>();
        [SerializeField] Dictionary<string, QuestStep> stepsDictionary = new Dictionary<string, QuestStep>();
        public override void OnEnable()
        {
            base.OnEnable();
            foreach(QuestStep step in steps)
            {
                childStep.Invoke(step);
                sendNextStepId?.Invoke(step.StepId);
                stepsDictionary.Add(step.StepId, step);
            }

            // Named handler: unsubscribing a fresh lambda never detaches.
            stepCompleted += RemoveStepFromBundle;
        }

        protected override void Unsubscribe()
        {
            base.Unsubscribe();
            stepCompleted -= RemoveStepFromBundle;
        }

        private void RemoveStepFromBundle(string stepId)
        {
            if (stepsDictionary.ContainsKey(stepId))
            {
                stepsDictionary.Remove(stepId);

                if (stepsDictionary.Count <= 0)
                {
                    FinishQuestStep();
                }
            }
        }

    }
}
