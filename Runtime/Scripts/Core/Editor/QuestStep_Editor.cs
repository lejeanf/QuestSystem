#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace jeanf.questsystem
{
    [CustomEditor(typeof(QuestStep), true)]
    public class QuestStep_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(10);
            var eventToSend = (QuestStep)target;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Regenerate questStep id", GUILayout.Height(20)))
            {
                eventToSend.GenerateId(); // how do i call this?
            }
            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if(GUILayout.Button("Validate step", GUILayout.Height(20))) {
                eventToSend.FinishQuestStep(); // how do i call this?
            }
            GUI.backgroundColor = originalColor;
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            DrawDefaultInspector();
        }
    }
}
#endif
