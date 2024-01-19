using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jeanf.questsystem
{
    public class ScenarioManager : MonoBehaviour
    {
        [Header("Configuration")] [SerializeField]
        private List<string> unlockedScenarios = new List<string>();



        private void OnEnable()
        {
            GameEventsManager.instance.scenarioEvents.onScenarioUnlocked += ScenarioUnlocked;
        }

        private void OnDisable()
        {
            GameEventsManager.instance.scenarioEvents.onScenarioUnlocked -= ScenarioUnlocked;
        }

        private void Start()
        {
            GameEventsManager.instance.scenarioEvents.ScenarioUnlocked("000_Tutorial");
        }

        public void ScenarioUnlocked(string id)
        {
            GameEventsManager.instance.scenarioEvents.ScenarioUnlocked(id);
        }
    }
}