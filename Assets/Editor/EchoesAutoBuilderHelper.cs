using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
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
