using System;
using GraphProcessor;
using jeanf.EventSystem;
using UnityEngine;
using jeanf.propertyDrawer;

namespace jeanf.questsystem
{
    [CreateAssetMenu(fileName = "QuestSO", menuName = "Quests/QuestSO", order = 1)]
    [ScriptableObjectDrawer]
    public class QuestSO : ScriptableObject
    {
        [field: Space(10)] [field: ReadOnly] [SerializeField] public string id = string.Empty;

        [Header("General")] public string displayName;
        [SerializeField] public BaseGraph QuestTree;
        [SerializeField] public QuestStep startingStep;
        
        [Header("Custom messages init/finish")] 
        [SerializeField] public StringEventChannelSO messageChannel;
        [SerializeField] public bool sendMessageOnInitialization = false;
        [SerializeField] public string messageToSendOnInitialization = "";
        [SerializeField] public bool sendMessageOnFinish = false;
        [SerializeField] public string messageToSendOnFinish = "";

        [Header("Requirements")] public int levelRequirement;
        public QuestSO[] questPrerequisites;

        [Header("Steps")] public GameObject[] questStepPrefabs;

        [Header("Rewards")] public string unlockedScenario;

        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (id == string.Empty || id == null) GenerateId();
        }

        public void GenerateId()
        {
            id = $"{System.Guid.NewGuid()}";
            UnityEditor.EditorUtility.SetDirty(this);
        }
    #endif
    }
}