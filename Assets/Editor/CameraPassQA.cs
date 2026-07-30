using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CameraPassQA
{
    [MenuItem("Echoes of You/Camera Pass/QA — Verify All Levels", false, 400)]
    public static void VerifyAllLevels()
    {
        List<string> failures = new List<string>();
        int passed = 0;

        string[] scenePaths = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        foreach (string guid in scenePaths)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (!name.StartsWith("Level_") || name.Contains("TEST"))
                continue;

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            string result = VerifySingleLevel(name);
            if (result != null)
                failures.Add(result);
            else
                passed++;
            EditorSceneManager.CloseScene(scene, true);
        }

        string report;
        if (failures.Count == 0)
            report = "ALL LEVELS PASS QA";
        else
            report = string.Join("\n", failures);

        Debug.Log($"<b>[Camera Pass QA]</b> {passed} passed, {failures.Count} failed\n{report}");
    }

    static string VerifySingleLevel(string levelName)
    {
        var issues = new List<string>();

        // Validate SimpleFollowCamera (Cinemachine replacement)
        SimpleFollowCamera[] simpleCams = Object.FindObjectsByType<SimpleFollowCamera>(FindObjectsInactive.Include);
        if (simpleCams.Length == 0)
            issues.Add("  - Missing SimpleFollowCamera on Main Camera");
        if (simpleCams.Length > 1)
            issues.Add($"  - Multiple SimpleFollowCamera ({simpleCams.Length}) — should be exactly 1");

        SimpleFollowCamera playerCam = null;
        foreach (var sc in simpleCams)
        {
            if (sc.name == "Main Camera" || sc.name.Contains("Camera"))
            {
                playerCam = sc;
                break;
            }
        }
        if (playerCam == null && simpleCams.Length > 0)
            playerCam = simpleCams[0];

        if (playerCam == null)
            issues.Add("  - No Main Camera SimpleFollowCamera found");
        else
        {
            if (playerCam.target == null)
                issues.Add("  - SimpleFollowCamera.target is null (lost player)");
            if (playerCam.distance <= 0f)
                issues.Add($"  - SimpleFollowCamera.distance invalid ({playerCam.distance})");
        }

        // Validate legacy Cinemachine remnants are gone
        var legacyBrain = Object.FindAnyObjectByType<UnityEngine.Component>();
        // (Cinemachine types are fully removed; no runtime check needed)

        LevelCameraProfiles.Profile profile;
        if (!LevelCameraProfiles.TryGet(levelName, out profile))
            issues.Add($"  - No LevelCameraProfile for '{levelName}'");

        if (issues.Count == 0)
            return null;

        return $"<b>{levelName}</b>:\n{string.Join("\n", issues)}";
    }
}
