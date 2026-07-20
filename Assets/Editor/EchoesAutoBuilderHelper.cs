using UnityEditor;
using UnityEngine;

// [InitializeOnLoad]  // DISABLED for Environment Pass - manual prop placement
// [InitializeOnLoad]  // DISABLED - line 5 - Environment Pass 2.0 (2026-07-17)
public static class EchoesAutoBuilderHelper
{
    static EchoesAutoBuilderHelper()
    {
        EditorApplication.delayCall += AutoTriggerBuild;
    }

    private static void AutoTriggerBuild()
    {
        string flagPath = "Temp/echoes_auto_built.tmp";
        if (System.IO.File.Exists(flagPath)) return;

        Debug.Log("[Echoes Auto Builder] Triggering automated production rebuild...");
        try
        {
            EchoesNewProductionBuilder.BuildAllBlueprints();
            System.IO.File.WriteAllText(flagPath, "done");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Echoes Auto Builder] Build failed: " + e.Message);
        }
    }
}
