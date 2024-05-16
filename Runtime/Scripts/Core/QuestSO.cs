using System;
using GraphProcessor;
using jeanf.EventSystem;
using UnityEngine;
using jeanf.propertyDrawer;
using jeanf.validationTools;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace jeanf.questsystem
{
    [CreateAssetMenu(fileName = "QuestSO", menuName = "Quests/QuestSO", order = 1)]
    [ScriptableObjectDrawer]
    public class QuestSO : ScriptableObject, IValidatable
    {
        [field: Space(10)] [field: ReadOnly] [SerializeField] public string id = string.Empty;
        private bool isDebug;
        public bool IsValid { get; private set; }

        [Header("General")] public string displayName;
        
        [Header("Custom messages init/finish")] 
        [SerializeField] public StringEventChannelSO messageChannel; //change to delegate?
        [SerializeField] public bool sendMessageOnInitialization = false;
        [SerializeField] public string messageToSendOnInitialization = "";
        [SerializeField] public bool sendMessageOnFinish = false;
        [SerializeField] public string messageToSendOnFinish = "";

        [Header("Requirements")] public int levelRequirement;
        public QuestSO[] questPrerequisites;
        public List<string> ScenesToLoad = new List<string>();
        public List<int> roomsToUnlock = new List<int>();

        [Header("Steps")] public QuestStep[] questSteps;

        [Header("Rewards")] public string unlockedScenario;



        private void ValidityCheck()
        {
            var validityCheck = true;
            var invalidObjects = new List<object>();
            var errorMessages = new List<string>();

            IsValid = validityCheck;
            if (!IsValid) return;

            if (IsValid && !Application.isPlaying) return;
            for (var i = 0; i < invalidObjects.Count; i++)
            {
                Debug.LogError($"Error: {errorMessages[i]} ", this);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (id == string.Empty || id == null) GenerateId();
            ValidityCheck();
        }

        public void GenerateId()
        {
            id = $"{System.Guid.NewGuid()}";
            UnityEditor.EditorUtility.SetDirty(this);
        }
    #endif
    }
}
