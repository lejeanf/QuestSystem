using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using jeanf.propertyDrawer;

namespace jeanf.questsystem
{
    [CreateAssetMenu(fileName = "StepQuestRequirement", menuName = "Quests/QuestRequirementSO/StepQuestRequirement", order = 0)]
    [ScriptableObjectDrawer]
    public class StepQuestRequirement : QuestRequirementSO
    {
        QuestStep questStep;
        [SerializeField] QuestStepStatus requiredStatus;
        public override bool ValidateFulfilled()
        {
            if (requiredStatus == questStep.GetStatus())
            {
                return true;
            }
            return false;
            
        }
    }
}
