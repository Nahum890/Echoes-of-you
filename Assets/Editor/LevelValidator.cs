using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Valida niveles post-build: detecta diseños demasiado simples,
/// falta de movimiento, falta de timing y problemas de visibilidad.
/// </summary>
public static class LevelValidator
{
    [MenuItem("Echoes of You/Production/Validate Current Scene", false, 210)]
    public static void ValidateCurrentScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        Debug.Log($"[LevelValidator] Validating: {scene.name}");
        ValidateScene(scene);
    }

    public static bool ValidateScene(Scene scene)
    {
        bool passed = true;
        string name = scene.name;

        // Skip non-gameplay scenes
        if (name == "MainMenu" || name == "Level_07")
        {
            Debug.Log($"[LevelValidator] Skipping {name} (non-gameplay)");
            return true;
        }

        // 1. Check minimum PressurePlates
        PressurePlate[] plates = Object.FindObjectsOfType<PressurePlate>();
        if (plates.Length < 2)
        {
            Debug.LogWarning($"[LevelValidator] {name}: Only {plates.Length} PressurePlate(s) — minimum is 2.");
            passed = false;
        }
        else
        {
            Debug.Log($"[LevelValidator] ✓ {name}: {plates.Length} PressurePlates");
        }

        // 2. Check PuzzleIntent
        PuzzleIntent intent = Object.FindObjectOfType<PuzzleIntent>();
        if (intent == null)
        {
            Debug.LogWarning($"[LevelValidator] {name}: Missing PuzzleIntent component.");
            passed = false;
        }
        else
        {
            if (intent.requiredActions < 3)
            {
                Debug.LogWarning($"[LevelValidator] {name}: requiredActions={intent.requiredActions} — minimum is 3.");
                passed = false;
            }
            else
            {
                Debug.Log($"[LevelValidator] ✓ {name}: requiredActions={intent.requiredActions}");
            }

            if (!intent.requiresMovement)
            {
                Debug.LogWarning($"[LevelValidator] {name}: Echo movement not required — level may be static.");
                passed = false;
            }
            else
            {
                Debug.Log($"[LevelValidator] ✓ {name}: Echo movement required");
            }

            if (intent.buttonCount < 2)
            {
                Debug.LogWarning($"[LevelValidator] {name}: buttonCount={intent.buttonCount} — minimum is 2.");
                passed = false;
            }
        }

        // 3. Check LevelExit visibility
        LevelExit exit = Object.FindObjectOfType<LevelExit>();
        if (exit == null)
        {
            Debug.LogWarning($"[LevelValidator] {name}: No LevelExit found.");
            passed = false;
        }
        else
        {
            Debug.Log($"[LevelValidator] ✓ {name}: LevelExit present at {exit.transform.position}");
        }

        // 4. Check height variation (asymmetry)
        float[] heights = CollectPlatformHeights();
        HashSet<float> uniqueHeights = new HashSet<float>();
        foreach (float h in heights)
            uniqueHeights.Add(Mathf.Round(h * 10f) / 10f);

        if (uniqueHeights.Count < 2)
        {
            Debug.LogWarning($"[LevelValidator] {name}: Only {uniqueHeights.Count} height level(s) — layout may be flat.");
            passed = false;
        }
        else
        {
            Debug.Log($"[LevelValidator] ✓ {name}: {uniqueHeights.Count} distinct height levels");
        }

        // 5. Check EchoPathHint exists
        EchoPathHint pathHint = Object.FindObjectOfType<EchoPathHint>();
        if (pathHint == null)
        {
            Debug.LogWarning($"[LevelValidator] {name}: No EchoPathHint — echo route not guided.");
        }
        else
        {
            Debug.Log($"[LevelValidator] ✓ {name}: EchoPathHint present");
        }

        // Summary
        if (passed)
            Debug.Log($"[LevelValidator] ✓ {name}: ALL VALIDATIONS PASSED");
        else
            Debug.LogError($"[LevelValidator] ✗ {name}: VALIDATION FAILED — review warnings above.");

        return passed;
    }

    static float[] CollectPlatformHeights()
    {
        List<float> heights = new List<float>();
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        foreach (GameObject go in allObjects)
        {
            if (go.isStatic && go.GetComponent<MeshRenderer>() != null && go.GetComponent<Collider>() != null)
                heights.Add(go.transform.position.y);
        }
        return heights.ToArray();
    }

    /// <summary>
    /// Called by the builder after all scenes are built.
    /// Opens each scene and validates it.
    /// </summary>
    public static void ValidateAllLevels()
    {
        string[] levelScenes = {
            "Assets/Scenes/Level_01.unity",
            "Assets/Scenes/Level_02.unity",
            "Assets/Scenes/Level_03.unity",
            "Assets/Scenes/Level_04.unity",
            "Assets/Scenes/Level_05.unity",
            "Assets/Scenes/Level_06.unity"
        };

        int passed = 0;
        int total = levelScenes.Length;

        for (int i = 0; i < levelScenes.Length; i++)
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(levelScenes[i]) == null)
            {
                Debug.LogWarning($"[LevelValidator] Scene not found: {levelScenes[i]}");
                continue;
            }

            Scene scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(levelScenes[i], UnityEditor.SceneManagement.OpenSceneMode.Single);
            if (ValidateScene(scene))
                passed++;
        }

        Debug.Log($"[LevelValidator] ═══════════════════════════════════════");
        Debug.Log($"[LevelValidator] Results: {passed}/{total} levels passed validation.");
        Debug.Log($"[LevelValidator] ═══════════════════════════════════════");
    }
}
