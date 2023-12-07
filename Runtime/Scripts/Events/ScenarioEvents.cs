using System;

public class ScenarioEvents
{
    public event Action<string> onScenarioUnlocked;
    public void ScenarioUnlocked(string id) 
    {
        if (onScenarioUnlocked != null) 
        {
            onScenarioUnlocked(id);
        }
    }

    public event Action<string> onScenarioLocked;
    public void ScenarioLocked(string id) 
    {
        if (onScenarioLocked != null) 
        {
            onScenarioLocked(id);
        }
    }
}
