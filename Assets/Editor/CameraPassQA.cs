using System.Collections.Generic;
using Unity.Cinemachine;
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

        CinemachineBrain[] brains = Object.FindObjectsByType<CinemachineBrain>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (brains.Length == 0)
            issues.Add("Missing CinemachineBrain on Main Camera");
        if (brains.Length > 1)
            issues.Add($"  - Multiple CinemachineBrains ({brains.Length}) — should be exactly 1");

        CinemachineCamera[] vcams = Object.FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        CinemachineCamera playerVCam = null;
        foreach (var vc in vcams)
        {
            if (vc.name == "PlayerVCam" || vc.name.Contains("Player"))
            {
                playerVCam = vc;
                break;
            }
        }
        if (playerVCam == null && vcams.Length > 0)
            playerVCam = vcams[0];

        if (playerVCam == null)
            issues.Add("  - No PlayerVCam (CinemachineCamera) found");

        if (playerVCam != null)
        {
            if (playerVCam.Follow == null)
                issues.Add("  - PlayerVCam.Follow is null (lost player)");
            if (playerVCam.LookAt == null)
                issues.Add("  - PlayerVCam.LookAt is null (no look target)");
        }

        CinemachineTargetGroup targetGroup = Object.FindAnyObjectByType<CinemachineTargetGroup>();
        if (targetGroup == null)
            issues.Add("  - No CinemachineTargetGroup found");
        else if (targetGroup.Targets == null || targetGroup.Targets.Count < 2)
            issues.Add($"  - TargetGroup has only {targetGroup.Targets?.Count ?? 0} targets (need Player + Goal + Echo)");

        ThirdPersonCamera tpc = Object.FindAnyObjectByType<ThirdPersonCamera>();
        if (tpc != null && tpc.enabled)
        {
            CinemachineBrain brain = tpc.GetComponent<CinemachineBrain>();
            if (brain == null)
                issues.Add("  - ThirdPersonCamera active WITHOUT CinemachineBrain (legacy system running)");
            else if (!brain.enabled)
                issues.Add("  - ThirdPersonCamera active with disabled Brain (conflict potential)");
        }

        CinematicCameraDynamics dynamics = Object.FindAnyObjectByType<CinematicCameraDynamics>();
        if (dynamics != null && dynamics.enabled)
        {
        }

        FixedPuzzleCameraController fixedCam = Object.FindAnyObjectByType<FixedPuzzleCameraController>();
        if (fixedCam != null && fixedCam.enabled)
        {
            if (fixedCam.playerFocus == null)
                issues.Add("  - FixedPuzzleCameraController.playerFocus is null");
            if (fixedCam.virtualCamera == null)
                issues.Add("  - FixedPuzzleCameraController.virtualCamera is null");
            if (fixedCam.targetGroup == null)
                issues.Add("  - FixedPuzzleCameraController.targetGroup is null");
            if (fixedCam.followTarget == null)
                issues.Add("  - FixedPuzzleCameraController.followTarget is null");
        }

        EchoCameraTargetGroupManager echoManager = Object.FindAnyObjectByType<EchoCameraTargetGroupManager>();
        if (echoManager == null)
            issues.Add("  - No EchoCameraTargetGroupManager (Echo won't be in TargetGroup)");

        EventCameraDirector eventDirector = Object.FindAnyObjectByType<EventCameraDirector>();
        if (eventDirector == null)
            issues.Add("  - No EventCameraDirector (no activation sequences)");

        LevelCameraProfiles.Profile profile;
        if (!LevelCameraProfiles.TryGet(levelName, out profile))
            issues.Add($"  - No LevelCameraProfile for '{levelName}'");

        if (issues.Count == 0)
            return null;

        return $"<b>{levelName}</b>:\n{string.Join("\n", issues)}";
    }
}