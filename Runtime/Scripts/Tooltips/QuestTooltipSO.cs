using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using jeanf.tooltip;

namespace jeanf.questsystem
{
    [CreateAssetMenu(fileName = "QuestTooltipSO", menuName = "Tooltips/QuestTooltipSO", order = 1)]
    public class QuestTooltipSO : TooltipSO
    {
        public string tooltipToSend;
        public override string Tooltip
        {
            get
            {
                return tooltipToSend;
            }
        }
    }

}
