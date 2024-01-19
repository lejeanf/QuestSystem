using System;
using UnityEngine;

namespace jeanf.questsystem
{
    public class GameEventsManager : MonoBehaviour
    {
        public static GameEventsManager instance { get; private set; }

        public InputEvents inputEvents;
        public PlayerEvents playerEvents;
        public ScenarioEvents scenarioEvents;
        public MiscEvents miscEvents;
        public QuestEvents questEvents;

        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("Found more than one Game Events Manager in the scene.");
            }

            instance = this;

            // initialize all events
            inputEvents = new InputEvents();
            playerEvents = new PlayerEvents();
            scenarioEvents = new ScenarioEvents();
            miscEvents = new MiscEvents();
            questEvents = new QuestEvents();
        }
    }
}
