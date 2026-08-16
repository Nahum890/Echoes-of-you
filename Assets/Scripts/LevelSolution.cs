using UnityEngine;

public class LevelSolution : ScriptableObject
{
    [Header("Level Identity")]
    public int levelId;
    public string levelName;

    [Header("Optimal Path")]
    [TextArea(3, 10)] public string optimalPathDescription;
    public StepRecord[] steps;

    [Header("Validation")]
    public bool bfsReachable = true;
    public bool timingFloorMet = true;
    public bool signalGraphValid = true;
    public bool headlessPlaytestPassed = true;
    public float expectedCompletionTimeSeconds;

    [System.Serializable]
    public class StepRecord
    {
        public int order;
        public string action;
        public string targetComponent;
        public float timestamp;
        public string expectedResult;
    }
}
