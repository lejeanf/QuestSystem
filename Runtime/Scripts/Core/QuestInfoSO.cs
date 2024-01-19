 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 using jeanf.propertyDrawer;

 namespace jeanf.questsystem
 {
     [CreateAssetMenu(fileName = "QuestInfoSO", menuName = "Quests/QuestInfoSO", order = 1)]
     [ScriptableObjectDrawer]
     public class QuestInfoSO : ScriptableObject
     {
         [field: SerializeField] public string id { get; private set; }

         [Header("General")] public string displayName;

         [Header("Requirements")] public int levelRequirement;
         public QuestInfoSO[] questPrerequisites;

         [Header("Steps")] public GameObject[] questStepPrefabs;

         [Header("Rewards")] public string unlockedScenario;
     }
 }