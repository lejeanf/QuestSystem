 using jeanf.EventSystem;
 using UnityEngine;
 using jeanf.propertyDrawer;

 namespace jeanf.questsystem
 {
     [CreateAssetMenu(fileName = "QuestInfoSO", menuName = "Quests/QuestInfoSO", order = 1)]
     [ScriptableObjectDrawer]
     public class QuestInfoSO : ScriptableObject
     {
         public string id { get; private set; }

         [Header("General")] public string displayName;
         
         [Header("Custom messages init/finish")] 
         [SerializeField] public StringEventChannelSO messageChannel;
         [SerializeField] public bool sendMessageOnInitialization = false;
         [SerializeField] public string messageToSendOnInitialization = "";
         [SerializeField] public bool sendMessageOnFinish = false;
         [SerializeField] public string messageToSendOnFinish = "";

         [Header("Requirements")] public int levelRequirement;
         public QuestInfoSO[] questPrerequisites;

         [Header("Steps")] public GameObject[] questStepPrefabs;

         [Header("Rewards")] public string unlockedScenario;
     }
 }