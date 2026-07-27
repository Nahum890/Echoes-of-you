using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data container for school greybox architecture layout per level.
/// </summary>
[CreateAssetMenu(fileName = "SchoolArchitectureData", menuName = "Echoes of You/School Architecture Data", order = 1)]
public class SchoolArchitectureData : ScriptableObject
{
    public string levelName = "Level_01";
    public List<ModulePlacementData> modules = new List<ModulePlacementData>();
    public Vector3 playerStartPosition = new Vector3(0f, 0.1f, -4f);
    public Vector3 levelExitPosition = new Vector3(0f, 0.1f, 50f);
}

[System.Serializable]
public class ModulePlacementData
{
    public string name;
    public ModuleType type;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale = Vector3.one;
    public string customData;
}