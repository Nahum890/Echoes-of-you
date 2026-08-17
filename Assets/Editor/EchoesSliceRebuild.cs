using System.Collections.Generic;
using System.IO;
using Echoes.Narrative;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// Vertical-slice rebuild for N01-N03 (Echoes of You 2.0).
///
/// Reauthors the three LevelBlueprint assets with place-first liminal-school
/// layouts (correct ModuleType enum indices), builds each scene with the
/// existing EchoesNewProductionBuilder, then patches the post-build gaps
/// (autoRelease / latchOpen / fastReturn / anyTriggerSatisfiesGoal /
/// skipEscapeSequence / completionLine), wires the N03 capability unlock,
/// drops a greybox statue with a shadow director, and finally applies the
/// liminal-school game feel (fog raised to 0.012, hard shadows, post-proc
/// tuned to POST_PROCESSING_SPEC).
///
/// This script NEVER invokes "Build All": it only rebuilds the three slice
/// scenes, leaving N04-N15 decoration intact.
/// </summary>
public static class EchoesSliceRebuild
{
    const string BlueprintRoot = "Assets/Data/Levels";
    const string SceneRoot = "Assets/Scenes";

    // Liminal atmosphere — Chapter I raised toward the feel the user asked for.
    static readonly Color FogColorI   = Hex("#1C2430");
    static readonly Color AmbientI   = Hex("#0F141A");
    static readonly Color SunColor    = Hex("#F2F2FF");
    static readonly Vector3 SunRot    = new Vector3(50f, -30f, 0f);
    const float SunIntensity = 0.85f;
    const float FogDensitySlice = 0.012f; // spec cap.I = 0.008; raised per user request

    [MenuItem("Echoes of You/Slice Rebuild/1. Author N01-N03 Blueprints", false, 210)]
    public static void AuthorBlueprints()
    {
        AuthorLevel01();
        AuthorLevel02();
        AuthorLevel03();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[SliceRebuild] Blueprints N01-N03 authored.");
    }

