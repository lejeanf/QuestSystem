using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jeanf.questsystem
{
    public enum QuestState
    {
        REQUIREMENTS_NOT_MET,
        CAN_START,
        IN_PROGRESS,
        CAN_FINISH,
        FINISHED
    }
}