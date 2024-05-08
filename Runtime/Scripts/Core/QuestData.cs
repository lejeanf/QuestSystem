using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

namespace jeanf.questsystem
{
    // this is the information needed to save the quest
    [System.Serializable]
    public class QuestData
    {
        // old constructor (linear structure)
        public QuestState state;
        //public int questStepIndex;
        //public QuestStepState[] questStepStates;

        //public QuestData(QuestState state, int questStepIndex, QuestStepState[] questStepStates)
        //{
        //    this.state = state;
        //    this.questStepIndex = questStepIndex;
        //    this.questStepStates = questStepStates;
        //}
        
        
        // new constructor (tree structure)
        public BaseGraph QuestTree;
        public Dictionary<string, QuestStep> QuestStepDictionary;
        
        public QuestData(BaseGraph QuestTree, Dictionary<string, QuestStep> QuestStepDictionary)
        {
            this.QuestTree = QuestTree;
            this.QuestStepDictionary = QuestStepDictionary;
        }
    }
}