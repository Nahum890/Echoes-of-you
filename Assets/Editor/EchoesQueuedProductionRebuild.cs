#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class EchoesQueuedProductionRebuild
{
    const string FlagAssetPath = "Assets/Editor/EchoesQueuedProductionRebuild.flag";
    static bool _scheduled;

    static EchoesQueuedProductionRebuild()
    {
        EditorApplication.delayCall += TryRunQueuedRebuild;
    }

    [MenuItem("Echoes of You/Production/Queue Rebuild In Open Editor", false, 199)]
    public static void QueueRebuild()
    {
        File.WriteAllText(FlagAssetPath, "queued");
        AssetDatabase.ImportAsset(FlagAssetPath);
        TryRunQueuedRebuild();
    }

    static void TryRunQueuedRebuild()
    {
        if (_scheduled)
            return;

        if (!File.Exists(FlagAssetPath))
            return;

        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += TryRunQueuedRebuild;
            return;
        }

        _scheduled = true;
        EditorApplication.delayCall += RunQueuedRebuild;
    }

    static void RunQueuedRebuild()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            _scheduled = false;
            EditorApplication.delayCall += TryRunQueuedRebuild;
            return;
        }

        ClearFlag();

        try
        {
            Debug.Log("[Echoes Production] Running queued rebuild in the open Unity Editor.");
            EchoesProductionBuilder.RebuildAll();
        }
        catch (Exception ex)
        {
            Debug.LogError("[Echoes Production] Queued rebuild failed:\n" + ex);
        }
        finally
        {
            _scheduled = false;
        }
    }

    static void ClearFlag()
    {
        if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(FlagAssetPath) != null)
            AssetDatabase.DeleteAsset(FlagAssetPath);

        if (File.Exists(FlagAssetPath))
            File.Delete(FlagAssetPath);

        string metaPath = FlagAssetPath + ".meta";
        if (File.Exists(metaPath))
            File.Delete(metaPath);
    }
}
#endif
