 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestInfoSO", menuName = "Quests/QuestInfoSO", order = 1)]
[ScriptableObjectDrawer]
public class QuestInfoSO : ScriptableObject
{
    [field: SerializeField] public string id { get; private set; }

    [Header("General")]
    public string displayName;

    [Header("Requirements")]
    public int levelRequirement;
    public QuestInfoSO[] questPrerequisites;

    [Header("Steps")]
    public GameObject[] questStepPrefabs;

    [Header("Rewards")]
    public string unlockedScenario;

    // ensure the id is always the name of the Scriptable Object asset
    private void OnValidate()
    {
        /*
        #if UNITY_EDITOR
        if (id == string.Empty) id = this.name;
        if (this != null) UnityEditor.EditorUtility.SetDirty(this);
        #endif
        */
    }
}
