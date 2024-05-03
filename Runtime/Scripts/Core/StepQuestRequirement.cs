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
                return false;
                Debug.Log("Did not find any quest with this ID");

            }

            if (quest.GetQuestStepStatusById(requiredStep.StepId) == requiredStatus)
            {
                return true;
                Debug.Log("return true because status is the right one");
            }
            else
            {
                Debug.Log($"required status = {requiredStatus} && stepStatusInDictionnary = {quest.GetQuestStepStatusById(requiredStep.StepId)}");
            }
            //Get le QuestId du requiredStep
            //Check dans le QuestManager pour get la quest dont l'id correspond -> comment on get le QuestManager ici though ?
            //Check dans la quête si y'a un questStep avec l'id du requiredStep dans la map
            //Check, si on a trouvé un questStep avec le bon id, si son status est le bon
            //Return true or false selon le résultat
            return false;            
        }
    }
}
