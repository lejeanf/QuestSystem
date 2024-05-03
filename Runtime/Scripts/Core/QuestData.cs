using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jeanf.questsystem
{
    // this is the information needed to save the quest
    [System.Serializable]
    public class QuestData
    {
        public QuestState state;
        public int questStepIndex;
        public QuestStepState[] questStepStates;

        public QuestData(QuestState state, int questStepIndex, QuestStepState[] questStepStates)
        {
            this.state = state;
            this.questStepIndex = questStepIndex;
            this.questStepStates = questStepStates;
        }
    }
}