#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace jeanf.questsystem
{
    [CustomEditor(typeof(QuestItem), true)]
    public class QuestItem_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(10);
            var eventToSend = (QuestItem)target;
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Log active steps", GUILayout.Height(20))) {
                eventToSend.LogActiveSteps(); // how do i call this?
            }
            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Validate Currently Active Steps", GUILayout.Height(20)))
            {
                eventToSend.ValidateCurrentlyActiveSteps(); // how do i call this?
            }
            GUI.backgroundColor = originalColor;
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            DrawDefaultInspector();
        }
    }
}
#endif