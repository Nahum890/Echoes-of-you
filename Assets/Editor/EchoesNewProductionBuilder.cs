using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class EchoesNewProductionBuilder
{
    private const string SceneRoot = "Assets/Scenes";
    private const string BlueprintRoot = "Assets/Data/Levels";

    [MenuItem("Echoes of You/Production/Build All Blueprint Levels", false, 201)]
    public static void BuildAllBlueprints()
    {
        // 1. Ensure setup
        EchoesMaterialLibrary.EnsureMaterials();
        
        // 2. Find all LevelBlueprint ScriptableObjects
        string[] guids = AssetDatabase.FindAssets("t:LevelBlueprint", new[] { BlueprintRoot });
        if (guids == null || guids.Length == 0)
        {
            Debug.LogWarning("[Echoes Blueprint Builder] No LevelBlueprint ScriptableObjects found in " + BlueprintRoot);
            return;
        }

        Debug.Log($"[Echoes Blueprint Builder] Found {guids.Length} LevelBlueprints. Starting build...");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            LevelBlueprint blueprint = AssetDatabase.LoadAssetAtPath<LevelBlueprint>(path);
            if (blueprint != null)
            {
                BuildLevelFromBlueprint(blueprint);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Echoes Blueprint Builder] Rebuild completed successfully.");
    }

    public static void BuildLevelFromBlueprint(LevelBlueprint blueprint)
    {
        Debug.Log($"[Echoes Blueprint Builder] Building scene for: {blueprint.levelName}");

        Transform envRoot, mechRoot, uiRoot;
        Scene scene = EchoesLevelShell.CreateNewScene(blueprint.levelName, out envRoot, out mechRoot, out uiRoot);

        // Setup Atmosphere & Lighting
        EchoesLevelShell.SetupAtmosphere(blueprint);
        EchoesLevelShell.SpawnLevelLightingSettings(envRoot, blueprint);
        EchoesLevelShell.SpawnDirectionalLight(blueprint);

        // Keep track of instantiated modules to wire them up afterwards
        List<InstantiatedModule> instantiated = new List<InstantiatedModule>();
        Vector3 playerPos = Vector3.zero;

        // Place Modules
        foreach (var placement in blueprint.modules)
        {
            if (placement.type == ModuleType.PlayerStart)
            {
                playerPos = placement.position;
                continue;
            }

            GameObject obj = EchoesLevelFactoryBuild(placement, envRoot, mechRoot);
            if (obj != null)
            {
                instantiated.Add(new InstantiatedModule
                {
                    config = placement,
                    gameObject = obj
                });
            }
        }

        // Post-spawn signal wiring
        WireSignals(instantiated, mechRoot);

        // Find primary exit / goal for camera target
        LevelExit exit = Object.FindAnyObjectByType<LevelExit>();
        Transform goalFocus = exit != null ? exit.transform : null;

        // Spawn player and camera
        GameObject player = EchoesLevelShell.SpawnPlayer(playerPos, blueprint);
        EchoesLevelShell.SpawnGameplayCamera(player.transform, new Vector3(-8f, 9f, -12f), goalFocus);

        // Spawn boilerplate environment elements
        EchoesLevelShell.SpawnAmbientLights(envRoot, Vector3.zero, 30f, 40f);
        EchoesLevelShell.SpawnUI(uiRoot);

        // Spawn Level Runtime Controller
        EchoesLevelShell.SpawnLevelRuntime(mechRoot, blueprint);

        // Spawn Pacing and Experience systems
        float routeStartZ = -10f;
        float routeEndZ = 30f;
        if (exit != null) routeEndZ = exit.transform.position.z;
        EchoesLevelShell.SpawnExperienceSystems(mechRoot, envRoot, exit, blueprint, routeStartZ, routeEndZ);

        // Save Scene
        EditorSceneManager.SaveScene(scene, $"{SceneRoot}/{blueprint.levelName}.unity");
        Debug.Log($"[Echoes Blueprint Builder] Scene saved: {blueprint.levelName}");
    }

    private static GameObject EchoesLevelFactoryBuild(ModulePlacement placement, Transform envRoot, Transform mechRoot)
    {
        return EchoesModuleFactory.BuildModule(placement, envRoot, mechRoot);
    }

    private static void WireSignals(List<InstantiatedModule> instantiated, Transform mechRoot)
    {
        // Setup a lookup dictionary by name
        Dictionary<string, GameObject> nameLookup = new Dictionary<string, GameObject>();
        foreach (var inst in instantiated)
        {
            if (!nameLookup.ContainsKey(inst.config.name))
            {
                nameLookup.Add(inst.config.name, inst.gameObject);
            }
        }

        // 1. Wire DoorControllers to their PressurePlates
        foreach (var inst in instantiated)
        {
            if (inst.config.type == ModuleType.Door)
            {
                DoorController door = inst.gameObject.GetComponent<DoorController>();
                if (door != null && inst.config.targetSignals != null)
                {
                    List<PressurePlate> matchedPlates = new List<PressurePlate>();
                    foreach (string sig in inst.config.targetSignals)
                    {
                        if (nameLookup.TryGetValue(sig, out GameObject plateObj))
                        {
                            PressurePlate p = plateObj.GetComponent<PressurePlate>();
                            if (p != null) matchedPlates.Add(p);
                        }
                    }
                    SetSerializedValue(door, "plates", matchedPlates.ToArray());
                }
            }
        }

        // 2. Wire ResonanceSystem to its PuzzleSignal
        foreach (var inst in instantiated)
        {
            if (inst.config.type == ModuleType.ResonanceChamber)
            {
                ResonanceSystem resonance = inst.gameObject.GetComponent<ResonanceSystem>();
                if (resonance != null && inst.config.targetSignals != null && inst.config.targetSignals.Length > 0)
                {
                    // Find puzzle signal
                    string sigName = inst.config.targetSignals[0];
                    // We can check if there's a PuzzleSignal component in the hierarchy
                    PuzzleSignal signal = Object.FindAnyObjectByType<PuzzleSignal>();
                    if (signal != null && signal.gameObject.name == sigName)
                    {
                        SetSerializedValue(resonance, "targetSignal", signal);
                    }
                }
            }
        }

        // 3. Wire LevelGoals to their triggers (Plates or Signals)
        foreach (var inst in instantiated)
        {
            if (inst.config.type == ModuleType.LevelGoal)
            {
                LevelGoal goal = inst.gameObject.GetComponent<LevelGoal>();
                if (goal != null && inst.config.targetSignals != null)
                {
                    int triggerIdx = 1;
                    foreach (string sig in inst.config.targetSignals)
                    {
                        if (nameLookup.TryGetValue(sig, out GameObject targetObj))
                        {
                            PressurePlate plate = targetObj.GetComponent<PressurePlate>();
                            if (plate != null)
                            {
                                CreateGoalTrigger(inst.gameObject.transform, plate, "Memoria " + triggerIdx);
                                triggerIdx++;
                                continue;
                            }

                            PuzzleSignal signal = targetObj.GetComponent<PuzzleSignal>();
                            if (signal != null)
                            {
                                CreateGoalTrigger(inst.gameObject.transform, signal, "Memoria " + triggerIdx);
                                triggerIdx++;
                            }
                        }
                    }
                }
            }
        }

        // 4. Wire MovingPlatforms to their PressurePlates
        foreach (var inst in instantiated)
        {
            if (inst.config.type == ModuleType.MovingPlatform)
            {
                TimedMovingPlatform platform = inst.gameObject.GetComponentInChildren<TimedMovingPlatform>();
                if (platform != null && inst.config.targetSignals != null && inst.config.targetSignals.Length > 0)
                {
                    string sig = inst.config.targetSignals[0];
                    if (nameLookup.TryGetValue(sig, out GameObject plateObj))
                    {
                        PressurePlate p = plateObj.GetComponent<PressurePlate>();
                        if (p != null) SetSerializedValue(platform, "plate", p);
                    }
                }
            }
        }

        // 5. Wire PuzzleConditions
        foreach (var inst in instantiated)
        {
            if (inst.config.type == ModuleType.PuzzleCondition)
            {
                PuzzleCondition condition = inst.gameObject.GetComponent<PuzzleCondition>();
                if (condition != null && inst.config.targetSignals != null)
                {
                    List<PressurePlate> condPlates = new List<PressurePlate>();
                    PuzzleSignal targetSig = null;
                    List<DoorController> doors = new List<DoorController>();

                    foreach (string sig in inst.config.targetSignals)
                    {
                        if (nameLookup.TryGetValue(sig, out GameObject targetObj))
                        {
                            PressurePlate p = targetObj.GetComponent<PressurePlate>();
                            if (p != null) condPlates.Add(p);

                            PuzzleSignal s = targetObj.GetComponent<PuzzleSignal>();
                            if (s != null) targetSig = s;

                            DoorController d = targetObj.GetComponent<DoorController>();
                            if (d != null) doors.Add(d);
                        }
                    }

                    SetSerializedValue(condition, "plates", condPlates.ToArray());
                    if (targetSig != null) SetSerializedValue(condition, "targetSignal", targetSig);
                    if (doors.Count > 0) SetSerializedValue(condition, "doorsToOpen", doors.ToArray());
                }
            }
        }

        // 6. Wire HazardFields (EchoShieldField)
        foreach (var inst in instantiated)
        {
            if (inst.config.type == ModuleType.HazardField)
            {
                EchoShieldField hazard = inst.gameObject.GetComponent<EchoShieldField>();
                if (hazard != null && inst.config.targetSignals != null && inst.config.targetSignals.Length > 0)
                {
                    string sigName = inst.config.targetSignals[0];
                    if (nameLookup.TryGetValue(sigName, out GameObject sigObj))
                    {
                        PuzzleSignal s = sigObj.GetComponent<PuzzleSignal>();
                        if (s != null) SetSerializedValue(hazard, "signal", s);
                    }
                }
            }
        }

        // 7. Wire ConflictTraps (EchoConflictTrap)
        foreach (var inst in instantiated)
        {
            if (inst.config.type == ModuleType.ConflictTrap)
            {
                EchoConflictTrap trap = inst.gameObject.GetComponent<EchoConflictTrap>();
                if (trap != null && inst.config.targetSignals != null)
                {
                    List<DoorController> closeDoors = new List<DoorController>();
                    PuzzleSignal targetSig = null;

                    foreach (string sig in inst.config.targetSignals)
                    {
                        if (nameLookup.TryGetValue(sig, out GameObject targetObj))
                        {
                            DoorController d = targetObj.GetComponent<DoorController>();
                            if (d != null) closeDoors.Add(d);

                            PuzzleSignal s = targetObj.GetComponent<PuzzleSignal>();
                            if (s != null) targetSig = s;
                        }
                    }

                    SetSerializedValue(trap, "doorsToClose", closeDoors.ToArray());
                    if (targetSig != null) SetSerializedValue(trap, "signal", targetSig);
                }
            }
        }

        // 8. Wire MomentumRelays (EchoKineticZone)
        foreach (var inst in instantiated)
        {
            if (inst.config.type == ModuleType.MomentumRelay)
            {
                EchoKineticZone relay = inst.gameObject.GetComponent<EchoKineticZone>();
                if (relay != null && inst.config.targetSignals != null && inst.config.targetSignals.Length > 0)
                {
                    string targetName = inst.config.targetSignals[0];
                    GameObject targetObj = GameObject.Find(targetName);
                    if (targetObj != null)
                    {
                        SetSerializedValue(relay, "momentumRelayTarget", targetObj.transform);
                    }
                }
            }
        }
    }

    private static GoalTrigger CreateGoalTrigger(Transform parent, PressurePlate plate, string displayName)
    {
        GameObject triggerObject = new GameObject(displayName.Replace(" ", string.Empty) + "_Goal");
        triggerObject.transform.SetParent(parent, false);
        if (plate != null)
            triggerObject.transform.position = plate.transform.position + new Vector3(0f, 0.4f, 0f);

        GoalTrigger trigger = triggerObject.AddComponent<GoalTrigger>();
        SetSerializedValue(trigger, "displayName", displayName);
        SetSerializedValue(trigger, "pressurePlate", plate);
        SetSerializedValue(trigger, "usePlatePressedState", true);
        SetSerializedValue(trigger, "useDoorOpenState", false);
        SetSerializedValue(trigger, "accumulateOnce", true);
        return trigger;
    }

    private static GoalTrigger CreateGoalTrigger(Transform parent, PuzzleSignal signal, string displayName)
    {
        GameObject triggerObject = new GameObject(displayName.Replace(" ", string.Empty) + "_Goal");
        triggerObject.transform.SetParent(parent, false);
        if (signal != null)
            triggerObject.transform.position = signal.transform.position;

        GoalTrigger trigger = triggerObject.AddComponent<GoalTrigger>();
        SetSerializedValue(trigger, "displayName", displayName);
        SetSerializedValue(trigger, "puzzleSignal", signal);
        SetSerializedValue(trigger, "usePlatePressedState", false);
        SetSerializedValue(trigger, "useDoorOpenState", false);
        SetSerializedValue(trigger, "usePuzzleSignalState", true);
        SetSerializedValue(trigger, "accumulateOnce", true);
        return trigger;
    }

    private static void SetSerializedValue(object component, string propertyName, object value)
    {
        if (component is UnityEngine.Object unityObj)
        {
            SerializedObject serializedObject = new SerializedObject(unityObj);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null) return;

            switch (value)
            {
                case int intValue:
                    property.intValue = intValue;
                    break;
                case float floatValue:
                    property.floatValue = floatValue;
                    break;
                case bool boolValue:
                    property.boolValue = boolValue;
                    break;
                case string stringValue:
                    property.stringValue = stringValue;
                    break;
                case Color colorValue:
                    property.colorValue = colorValue;
                    break;
                case UnityEngine.Object objectValue:
                    property.objectReferenceValue = objectValue;
                    break;
                case Vector3 vectorValue:
                    property.vector3Value = vectorValue;
                    break;
                case System.Enum enumValue:
                    property.enumValueIndex = System.Convert.ToInt32(enumValue);
                    break;
                case UnityEngine.Object[] arrayValue:
                    property.ClearArray();
                    for (int i = 0; i < arrayValue.Length; i++)
                    {
                        property.InsertArrayElementAtIndex(i);
                        property.GetArrayElementAtIndex(i).objectReferenceValue = arrayValue[i];
                    }
                    break;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private struct InstantiatedModule
    {
        public ModulePlacement config;
        public GameObject gameObject;
    }

    [MenuItem("Echoes of You/Diagnóstico/Listar Modelos Reales (excluye SciFi)", false, 300)]
    public static void ListRealProjectAssets()
    {
        string[] modelGuids = AssetDatabase.FindAssets("t:Model");
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== MODELOS 3D EN Assets/3D Models (excluyendo SciFi MegaKit) ===");
        foreach (string guid in modelGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.StartsWith("Assets/3D Models/") && !path.Contains("SciFi"))
                sb.AppendLine(path);
        }
        string outputPath = "Assets/_RealAssetManifest.txt";
        File.WriteAllText(outputPath, sb.ToString());
        AssetDatabase.Refresh();
        Debug.Log($"[Echoes Production] Manifiesto escrito en {outputPath}");
        EditorUtility.RevealInFinder(outputPath);
    }
}
