using UnityEngine;
using jeanf.propertyDrawer;

namespace jeanf.questsystem
{
    [CreateAssetMenu(fileName = "StepQuestRequirement", menuName = "Quests/QuestRequirementSO/StepQuestRequirement", order = 0)]
    [ScriptableObjectDrawer]
    public class StepQuestRequirement : QuestRequirementSO
    {
        QuestManager questManager;
        [SerializeField] QuestStep requiredStep;
        [SerializeField] QuestStepStatus requiredStatus;
        public override bool ValidateFulfilled()
        {
            questManager = FindObjectOfType<QuestManager>();
            if (questManager == null)
            {
                Debug.Log("questManager is null");
            }
            Quest quest = questManager.GetQuestById(requiredStep.QuestId);
            
            if (quest == null)
            {
                Debug.Log("Did not find any quest with this ID");
                return false;
            }

            //if (quest.GetQuestStepStatusById(requiredStep.StepId) == requiredStatus)
            //{
            //    Debug.Log("return true because status is the right one");
            //    return true;
            //}
            //else
            //{
            //    Debug.Log($"required status = {requiredStatus} && stepStatusInDictionnary = {quest.GetQuestStepStatusById(requiredStep.StepId)}");
            //}

            return false;            
        }
    }
}
