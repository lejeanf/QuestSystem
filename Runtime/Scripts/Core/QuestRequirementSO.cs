using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using jeanf.propertyDrawer;

namespace jeanf.questsystem
{
    [ScriptableObjectDrawer]
    public abstract class QuestRequirementSO : ScriptableObject
    {

        public abstract bool ValidateFulfilled();

    }
}
