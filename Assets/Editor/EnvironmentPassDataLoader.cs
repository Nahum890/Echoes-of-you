using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Echoes.EnvironmentPass
{
    public static class EnvironmentPassDataLoader
    {
        private static List<LevelDataSO> s_cachedLevels;
        private static bool s_cacheValid;

        public static List<LevelDataSO> LoadAllLevels()
        {
            if (s_cacheValid && s_cachedLevels != null)
                return s_cachedLevels;

            var guids = AssetDatabase.FindAssets("t:LevelDataSO", new[] { "Assets/ScriptableObjects/EnvironmentPass" });
            s_cachedLevels = new List<LevelDataSO>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var level = AssetDatabase.LoadAssetAtPath<LevelDataSO>(path);
                if (level != null) s_cachedLevels.Add(level);
            }

            s_cachedLevels = s_cachedLevels.OrderBy(l => l.levelNumber).ToList();
            s_cacheValid = true;
            return s_cachedLevels;
        }

        public static LevelDataSO LoadLevel(int levelNumber)
        {
            return LoadAllLevels().FirstOrDefault(l => l.levelNumber == levelNumber);
        }

        public static void InvalidateCache()
        {
            s_cacheValid = false;
            s_cachedLevels = null;
        }
    }
}