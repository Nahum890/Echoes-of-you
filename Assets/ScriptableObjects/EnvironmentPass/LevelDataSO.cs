using System.Collections.Generic;
using UnityEngine;

namespace Echoes.EnvironmentPass
{
    [CreateAssetMenu(menuName = "Echoes/Environment Pass/Level Data", fileName = "LevelData_")]
    public class LevelDataSO : ScriptableObject
    {
        public int levelNumber;
        public string levelName;
        public string scenePath;
        public Chapter chapter;
        public List<RoomDataSO> rooms = new();
        public NarrativeClusterSO narrativeCluster;
    }
}