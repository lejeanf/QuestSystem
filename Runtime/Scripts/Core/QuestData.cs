using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jeanf.questsystem
{
    // this is the information needed to save the quest
    [System.Serializable]
    public class QuestData
    {
        // old constructor (linear structure)
        public QuestState state;
        public int questStepIndex;
        public List<QuestStepState> questStepStates;

        public QuestData(QuestState state, int questStepIndex, List<QuestStepState> questStepStates)
        {
            this.state = state;
            this.questStepIndex = questStepIndex;
            this.questStepStates = questStepStates;
        }
    }
}