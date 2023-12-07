    using System;
using System.Collections;
using System.Collections.Generic;
using jeanf.EventSystem;
using UnityEngine;

public class QuestItem : MonoBehaviour, IDebugBehaviour
{
    public bool isDebug
    { 
        get => _isDebug;
        set => _isDebug = value; 
    }
    [SerializeField] private bool _isDebug = false;
    
    [Tooltip("Visual feedback for the quest state")]
    
    
    [Header("Quest")]
    [SerializeField] private QuestInfoSO questInfoForPoint;
    
    [ReadOnly] [Range(0,1)] [SerializeField] private float progress = 0.0f;

    [Header("Config")]
    [SerializeField] private bool startPoint = true;
    [SerializeField] private bool finishPoint = true;
    
    [SerializeField] [ReadOnly] private bool playerIsNear = false;
    private string questId;
    private QuestState currentQuestState;

    // the event is Located in Assets/Resources/Quests/QuestsProgressChannel - it is searched for at Awake time.
    private StringFloatEventChannelSO QuestProgress;

    

    private void Awake()
    {
        if (QuestProgress == null)
        {
            if(isDebug) Debug.Log("attempting to find Quests/QuestsProgressChannel in the resources folder" ,this);
            QuestProgress = Resources.Load<StringFloatEventChannelSO>("Quests/QuestsProgressChannel");
            if(QuestProgress == null) Debug.LogError("Quests/QuestsProgressChannel is not in the resources folder",this);
        }


        var questIconPrefab = Instantiate(Resources.Load<GameObject>("Quests/QuestIcon"), this.transform);
        
        questId = questInfoForPoint.id;
    }

    private void OnEnable()
    {
        QuestProgress.OnEventRaised += UpdateProgress;
        GameEventsManager.instance.questEvents.onQuestStateChange += QuestStateChange;
        GameEventsManager.instance.inputEvents.onSubmitPressed += UpdateState;
    }

    private void OnDisable() => Unsubscribe();
    private void OnDestroy() => Unsubscribe();

    private void Unsubscribe()
    {
        QuestProgress.OnEventRaised -= UpdateProgress;
        GameEventsManager.instance.questEvents.onQuestStateChange -= QuestStateChange;
        GameEventsManager.instance.inputEvents.onSubmitPressed -= UpdateState;
    }

    public void UpdateState()
    {
        if (isDebug) Debug.Log($"Updating State...");
        if (!playerIsNear)
        {
            return;
        }
        if (isDebug) Debug.Log($"Player is near, continuing ...");

        // start or finish a quest
        if (currentQuestState.Equals(QuestState.CAN_START) && startPoint)
        {
            if(isDebug) Debug.Log($"Starting quest: {questId}");
            GameEventsManager.instance.questEvents.StartQuest(questId);
        }
        else if (currentQuestState.Equals(QuestState.CAN_FINISH) && finishPoint)
        {
            if(isDebug) Debug.Log($"Finishing quest: {questId}");
            GameEventsManager.instance.questEvents.FinishQuest(questId);
        }
    }

    private void UpdateProgress(string id, float progress)
    {
        if (id == questId)
        {
            this.progress = progress;
            if(isDebug) Debug.Log($"questid [{id}] progress = {progress*100}%");
        }

    }

    private void QuestStateChange(Quest quest)
    {
        // only update the quest state if this point has the corresponding quest
        if (quest.info.id.Equals(questId))
        {
            currentQuestState = quest.state;
        }
    }

    public void PlayerIsNear(bool value)
    {
        playerIsNear = value;
        UpdateState();
    }
}
