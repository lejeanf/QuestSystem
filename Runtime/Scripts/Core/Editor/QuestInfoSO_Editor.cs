
#if UNITY_EDITOR
using jeanf.questsystem;
using UnityEditor;
using UnityEngine;

public class QuestInfoSO_Editor : Editor
{
    [CustomEditor(typeof(QuestInfoSO))]
    public class BoolEventOnClickEditor : Editor {
        override public void  OnInspectorGUI () {
            GUILayout.Space(10);
            var eventToSend = (QuestInfoSO)target;
            if(GUILayout.Button("Regenerate quest id", GUILayout.Height(20))) {
                eventToSend.GenerateId(); // how do i call this?
            }
            GUILayout.Space(10);
            
            DrawDefaultInspector();
        }
    }
}
#endif