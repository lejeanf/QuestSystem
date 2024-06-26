using System;
using jeanf.EventSystem;
using UnityEditor;
using UnityEngine;

namespace jeanf.questsystem
{
    public class QuestEvents
    {
        public event Action<string> onStartQuest;

        public void StartQuest(string id)
        {
            if (onStartQuest != null)
            {
                Debug.Log($"starting quest: {id}");
                onStartQuest(id);
            }
        }

        public event Action<string> onFinishQuest;

        public void FinishQuest(string id)
        {
            if (onFinishQuest != null)
            {
                Debug.Log($"finishing quest: {id}");
                onFinishQuest(id);
            }
        }

        public event Action<Quest> onQuestStateChange;

        public void QuestStateChange(Quest quest)
        {
            if (onQuestStateChange != null)
            {
                Debug.Log($"quest state change: {quest.questSO.id} -- state: {quest.state}");
                onQuestStateChange(quest);
            }
        }
    }
}