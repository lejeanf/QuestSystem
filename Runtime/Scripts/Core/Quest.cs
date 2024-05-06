using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using jeanf.EventSystem;
using jeanf.propertyDrawer;
using UnityEngine;
using Object = UnityEngine.Object;

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
        private List<QuestStep> questSteps = new List<QuestStep>();

        public  delegate void QuestStepSender(QuestStep questStep);
        public static QuestStepSender questStepSender;
        public delegate void QuestSender(string questId, Quest quest);
        public static QuestSender questSender;


        public Quest(QuestSO questQuestSo)
        {
            this.questSO = questQuestSo;
            this.state = QuestState.REQUIREMENTS_NOT_MET;
            this.questStepStates = new QuestStepState[questSO.questSteps.Length];
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

            questSender?.Invoke(questQuestSo.id, this);
            
            //AddToStepMap(questSteps);

        }

        public Quest(QuestSO questQuestSo, QuestState questState, int currentQuestStepIndex,
            QuestStepState[] questStepStates)
        {
            this.questSO = questQuestSo;
            this.state = questState;
            this.questStepStates = questStepStates;

            // if the quest step states and prefabs are different lengths,
            // something has changed during development and the saved data is out of sync.
            if (this.questStepStates.Length != this.questSO.questSteps.Length)
            {
                Debug.LogWarning("Quest Step Prefabs and Quest Step States are "
                                 + "of different lengths. This indicates something changed "
                                 + "with the QuestInfo and the saved data is now out of sync. "
                                 + "Reset your data - as this might cause issues. QuestId: " + this.questSO.id);
            }



            //AddToStepMap(questSteps);

        }

        public void SaveQuestTreeState(BaseGraph tree, Dictionary<string, QuestStep> stepDictionary)
        {
            
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
                                 + "Quest Id = " + questSO.id + ", Step Index = " + stepIndex);
            }
        }

        public void InstantiateCurrentQuestStep(QuestStep step, Transform parent)
        {
            Object.Instantiate(step.PrefabToInstantiate, parent);
        }



        

        /*public QuestData GetQuestData()
        {
            return new QuestData(state, currentQuestStepIndex, questStepStates);
        }*/


        //public void InstantiateCurrentQuestStep(Transform parentTransform)
        //{
        //    GameObject questStepPrefab = GetCurrentQuestStepPrefab();
        //    if (questStepPrefab != null)
        //    {
        //        QuestStep questStep = Object.Instantiate<GameObject>(questStepPrefab, parentTransform)
        //            .GetComponent<QuestStep>();
        //        questStep.InitializeQuestStep(info.id, currentQuestStepIndex,
        //            questStepStates[currentQuestStepIndex].state);
        //    }
        //}

        //private void AddToStepMap(List<QuestStep> questSteps)
        //{
        //    questStepSender?.Invoke(questSteps);
        //    //if (questStep.QuestId != questSO.id)
        //    //{
        //    //    return;
        //    //}


        //    //if (stepDictionary.ContainsKey(questStep.StepId))
        //    //{
        //    //    Debug.Log(questStep.GetStatus());
        //    //    stepDictionary[questStep.StepId].stepStatus = questStep.GetStatus();
        //    //}
        //    //else
        //    //{
        //    //    stepDictionary.Add(questStep.StepId, questStep);
        //    //}
        //}

        //public QuestStepStatus GetQuestStepStatusById(string id)
        //{
        //    //QuestStepStatus status = stepDictionary[id].stepStatus;
        //    //return status;
        //}
    }
}