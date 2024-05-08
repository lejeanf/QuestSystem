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
        //private QuestStepState[] questStepStates;
        public StringEventChannelSO messageChannel;
        public bool sendMessageOnInitialization = false;
        public string messageToSendOnInit = "";
        public bool sendMessageOnFinish = false;
        public string messageToSendOnFinish = "";

        public Quest(QuestSO questSO)
        {
            this.questSO = questSO;
            this.state = QuestState.REQUIREMENTS_NOT_MET;
            //this.questStepStates = new QuestStepState[this.questSO.questSteps.Length];
            this.messageChannel = questSO.messageChannel;
            //init
            this.sendMessageOnInitialization = questSO.sendMessageOnInitialization;
            this.messageToSendOnInit = questSO.messageToSendOnInitialization;
            //finish
            this.sendMessageOnFinish = questSO.sendMessageOnFinish;
            this.messageToSendOnFinish = questSO.messageToSendOnFinish;
            //for (int i = 0; i < questStepStates.Length; i++)
            //{
            //    questStepStates[i] = new QuestStepState();
            //}
        }

        //public Quest(QuestSO questQuestSo, QuestState questState, int currentQuestStepIndex,
        //    QuestStepState[] questStepStates)
        //{
        //    this.questSO = questQuestSo;
        //    this.state = questState;
        //    //this.questStepStates = questStepStates;

        //    // if the quest step states and prefabs are different lengths,
        //    // something has changed during development and the saved data is out of sync.
        //    //if (this.questStepStates.Length != this.questSO.questSteps.Length)
        //    //{
        //    //    Debug.LogWarning("Quest Step Prefabs and Quest Step States are "
        //    //                     + "of different lengths. This indicates something changed "
        //    //                     + "with the QuestInfo and the saved data is now out of sync. "
        //    //                     + "Reset your data - as this might cause issues. QuestId: " + this.questSO.id);
        //    //}
        //}


        
        //public void StoreQuestStepState(QuestStepState questStepState, int stepIndex)
        //{
        //    if (stepIndex < questStepStates.Length)
        //    {
        //        questStepStates[stepIndex].state = questStepState.state;
        //    }
        //    else
        //    {
        //        Debug.LogWarning("Tried to access quest step data, but stepIndex was out of range: "
        //                         + "Quest Id = " + questSO.id + ", Step Index = " + stepIndex);
        //    }
        //}

        //public QuestData GetQuestData()
        //{
        //    return new QuestData(state, currentQuestStepIndex, questStepStates);
        //}

    }
}