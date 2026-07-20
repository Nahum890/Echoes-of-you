using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Echoes.EnvironmentPass;

public static class PropPlacementRunner
{
    [MenuItem("Tools/EnvPass/Run PlaceAll Silent")]
    public static void RunPlaceAllSilent()
    {
        var levels = EnvironmentPassDataLoader.LoadAllLevels();
        foreach (var level in levels)
        {
            Debug.Log($"[EnvPassSilent] Placing level {level.levelName}");
            var result = EnvironmentPassPlacementEngine.PlaceLevel(level, dryRun: false);
            if (!result.success)
                Debug.LogError($"[EnvPassSilent] Errors in {level.levelName}");
        }
        Debug.Log("[EnvPassSilent] Completed.");
    }
}
