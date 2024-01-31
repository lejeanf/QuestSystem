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
        public QuestInfoSO info;

        // state info
        public QuestState state;
        private int currentQuestStepIndex;
        public int currentStep;
        private QuestStepState[] questStepStates;
        public StringEventChannelSO messageChannel;
        public bool sendMessageOnInitialization = false;
        public string messageToSendOnInit = "";
        public bool sendMessageOnFinish = false;
        public string messageToSendOnFinish = "";

        public Quest(QuestInfoSO questInfo)
        {
            this.info = questInfo;
            this.state = QuestState.REQUIREMENTS_NOT_MET;
            this.currentQuestStepIndex = currentStep = 0;
            this.questStepStates = new QuestStepState[info.questStepPrefabs.Length];
            this.messageChannel = questInfo.messageChannel;
            //init
            this.sendMessageOnInitialization = questInfo.sendMessageOnInitialization;
            this.messageToSendOnInit = questInfo.messageToSendOnInitialization;
            //finish
            this.sendMessageOnFinish = questInfo.sendMessageOnFinish;
            this.messageToSendOnFinish = questInfo.messageToSendOnFinish;
            for (int i = 0; i < questStepStates.Length; i++)
            {
                questStepStates[i] = new QuestStepState();
            }
        }

        public Quest(QuestInfoSO questInfo, QuestState questState, int currentQuestStepIndex,
            QuestStepState[] questStepStates)
        {
            this.info = questInfo;
            this.state = questState;
            this.currentQuestStepIndex = currentQuestStepIndex;
            currentStep = this.currentQuestStepIndex;
            this.questStepStates = questStepStates;

            // if the quest step states and prefabs are different lengths,
            // something has changed during development and the saved data is out of sync.
            if (this.questStepStates.Length != this.info.questStepPrefabs.Length)
            {
                Debug.LogWarning("Quest Step Prefabs and Quest Step States are "
                                 + "of different lengths. This indicates something changed "
                                 + "with the QuestInfo and the saved data is now out of sync. "
                                 + "Reset your data - as this might cause issues. QuestId: " + this.info.id);
            }
        }


        public void MoveToNextStep()
        {
            currentQuestStepIndex++;
            currentStep = currentQuestStepIndex;
        }

        public bool CurrentStepExists()
        {
            return (currentQuestStepIndex < info.questStepPrefabs.Length);
        }

        public void InstantiateCurrentQuestStep(Transform parentTransform)
        {
            GameObject questStepPrefab = GetCurrentQuestStepPrefab();
            if (questStepPrefab != null)
            {
                QuestStep questStep = Object.Instantiate<GameObject>(questStepPrefab, parentTransform)
                    .GetComponent<QuestStep>();
                questStep.InitializeQuestStep(info.id, currentQuestStepIndex,
                    questStepStates[currentQuestStepIndex].state);
            }
        }

        private GameObject GetCurrentQuestStepPrefab()
        {
            GameObject questStepPrefab = null;
            if (CurrentStepExists())
            {
                questStepPrefab = info.questStepPrefabs[currentQuestStepIndex];
            }
            else
            {
                Debug.LogWarning("Tried to get quest step prefab, but stepIndex was out of range indicating that "
                                 + "there's no current step: QuestId=" + info.id + ", stepIndex=" +
                                 currentQuestStepIndex);
            }

            return questStepPrefab;
        }

        public void StoreQuestStepState(QuestStepState questStepState, int stepIndex)
        {
            if (stepIndex < questStepStates.Length)
            {
                questStepStates[stepIndex].state = questStepState.state;
            }
            else
            {
                Debug.LogWarning("Tried to access quest step data, but stepIndex was out of range: "
                                 + "Quest Id = " + info.id + ", Step Index = " + stepIndex);
            }
        }

        public QuestData GetQuestData()
        {
            return new QuestData(state, currentQuestStepIndex, questStepStates);
        }
    }
}