    [MenuItem("Echoes of You/Slice Rebuild/2. Build N01-N03 Scenes", false, 211)]
    [MenuItem("Echoes of You/Slice Rebuild/3. Reapply Patches (fix save)", false, 212)]
    public static void ReapplyPatches()
    {
        foreach (int n in new[] { 1, 2, 3 })
        {
            string path = $"{SceneRoot}/Level_{n:00}.unity";
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[SliceRebuild] No se pudo abrir {path}");
                continue;
            }
            PatchScene(n);
            ApplyGameFeel(n);
            SaveActiveScene();
        }
        Debug.Log("[SliceRebuild] Patches re-applied and saved on N01-N03.");
    }

    public static void BuildScenes()
    {
        foreach (int n in new[] { 1, 2, 3 })
        {
            var bp = LoadBlueprint(n);
            if (bp == null) { Debug.LogError($"[SliceRebuild] Blueprint Level_{n:00} not found."); continue; }
            Debug.Log($"[SliceRebuild] Building {bp.levelName}...");
            EchoesNewProductionBuilder.BuildLevelFromBlueprint(bp);
            PatchScene(n);
            ApplyGameFeel(n);
            SaveActiveScene();
        }
        Debug.Log("[SliceRebuild] N01-N03 scenes rebuilt, patched, and feel applied.");
    }

    [MenuItem("Echoes of You/Slice Rebuild/3. Run All (Author + Build)", false, 212)]
    public static void RunAll()
    {
        AuthorBlueprints();
        BuildScenes();
    }

    // ═══════════════════════════════════════════════════════════════════
    // BLUEPRINT AUTHORING
    // ═══════════════════════════════════════════════════════════════════

    static LevelBlueprint AuthorLevel01()
    {
        var bp = LoadBlueprint(1);
        bp.levelName = "Level_01";
        bp.nextLevel = "Level_02";
        bp.actNumber = 1;
        bp.echoEnabled = true;
        bp.maxEchoes = 1;
        bp.maxRecordSeconds = 20f; // Biblia: 20s (unifies the 6/10/20 inconsistency for the slice)
        bp.fogColor = FogColorI;
        bp.fogDensity = FogDensitySlice;
        bp.skyColor = Hex("#06080C");
        bp.ambientColor = AmbientI;
        bp.directionalLightRotation = SunRot;
        bp.directionalLightColor = SunColor;
        bp.directionalLightIntensity = SunIntensity;
        bp.narrativeIntroTitle = "Nivel 1 — Desorientación";
        bp.narrativeIntroDesc = "Un porche silencioso abre a un pasillo escolar sin fin. El primer eco de Lyra repite la estructura.";
        bp.narrativeIntroDuration = 6f;
        bp.puzzleObjectiveText = "Tu eco abre el umbral que el presente no puede.";
        bp.puzzleActiveText = "Graba un camino corto; el eco cargará la placa por ti.";
        bp.puzzleCompleteText = "Desorientación superada.";
        bp.recordFuture = false;
        bp.inversionCamera = false;
        bp.timingFloor = 0.4f;
        bp.pathHints = new Vector3[]
        {
            New3(0, 0.1f, -3),
            New3(0, 0.1f, 8),
            New3(0, 0.1f, 24),
            New3(0, 0.1f, 44),
        };

        bp.modules.Clear();
        // Entrance hall — wide, liminal, low light.
        bp.modules.Add(Mod("Entrada", ModuleType.SchoolEntrance, New3(0, 0, -4), New3(8, 3f, 6)));
        bp.modules.Add(Mod("PlayerStart", ModuleType.PlayerStart, New3(0, 0.1f, -6)));
        // Spine corridor.
        bp.modules.Add(Mod("PasilloA", ModuleType.SchoolCorridor, New3(0, 0, 6), New3(5, 3f, 20), "flicker=true"));
        bp.modules.Add(Mod("PasilloB", ModuleType.SchoolCorridor, New3(0, 0, 24), New3(5, 3f, 18), "flicker=false"));
        // Teaching puzzle: ONE echo-only plate that holds the door, long auto-release
        // so the player can feel "the past opens what the present cannot".
        bp.modules.Add(Mod("PlacaEco_Aula", ModuleType.PressurePlate, New3(0, 0.05f, 14), Vector3.zero, "EchoOnly"));
        bp.modules.Add(Mod("PuertaAula", ModuleType.Door, New3(0, 0, 30), New3(4, 3, 0.5f), sig: Sig("PlacaEco_Aula")));
        bp.modules.Add(Mod("AulaAusente", ModuleType.SchoolClassroom, New3(0, 0, 38), New3(8, 3f, 8)));
        // Exit + goal.
        bp.modules.Add(Mod("LevelExit_Area", ModuleType.LevelExit, New3(0, 0.1f, 44), Vector3.zero, "Level_02"));
        bp.modules.Add(Mod("LevelGoal", ModuleType.LevelGoal, New3(0, 0.1f, 44), Vector3.zero,
            "Tu eco abre el umbral que el presente no puede.|Graba un camino corto; el eco cargará la placa por ti.|Desorientación superada.",
            Sig("PlacaEco_Aula")));
        bp.modules.Add(Mod("LevelRuntime", ModuleType.LevelRuntime, Vector3.zero, Vector3.zero,
            "Avanza por el pasillo hasta el umbral.|El pasado se duplica.|Salida alcanzada."));
        // Subtle teaching hint light over the plate.
        bp.modules.Add(Mod("Hint_Placa", ModuleType.PointLight, New3(0, 3, 14), Vector3.zero, "FFBF00,1.6,6"));

        EditorUtility.SetDirty(bp);
        return bp;
    }

    static LevelBlueprint AuthorLevel02()
    {
        var bp = LoadBlueprint(2);
        bp.levelName = "Level_02";
        bp.nextLevel = "Level_03";
        bp.actNumber = 1;
        bp.echoEnabled = true;
        bp.maxEchoes = 2;
        bp.maxRecordSeconds = 20f;
        bp.fogColor = FogColorI;
        bp.fogDensity = FogDensitySlice;
        bp.skyColor = Hex("#05070C");
        bp.ambientColor = AmbientI;
        bp.directionalLightRotation = SunRot;
        bp.directionalLightColor = SunColor;
        bp.directionalLightIntensity = SunIntensity;
        bp.narrativeIntroTitle = "Nivel 2 — Repetición";
        bp.narrativeIntroDesc = "Aulas flanquean un corredor estrecho. Dos presiones al mismo tiempo son más que dos presiones.";
        bp.narrativeIntroDuration = 6f;
        bp.puzzleObjectiveText = "Sostén las dos placas a la vez: el eco carga una mientras tú cargas la otra.";
        bp.puzzleActiveText = "La sincronía del pasado y el presente.";
        bp.puzzleCompleteText = "Repetición superada.";
        bp.recordFuture = false;
        bp.inversionCamera = false;
        bp.timingFloor = 0.4f;
        bp.pathHints = new Vector3[]
        {
            New3(0, 0.1f, 0),
            New3(0, 0.1f, 12),
            New3(0, 0.1f, 28),
            New3(0, 0.1f, 44),
        };

        bp.modules.Clear();
        // 4m central corridor (per approved spec fix) — tight, school feel.
        bp.modules.Add(Mod("Entrada", ModuleType.SchoolEntrance, New3(0, 0, 0), New3(4, 3f, 6)));
        bp.modules.Add(Mod("PlayerStart", ModuleType.PlayerStart, New3(0, 0.1f, -2)));
        bp.modules.Add(Mod("CorredorCentral", ModuleType.SchoolCorridor, New3(0, 0, 14), New3(4, 3.2f, 22), "flicker=true"));
        // Two classrooms flanking the corridor with vision window (Arch_WallWindow placed post-build).
        bp.modules.Add(Mod("AulaIzquierda", ModuleType.SchoolClassroom, New3(-6, 0, 14), New3(6, 3f, 10), "style=ordered"));
        bp.modules.Add(Mod("AulaDerecha",  ModuleType.SchoolClassroom, New3( 6, 0, 14), New3(6, 3f, 10), "style=ordered"));
        // Combination puzzle: plate in left classroom (echo can hold), plate in corridor (player holds).
        // Both must be pressed simultaneously → door opens.
        bp.modules.Add(Mod("PlacaEco_Aula", ModuleType.PressurePlate, New3(-6, 0.05f, 14), Vector3.zero, "EchoOnly"));
        bp.modules.Add(Mod("PlacaJugador_Corredor", ModuleType.PressurePlate, New3(0, 0.05f, 14), Vector3.zero));
        bp.modules.Add(Mod("PuertaAula", ModuleType.Door, New3(0, 0, 27), New3(4, 3, 0.5f),
            sig: Sig("PlacaEco_Aula", "PlacaJugador_Corredor")));
        // Optional timed platform as a fast-return shortcut for re-attempts (block wired to Plate later).
        bp.modules.Add(Mod("PlataformaRapida", ModuleType.MovingPlatform, New3(0, 0, 34), New3(3, 0.3f, 3),
            "inactiveVec=0,0,0|activeVec=0,0,6|speed=2.5", sig: Sig("PlacaJugador_Corredor")));
        // Exit hall.
        bp.modules.Add(Mod("Hall_Salida", ModuleType.SchoolHall, New3(0, 0, 42), New3(8, 5f, 8)));
        bp.modules.Add(Mod("LevelExit_Area", ModuleType.LevelExit, New3(0, 0.1f, 46), Vector3.zero, "Level_03"));
        bp.modules.Add(Mod("LevelGoal", ModuleType.LevelGoal, New3(0, 0.1f, 46), Vector3.zero,
            "Sostén las dos placas a la vez.|La sincronía del pasado y el presente.|Repetición superada.",
            Sig("PlacaEco_Aula", "PlacaJugador_Corredor")));
        bp.modules.Add(Mod("LevelRuntime", ModuleType.LevelRuntime, Vector3.zero, Vector3.zero,
            "Avanza por el corredor de las aulas.|Coordina tu pasado con tu presente.|Aula final alcanzada."));
        bp.modules.Add(Mod("Hint_PlacaEco", ModuleType.PointLight, New3(-6, 3, 14), Vector3.zero, "FFBF00,1.4,5"));
        bp.modules.Add(Mod("Hint_PlacaJugador", ModuleType.PointLight, New3(0, 3, 14), Vector3.zero, "FFBF00,1.4,5"));

        EditorUtility.SetDirty(bp);
        return bp;
    }

    static LevelBlueprint AuthorLevel03()
    {
        var bp = LoadBlueprint(3);
        bp.levelName = "Level_03";
        bp.nextLevel = "Level_04";
        bp.actNumber = 1;
        bp.echoEnabled = true;
        bp.maxEchoes = 1;
        bp.maxRecordSeconds = 20f;
        bp.fogColor = FogColorI;
        bp.fogDensity = FogDensitySlice;
        bp.skyColor = Hex("#020306");
        bp.ambientColor = AmbientI;
        bp.directionalLightRotation = SunRot;
        bp.directionalLightColor = SunColor;
        bp.directionalLightIntensity = SunIntensity;
        bp.narrativeIntroTitle = "Nivel 3 — Bifurcación de Lyra";
        bp.narrativeIntroDesc = "Un pasillo se parte en dos. Tu eco puede ahogarte si se queda después de ayudar.";
        bp.narrativeIntroDuration = 10f;
        bp.puzzleObjectiveText = "Elige una ruta; el eco sostiene la otra, pero si se queda demasiado te encierra.";
        bp.puzzleActiveText = "El eco es tu llave y tu cárcel.";
        bp.puzzleCompleteText = "Bifurcación resuelta.";
        bp.recordFuture = false;
        bp.inversionCamera = false;
        bp.timingFloor = 0.4f;
        bp.pathHints = new Vector3[]
        {
            New3(0, 0.1f, 0),
            New3(-5, 0.1f, 14),
            New3(5, 0.1f, 14),
            New3(0, 0.1f, 30),
        };

        bp.modules.Clear();
        // Entrance + bifurcating corridor.
        bp.modules.Add(Mod("Entrada", ModuleType.SchoolEntrance, New3(0, 0, -2), New3(6, 3f, 6)));
        bp.modules.Add(Mod("PlayerStart", ModuleType.PlayerStart, New3(0, 0.1f, -4)));
        bp.modules.Add(Mod("CorredorBifurcacion", ModuleType.SchoolCorridor, New3(0, 0, 6), New3(6, 3.2f, 10), "flicker=true"));
        // Two branches.
        bp.modules.Add(Mod("RamaIzquierda", ModuleType.SchoolCorridor, New3(-5, 0, 16), New3(4, 3f, 14), "flicker=true"));
        bp.modules.Add(Mod("RamaDerecha",  ModuleType.SchoolCorridor, New3( 5, 0, 16), New3(4, 3f, 14), "flicker=true"));
        // "Aula de Lyra" on the left — keeps the amber memory prop (statue lives here).
        bp.modules.Add(Mod("AulaLyra", ModuleType.SchoolLyraClassroom, New3(-5, 0, 16), New3(5, 3f, 8)));
        bp.modules.Add(Mod("AulaEco",  ModuleType.SchoolClassroom,     New3( 5, 0, 16), New3(5, 3f, 8)));
        // Each branch has a plate; EITHER plate satisfies the goal (anyTriggerSatisfiesGoal = true),
        // so the player can route, but the echo must hold the other. Twist: the echo plate has a
        // long auto-release, and if the player lingers on their plate after the echo releases, the
        // consequence door (left, non-latched) closes — demonstrated by StatueShadowDirector.
        bp.modules.Add(Mod("PlacaEco_RamaDerecha", ModuleType.PressurePlate, New3(5, 0.05f, 16), Vector3.zero));
        bp.modules.Add(Mod("PlacaJugador_RamaIzquierda", ModuleType.PressurePlate, New3(-5, 0.05f, 16), Vector3.zero));
        // Branch doors — left is NOT latched (twist), right is latched (safe).
        bp.modules.Add(Mod("PuertaRamaIzquierda", ModuleType.Door, New3(-5, 0, 22), New3(4, 3, 0.5f),
            sig: Sig("PlacaJugador_RamaIzquierda")));
        bp.modules.Add(Mod("PuertaRamaDerecha", ModuleType.Door, New3(5, 0, 22), New3(4, 3, 0.5f),
            sig: Sig("PlacaEco_RamaDerecha")));
        // Convergence hall with the founder statue (greybox placed post-build).
        bp.modules.Add(Mod("Hall_Estatua", ModuleType.SchoolHall, New3(0, 0, 30), New3(10, 5f, 10)));
        // Exit + goal. anyTriggerSatisfiesGoal lets either branch reach the exit.
        bp.modules.Add(Mod("LevelExit_Area", ModuleType.LevelExit, New3(0, 0.1f, 36), Vector3.zero, "Level_04"));
        bp.modules.Add(Mod("LevelGoal", ModuleType.LevelGoal, New3(0, 0.1f, 36), Vector3.zero,
            "Elige una ruta; el eco sostiene la otra.|El eco es tu llave y tu cárcel.|Bifurcación resuelta.",
            Sig("PlacaJugador_RamaIzquierda", "PlacaEco_RamaDerecha")));
        bp.modules.Add(Mod("LevelRuntime", ModuleType.LevelRuntime, Vector3.zero, Vector3.zero,
            "Recorre una de las ramas hasta la estatua.|El eco se queda atrás.|La bifurcación se resuelve."));
        bp.modules.Add(Mod("Hint_Estatua", ModuleType.PointLight, New3(0, 3f, 30), Vector3.zero, "FFBF00,2,8"));

        EditorUtility.SetDirty(bp);
        return bp;
    }

    // ═══════════════════════════════════════════════════════════════════
    // POST-BUILD PATCHES
    // ═══════════════════════════════════════════════════════════════════

    static void PatchScene(int level)
    {
        Debug.Log($"[SliceRebuild] Patching Level_{level:00}...");

        switch (level)
        {
            case 1: PatchLevel1(); break;
            case 2: PatchLevel2(); break;
            case 3: PatchLevel3(); break;
        }

        // Common: Goal skipEscapeSequence + LevelRuntime completionLine.
        var goal = Object.FindAnyObjectByType<LevelGoal>();
        if (goal != null)
        {
            SetPrivate(goal, "skipEscapeSequence", true);
            SetPrivate(goal, "anyTriggerSatisfiesGoal", level == 3);
            EditorUtility.SetDirty(goal);
        }
        var runtime = Object.FindAnyObjectByType<LevelRuntimeController>();
        if (runtime != null)
        {
            SetPrivate(runtime, "completionLine", "Recuerdo restaurado.");
            EditorUtility.SetDirty(runtime);
        }
    }

    static void PatchLevel1()
    {
        // Echo plate: long auto-release so the held door gives the player time to walk through.
        var plate = FindPressurePlateByNameContains("PlacaEco_Aula");
        if (plate != null) { plate.autoReleaseTimer = 8.0f; EditorUtility.SetDirty(plate); }
        // Door latches open once the echo has pushed it (player never needs to re-press).
        var door = FindGameObjectByNameContains("PuertaAula")?.GetComponent<DoorController>();
        if (door != null) { door.latchOpen = true; EditorUtility.SetDirty(door); }
    }

    static void PatchLevel2()
    {
        // Echo plate: short hold keeps the door; the player must press their plate concurrently.
        var eco = FindPressurePlateByNameContains("PlacaEco_Aula");
        if (eco != null) { eco.autoReleaseTimer = 0.15f; EditorUtility.SetDirty(eco); }
        // Player plate: no auto-release (held while standing).
        var player = FindPressurePlateByNameContains("PlacaJugador_Corredor");
        if (player != null) { player.autoReleaseTimer = 0f; EditorUtility.SetDirty(player); }
        // Door latches open once both plates have triggered it, so the timed-platform can be used.
        var door = FindGameObjectByNameContains("PuertaAula")?.GetComponent<DoorController>();
        if (door != null) { door.latchOpen = true; EditorUtility.SetDirty(door); }
        // Moving platform: fast return so a failed attempt resets quickly.
        var platform = FindGameObjectByNameContains("PlataformaRapida")?.GetComponentInChildren<TimedMovingPlatform>();
        if (platform != null) { platform.fastReturn = true; platform.returnMultiplier = 8f; EditorUtility.SetDirty(platform); }
        // Vision window between the two flanking classrooms (ventanal).
        SpawnArchWindowBetween("AulaIzquierda", "AulaDerecha");
    }

    static void PatchLevel3()
    {
        // Echo plate on the right branch: medium auto-release — the twist is "if the echo
        // stays too long it locks the left path behind you" (left door is not latched).
        var eco = FindPressurePlateByNameContains("PlacaEco_RamaDerecha");
        if (eco != null) { eco.autoReleaseTimer = 4.0f; EditorUtility.SetDirty(eco); }
        var player = FindPressurePlateByNameContains("PlacaJugador_RamaIzquierda");
        if (player != null) { player.autoReleaseTimer = 0f; EditorUtility.SetDirty(player); }
        // Left door stays NON-latched (twist: closes when the player leaves the plate).
        var leftDoor = FindGameObjectByNameContains("PuertaRamaIzquierda")?.GetComponent<DoorController>();
        if (leftDoor != null) { leftDoor.latchOpen = false; EditorUtility.SetDirty(leftDoor); }
        // Right door latches open (safe path, stays open).
        var rightDoor = FindGameObjectByNameContains("PuertaRamaDerecha")?.GetComponent<DoorController>();
        if (rightDoor != null) { rightDoor.latchOpen = true; EditorUtility.SetDirty(rightDoor); }

        // Greybox founder statue from Arch_Column in the convergence hall, tinted, shadow-casting.
        SpawnGreyboxStatue("Hall_Estatua");

        // Capability unlock bridge on the chief GoalTrigger child (N04 reads unlock_future_echo).
        // EchoCapabilityUnlocker requires a GoalTrigger (RequireComponent), so we bind it to an
        // existing goal trigger rather than the LevelGoal root.
        var goal = Object.FindAnyObjectByType<LevelGoal>();
        if (goal != null)
        {
            var triggers = goal.GetComponentsInChildren<GoalTrigger>(true);
            if (triggers != null && triggers.Length > 0)
            {
                var chief = triggers[0];
                if (chief.GetComponent<EchoCapabilityUnlocker>() == null)
                    chief.gameObject.AddComponent<EchoCapabilityUnlocker>();
            }
            else
            {
                Debug.LogWarning("[SliceRebuild] No GoalTrigger children on LevelGoal; capability unlocker skipped.");
            }
        }

        // Statue shadow director: rotates the statue and tilts the sun when the left-branch plate triggers.
        var statueGo = FindGameObjectByNameContains("EstatuaFundador");
        if (statueGo != null)
        {
            var dir = statueGo.GetComponent<StatueShadowDirector>();
            if (dir == null) dir = statueGo.AddComponent<StatueShadowDirector>();
            if (player != null) dir.BindPlate(player);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // GAME FEEL — fog, hard shadows, post-processing (liminal)
    // ═══════════════════════════════════════════════════════════════════

    static void ApplyGameFeel(int level)
    {
        // Fog — exponential, chapter color, slice density.
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = FogColorI;
        RenderSettings.fogDensity = FogDensitySlice;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = AmbientI;
        RenderSettings.ambientIntensity = 0.15f;

        // Sun: 0.85 lux, #F2F2FF, hard shadows, 40m distance.
        foreach (Light l in Object.FindObjectsByType<Light>(FindObjectsInactive.Include))
        {
            if (l.type == LightType.Directional)
            {
                l.color = SunColor;
                l.intensity = SunIntensity;
                l.transform.rotation = Quaternion.Euler(SunRot);
                l.shadows = LightShadows.Hard;
                // l.shadowDistance = 40f; // NOTE: Light.shadowDistance is editor-only;.URP uses the asset. Kept for completeness.
            }
            else
            {
                // Additional lights: hard shadows only (RULE-LGT-002), cap respected by builder.
                l.shadows = LightShadows.Hard;
            }
        }

        // Volume + VolumeProfile tuned to POST_PROCESSING_SPEC (liminal).
        EnsureLiminalVolume(level);
    }

    static void EnsureLiminalVolume(int level)
    {
        // Find or create a global volume per scene.
        Volume vol = Object.FindAnyObjectByType<Volume>();
        if (vol == null)
        {
            var go = new GameObject("Slice_GlobalVolume");
            vol = go.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.priority = 1f;
        }

        string profilePath = $"{BlueprintRoot}/../../Settings/Volumes/Slice_N{level:00}_PostProc.asset";
        profilePath = "Assets/Settings/Volumes/Slice_N" + level.ToString("00") + "_PostProc.asset";
        EnsureDir("Assets/Settings/Volumes");

        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, profilePath);
        }
        else
        {
            // Wipe existing overrides to re-author cleanly (destroy + clear list).
            for (int i = profile.components.Count - 1; i >= 0; i--)
            {
                if (profile.components[i] != null)
                    Object.DestroyImmediate(profile.components[i], true);
            }
            profile.components.Clear();
        }

        // Bloom
        var bloom = profile.Add<Bloom>();
        bloom.intensity.Override(0.25f);
        bloom.threshold.Override(0.90f);
        bloom.scatter.Override(0.70f);
        bloom.tint.Override(Color.white);
        bloom.highQualityFiltering.Override(false);

        // Vignette
        var vignette = profile.Add<Vignette>();
        vignette.intensity.Override(0.35f);
        vignette.smoothness.Override(0.40f);
        vignette.color.Override(Hex("#0D0D1A"));
        vignette.rounded.Override(false);

        // Color adjustments
        var ca = profile.Add<ColorAdjustments>();
        ca.postExposure.Override(-0.5f);
        ca.contrast.Override(15f);
        ca.colorFilter.Override(Color.white);
        ca.hueShift.Override(0f);
        ca.saturation.Override(-8f);

        // Tonemapping off
        var tone = profile.Add<Tonemapping>();
        tone.mode.Override(TonemappingMode.None);

        // VolumeProfile.Add<T>() creates the components in memory; Unity's editor
        // additionally registers them as sub-assets, otherwise they serialize as
        // {fileID: 0} and the overrides never apply.
        foreach (var comp in profile.components)
            AssetDatabase.AddObjectToAsset(comp, profile);

        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();

        vol.sharedProfile = profile;
        vol.weight = 1f;
        EditorUtility.SetDirty(vol);
    }

    // ═══════════════════════════════════════════════════════════════════
    // GAME FEEL PROPS — ventanal + greybox statue
    // ═══════════════════════════════════════════════════════════════════

    static void SpawnArchWindowBetween(string leftName, string rightName)
    {
        var left = FindGameObjectByNameContains(leftName);
        var right = FindGameObjectByNameContains(rightName);
        if (left == null || right == null) return;

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Architecture/Arch_WallWindow.prefab");
        if (prefab == null) return;

        Vector3 mid = (left.transform.position + right.transform.position) * 0.5f;
        // Place the window inside the corridor (parent to env root), oriented to face corridor.
        var parent = left.transform.parent;
        var win = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        win.name = "Ventanal_Aulas";
        win.transform.position = mid + new Vector3(0f, 1.4f, 0f);
        win.transform.rotation = Quaternion.Euler(0f, 90f, 0f); // look along the corridor
        EditorUtility.SetDirty(win);
    }

    static void SpawnGreyboxStatue(string hallName)
    {
        var hall = FindGameObjectByNameContains(hallName);
        if (hall == null) return;
        // Avoid duplicates.
        if (FindGameObjectByNameContains("EstatuaFundador") != null) return;

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Architecture/Arch_Column.prefab");
        if (prefab == null) return;

        var parent = hall.transform.parent;
        var statue = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        statue.name = "EstatuaFundador";
        statue.transform.position = hall.transform.position + new Vector3(0f, 0f, 0f);
        statue.transform.localScale = new Vector3(1.2f, 3.0f, 1.2f);
        // Rotate slightly to face the entrance, cast a long shadow.
        statue.transform.rotation = Quaternion.Euler(0f, 15f, 0f);

        // Tint the statue material to a weathered bronze / dust rose.
        var rend = statue.GetComponentInChildren<MeshRenderer>();
        if (rend != null)
        {
            var mat = new Material(rend.sharedMaterial ?? EchoesMaterialLibrary.MemoryMat);
            mat.name = "Mat_EstatuaFundador";
            mat.color = Hex("#4A3438");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", Hex("#4A3438") * 0.2f);
            rend.sharedMaterial = mat;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            rend.receiveShadows = true;
        }
        EditorUtility.SetDirty(statue);
    }

    // ═══════════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════════

    static LevelBlueprint LoadBlueprint(int level) =>
        AssetDatabase.LoadAssetAtPath<LevelBlueprint>($"{BlueprintRoot}/Level_{level:00}_Blueprint.asset");

    static ModulePlacement Mod(string name, ModuleType type, Vector3 pos,
        Vector3 scale = default, string customData = null, string[] sig = null) =>
        new ModulePlacement
        {
            name = name,
            type = type,
            position = pos,
            rotation = Vector3.zero,
            scale = scale,
            customData = customData ?? "",
            targetSignals = sig ?? System.Array.Empty<string>()
        };

    static string[] Sig(params string[] s) => s;
    static Vector3 New3(float x, float y, float z) => new Vector3(x, y, z);
    static Color Hex(string h) => ColorUtility.TryParseHtmlString(h, out var c) ? c : Color.white;

    static void SetPrivate(Object obj, string field, object value)
    {
        var so = new SerializedObject(obj);
        var p = so.FindProperty(field);
        if (p == null) return;
        switch (value)
        {
            case bool b: p.boolValue = b; break;
            case float f: p.floatValue = f; break;
            case int i: p.intValue = i; break;
            case string s: p.stringValue = s; break;
            case Color c: p.colorValue = c; break;
            case Vector3 v: p.vector3Value = v; break;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static GameObject FindGameObjectByNameContains(string contains)
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (go.name.Contains(contains) && go.scene.IsValid()) return go;
        }
        return null;
    }

    static PressurePlate FindPressurePlateByNameContains(string contains)
    {
        foreach (var p in Object.FindObjectsByType<PressurePlate>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (p != null && p.name.Contains(contains) && p.gameObject.scene.IsValid()) return p;
        }
        return null;
    }

    static void EnsureDir(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            EnsureDir(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }

    static void SaveActiveScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid()) return;
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("[SliceRebuild] Ignorando guardado durante play mode.");
            return;
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, scene.path);
        AssetDatabase.SaveAssets();
    }
}
