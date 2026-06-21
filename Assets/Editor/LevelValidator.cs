using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        if (name == "MainMenu")
        {
            Debug.Log($"[LevelValidator] Skipping {name} (non-gameplay)");
            return true;
        }

        PressurePlate[] plates = Object.FindObjectsOfType<PressurePlate>();
        PuzzleSignal[] signals = Object.FindObjectsOfType<PuzzleSignal>();
        if (plates.Length < 1 && signals.Length < 1)
        {
            Debug.LogWarning($"[LevelValidator] {name}: Missing puzzle objectives.");
            passed = false;
        }

        int dynamicSystemCount =
            Object.FindObjectsOfType<EchoKineticBody>().Length +
            Object.FindObjectsOfType<EchoShieldField>().Length +
            Object.FindObjectsOfType<EchoConflictTrap>().Length +
            Object.FindObjectsOfType<DynamicTransformMotor>().Length +
            Object.FindObjectsOfType<TimedMovingPlatform>().Length +
            Object.FindObjectsOfType<GhostBridge>().Length +
            Object.FindObjectsOfType<MemoryPlatform>().Length +
            Object.FindObjectsOfType<DoorController>().Length;
        if (dynamicSystemCount < 1)
        {
            Debug.LogWarning($"[LevelValidator] {name}: Missing dynamic echo systems.");
            passed = false;
        }

        PuzzleIntent intent = Object.FindObjectOfType<PuzzleIntent>();
        if (intent == null)
        {
            Debug.LogWarning($"[LevelValidator] {name}: Missing PuzzleIntent.");
            passed = false;
        }
        else
        {
            if (intent.requiredActions < 2)
            {
                Debug.LogWarning($"[LevelValidator] {name}: requiredActions={intent.requiredActions} is too low.");
                passed = false;
            }
            if (!intent.requiresMovement)
            {
                Debug.LogWarning($"[LevelValidator] {name}: Puzzle does not require movement.");
                passed = false;
            }
        }

        LevelGoal goal = Object.FindObjectOfType<LevelGoal>();
        if (goal == null)
        {
            Debug.LogWarning($"[LevelValidator] {name}: Missing LevelGoal.");
            passed = false;
        }

        LevelExit[] exits = Object.FindObjectsOfType<LevelExit>();
        if (exits.Length == 0)
        {
            Debug.LogWarning($"[LevelValidator] {name}: No LevelExit found.");
            passed = false;
        }

        Light[] lights = Object.FindObjectsOfType<Light>();
        int pointLights = 0;
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null && lights[i].type == LightType.Point)
                pointLights++;
        }
        if (pointLights == 0)
        {
            Debug.LogWarning($"[LevelValidator] {name}: No guiding point lights found.");
            passed = false;
        }

        float[] heights = CollectPlatformHeights();
        HashSet<float> uniqueHeights = new HashSet<float>();
        for (int i = 0; i < heights.Length; i++)
            uniqueHeights.Add(Mathf.Round(heights[i] * 10f) / 10f);

        if (uniqueHeights.Count < 1)
        {
            Debug.LogWarning($"[LevelValidator] {name}: No grounded geometry detected.");
            passed = false;
        }

        EchoPathHint pathHint = Object.FindObjectOfType<EchoPathHint>();
        if (pathHint == null)
            Debug.LogWarning($"[LevelValidator] {name}: No EchoPathHint.");

        if (Object.FindObjectOfType<LevelExperienceBlueprint>() == null)
        {
            Debug.LogWarning($"[LevelValidator] {name}: Missing LevelExperienceBlueprint (run Production rebuild).");
            passed = false;
        }

        if (Object.FindObjectOfType<LevelEscapeSequence>() == null)
            Debug.LogWarning($"[LevelValidator] {name}: No LevelEscapeSequence.");

        if (!ValidateCameraSightline(name))
            passed = false;

        if (!ValidatePlayableKitIntegrity(name))
            passed = false;

        if (!ValidateWalkableSeparation(name))
            passed = false;

        if (passed)
            Debug.Log($"[LevelValidator] PASS: {name}");
        else
            Debug.LogError($"[LevelValidator] FAIL: {name}");

        return passed;
    }

    static float[] CollectPlatformHeights()
    {
        List<float> heights = new List<float>();
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        for (int i = 0; i < allObjects.Length; i++)
        {
            GameObject go = allObjects[i];
            if (go.isStatic && 
                (go.GetComponent<Collider>() != null || go.GetComponentInChildren<Collider>() != null) && 
                (go.GetComponent<MeshRenderer>() != null || go.GetComponentInChildren<MeshRenderer>() != null))
            {
                heights.Add(go.transform.position.y);
            }
        }
        return heights.ToArray();
    }

    static bool ValidateCameraSightline(string sceneName)
    {
        Camera cameraRef = Camera.main;
        PlayerController player = Object.FindObjectOfType<PlayerController>();
        LevelExit exit = Object.FindObjectOfType<LevelExit>();
        if (cameraRef == null || player == null)
        {
            Debug.LogWarning($"[LevelValidator] {sceneName}: Missing camera or player for sightline validation.");
            return false;
        }

        bool passed = true;
        passed &= CheckSightline(sceneName, cameraRef.transform.position, player.transform.position + Vector3.up * 1.35f, "player");
        if (exit != null)
            passed &= CheckSightline(sceneName, cameraRef.transform.position, exit.transform.position + Vector3.up * 1.4f, "exit");
        return passed;
    }

    static bool ValidatePlayableKitIntegrity(string sceneName)
    {
        bool passed = true;
        LevelKitPiece[] pieces = Object.FindObjectsOfType<LevelKitPiece>();
        int walkableCount = 0;

        for (int i = 0; i < pieces.Length; i++)
        {
            LevelKitPiece piece = pieces[i];
            if (piece == null || !piece.walkableSurface)
                continue;

            walkableCount++;
            Collider colliderRef = piece.GetComponent<Collider>();
            if (colliderRef == null || colliderRef.isTrigger)
            {
                Debug.LogWarning($"[LevelValidator] {sceneName}: Walkable kit piece {piece.name} is missing a solid collider.");
                passed = false;
            }

            if (piece.footprintSize.x <= 0.1f || piece.footprintSize.z <= 0.1f)
            {
                Debug.LogWarning($"[LevelValidator] {sceneName}: Walkable kit piece {piece.name} has invalid footprint metadata.");
                passed = false;
            }
        }

        if (walkableCount < 4)
        {
            Debug.LogWarning($"[LevelValidator] {sceneName}: Too few walkable megakit pieces ({walkableCount}).");
            passed = false;
        }

        return passed;
    }

    static bool ValidateWalkableSeparation(string sceneName)
    {
        bool passed = true;
        List<Collider> colliders = new List<Collider>();
        LevelKitPiece[] pieces = Object.FindObjectsOfType<LevelKitPiece>();

        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] == null || !pieces[i].walkableSurface)
                continue;

            Collider colliderRef = pieces[i].GetComponent<Collider>();
            if (colliderRef != null && !colliderRef.isTrigger)
                colliders.Add(colliderRef);
        }

        for (int a = 0; a < colliders.Count; a++)
        {
            Bounds first = colliders[a].bounds;
            for (int b = a + 1; b < colliders.Count; b++)
            {
                Bounds second = colliders[b].bounds;
                float topA = first.max.y;
                float topB = second.max.y;
                if (Mathf.Abs(topA - topB) > 0.08f)
                    continue;

                float overlapX = Mathf.Min(first.max.x, second.max.x) - Mathf.Max(first.min.x, second.min.x);
                float overlapZ = Mathf.Min(first.max.z, second.max.z) - Mathf.Max(first.min.z, second.min.z);
                if (overlapX > 0.08f && overlapZ > 0.08f)
                {
                    Debug.LogWarning($"[LevelValidator] {sceneName}: Walkable pieces overlap on the same plane: {colliders[a].name} and {colliders[b].name}.");
                    passed = false;
                }
            }
        }

        return passed;
    }

    static bool CheckSightline(string sceneName, Vector3 from, Vector3 to, string targetName)
    {
        Vector3 direction = to - from;
        float distance = direction.magnitude;
        if (distance <= 0.01f)
            return true;

        if (!Physics.Raycast(from, direction.normalized, out RaycastHit hit, distance - 0.35f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            return true;

        if (hit.collider != null && hit.collider.CompareTag("Player"))
            return true;

        if (hit.collider != null && hit.collider.GetComponentInParent<DoorController>() != null)
            return true;

        // Barrier walls and non-walkable environmental pieces are expected framing
        if (hit.collider != null && hit.collider.name.StartsWith("Barrier"))
            return true;

        LevelKitPiece kitPiece = hit.collider != null ? hit.collider.GetComponent<LevelKitPiece>() : null;
        if (kitPiece != null && !kitPiece.walkableSurface)
            return true;

        Debug.LogWarning($"[LevelValidator] {sceneName}: Camera sightline to {targetName} is blocked by {hit.collider?.name}.");
        return false;
    }

    public static void ValidateAllLevels()
    {
        string[] levelScenes =
        {
            "Assets/Scenes/Level_01.unity",
            "Assets/Scenes/Level_02.unity",
            "Assets/Scenes/Level_03.unity",
            "Assets/Scenes/Level_04.unity",
            "Assets/Scenes/Level_05.unity",
            "Assets/Scenes/Level_06.unity",
            "Assets/Scenes/Level_07.unity",
            "Assets/Scenes/Level_08.unity",
            "Assets/Scenes/Level_09.unity",
            "Assets/Scenes/Level_10.unity",
            "Assets/Scenes/Level_11.unity",
            "Assets/Scenes/Level_12.unity",
            "Assets/Scenes/Level_13.unity",
            "Assets/Scenes/Level_14.unity",
            "Assets/Scenes/Level_15.unity"
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

        Debug.Log($"[LevelValidator] Results: {passed}/{total} levels passed validation.");
    }
}
