using System.Collections;
using System.Collections.Generic;
using jeanf.EventSystem;
using jeanf.propertyDrawer;
using UnityEngine;

namespace jeanf.questsystem
{
    public class Quest
    {
        // static info
        public QuestSO questSO;

        // state info
        public QuestState state;
        private QuestStepState[] questStepStates;
        public StringEventChannelSO messageChannel;
        public bool sendMessageOnInitialization = false;
        public string messageToSendOnInit = "";
        public bool sendMessageOnFinish = false;
        public string messageToSendOnFinish = "";
        Dictionary<string, QuestStepStatus> stepList = new Dictionary<string, QuestStepStatus>();
        
        public Quest(QuestSO questQuestSo)
        {
            this.questSO = questQuestSo;
            this.state = QuestState.REQUIREMENTS_NOT_MET;
            this.questStepStates = new QuestStepState[questSO.questStepPrefabs.Length];
            this.messageChannel = questQuestSo.messageChannel;
            //init
            this.sendMessageOnInitialization = questQuestSo.sendMessageOnInitialization;
            this.messageToSendOnInit = questQuestSo.messageToSendOnInitialization;
            //finish
            this.sendMessageOnFinish = questQuestSo.sendMessageOnFinish;
            this.messageToSendOnFinish = questQuestSo.messageToSendOnFinish;
            for (int i = 0; i < questStepStates.Length; i++)
            {
                questStepStates[i] = new QuestStepState();
            }
            QuestStep.questStepSender += questStep => AddToQuestStepMap(questStep);
        }

        public Quest(QuestSO questQuestSo, QuestState questState, int currentQuestStepIndex,
            QuestStepState[] questStepStates)
        {
            this.questSO = questQuestSo;
            this.state = questState;
            this.questStepStates = questStepStates;

            // if the quest step states and prefabs are different lengths,
            // something has changed during development and the saved data is out of sync.
            if (this.questStepStates.Length != this.questSO.questStepPrefabs.Length)
            {
                Debug.LogWarning("Quest Step Prefabs and Quest Step States are "
                                 + "of different lengths. This indicates something changed "
                                 + "with the QuestInfo and the saved data is now out of sync. "
                                 + "Reset your data - as this might cause issues. QuestId: " + this.questSO.id);
            }
            QuestStep.questStepSender += questStep => AddToQuestStepMap(questStep);
        }


        /*public void MoveToNextStep()
        {
            currentQuestStepIndex++;
            currentStep = currentQuestStepIndex;
        }*/

        public bool CurrentStepExists()
        {
            return false;
        }

        public void InstantiateCurrentQuestStep(Transform parentTransform)
        {
            /*GameObject questStepPrefab = GetCurrentQuestStepPrefab();
            if (questStepPrefab != null)
            {
                QuestStep questStep = Object.Instantiate<GameObject>(questStepPrefab, parentTransform)
                    .GetComponent<QuestStep>();
                questStep.InitializeQuestStep(questSO.id);
                Debug.LogWarning($"InitializeQuestStep is Empty");
            }*/
        }

        /*
        private GameObject GetCurrentQuestStepPrefab()
        {
            GameObject questStepPrefab = null;
            if (CurrentStepExists())
            {
                questStepPrefab = questSO.questStepPrefabs[currentQuestStepIndex];
            }
            else
            {
                Debug.LogWarning("Tried to get quest step prefab, but stepIndex was out of range indicating that "
                                 + "there's no current step: QuestId=" + questSO.id + ", stepIndex=" +
                                 currentQuestStepIndex);
            }

            return questStepPrefab;
        }
        */

        public void StoreQuestStepState(QuestStepState questStepState, int stepIndex)
        {
            if (stepIndex < questStepStates.Length)
            {
                questStepStates[stepIndex].state = questStepState.state;
            }
            else
            {
                Debug.LogWarning("Tried to access quest step data, but stepIndex was out of range: "
                                 + "Quest Id = " + questSO.id + ", Step Index = " + stepIndex);
            }
        }

        /*public QuestData GetQuestData()
        {
            return new QuestData(state, currentQuestStepIndex, questStepStates);
        }*/

        private void AddToQuestStepMap(QuestStep questStep)
        {
            if (questStep.QuestId != questSO.id)
            {
                return;
            }


            if (stepList.ContainsKey(questStep.StepId))
            {
                Debug.Log(questStep.GetStatus());
                stepList[questStep.StepId] = questStep.GetStatus();
            }
            else
            {
                stepList.Add(questStep.StepId, questStep.GetStatus());
            }
        }

        public QuestStepStatus GetQuestStepStatusById(string id)
        {
            QuestStepStatus status = stepList[id];
            return status;
        }

        public void Unsubscribe()
        {
            QuestStep.questStepSender -= questStep => AddToQuestStepMap(questStep);

        }
    }
}