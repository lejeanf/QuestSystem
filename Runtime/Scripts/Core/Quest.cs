using System;
using System.Collections;
using System.Collections.Generic;
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
        private List<QuestStepState> questStepStates;
        public StringEventChannelSO messageChannel;
        public bool sendMessageOnInitialization = false;
        public string messageToSendOnInit = "";
        public bool sendMessageOnFinish = false;
        public string messageToSendOnFinish = "";

        public Quest(QuestSO questSO)
        {
            this.questSO = questSO;
            this.state = QuestState.REQUIREMENTS_NOT_MET;
            this.questStepStates = new List<QuestStepState>();
            this.messageChannel = questSO.messageChannel;
            //init
            this.sendMessageOnInitialization = questSO.sendMessageOnInitialization;
            this.messageToSendOnInit = questSO.messageToSendOnInitialization;
            //finish
            this.sendMessageOnFinish = questSO.sendMessageOnFinish;
            this.messageToSendOnFinish = questSO.messageToSendOnFinish;
            for (int i = 0; i < questStepStates.Count; i++)
            {
                questStepStates[i] = new QuestStepState();
            }
        }

        public Quest(QuestSO questQuestSo, QuestState questState, int currentQuestStepIndex,
            List<QuestStepState> questStepStates)
        {
            this.questSO = questQuestSo;
            this.state = questState;
            this.questStepStates = questStepStates;
        }
    }
}