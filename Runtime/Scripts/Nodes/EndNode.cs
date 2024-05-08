using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using NodeGraphProcessor.Examples;
using UnityEngine;

[System.Serializable, NodeMenuItem("questSystem/End")] // Add the node in the node creation context menu
public class EndNode : ConditionalNode
{
    public override IEnumerable<ConditionalNode> GetExecutedNodes()
    {
        throw new System.NotImplementedException();
    }
}
