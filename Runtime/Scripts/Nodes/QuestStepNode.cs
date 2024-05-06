using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using jeanf.EventSystem;
using jeanf.questsystem;
using NodeGraphProcessor.Examples;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable, NodeMenuItem("questSystem/Test")] // Add the node in the node creation context menu
public class QuestStepNode : BaseNode
{
    public QuestStep _questStep;

    public delegate void StepIdSender(string stepId);
    public static StepIdSender stepIdSender;
    
    [Input(name = "TriggeredBy"), Vertical]
    public ConditionalLink input;


    [Output(name = "TriggersNext"), Vertical]
    public ConditionalLink                output;


    public override string        name => "TestNode";
    
    private void OnEnable()
    {
        QuestStep.stepCompleted += CompleteStep;
    }


    private void OnDisable() => Unsubscribe();
    private void OnDestroy() => Unsubscribe();

    private void Unsubscribe()
    {
        QuestStep.stepCompleted -= CompleteStep;

    }
    protected override void Process()
    {
        stepIdSender?.Invoke(_questStep.StepId);

    }



    private void CompleteStep(string id)
    {
        if (id != _questStep.StepId) return;


        foreach (var outputPort in outputPorts)
        {
            var ownerGuid = outputPort.owner.GUID;
            Debug.Log($"output port ownerGuid: {ownerGuid}");
            outputPort.PushData();
        }
    }

}