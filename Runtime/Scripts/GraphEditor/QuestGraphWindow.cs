#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using GraphProcessor;

public class QuestGraphWindow : BaseGraphWindow
{
    [MenuItem("QuestGraph/01_DefaultGraph")]
    public static BaseGraphWindow Open()
    {
        var graphWindow = GetWindow<QuestGraphWindow>();

        graphWindow.Show();

        return graphWindow;
    }

    protected override void InitializeWindow(BaseGraph graph)
    {
        // Set the window title
        titleContent = new GUIContent("Default Graph");

        // Here you can use the default BaseGraphView or a custom one (see section below)
        var graphView = new BaseGraphView(this);

        rootView.Add(graphView);
    }
    
}
#endif