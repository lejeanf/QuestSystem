using UnityEngine;
using GraphProcessor;
using jeanf.questsystem;

public class QuestStepNode : BaseNode
{
    [Input(name = "A")] 
    private QuestStep StepsRequiredForCompletion;
    [Output(name = "Out")]
    private QuestStep StepsTriggeredOnCompletion;
}
