using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// EchoesMasterLightingDesigner
/// Technical & Lighting Artist utility for Echoes of You (N01 - N03).
/// Implements structured lighting hierarchy without destroying gameplay, triggers or colliders:
/// - Dominant Key Lights (Directional / Theatrical Spots)
/// - Secondary / Bounce Fill Lights (eliminating black crush)
/// - Focal Objectives (Pressure plates, doors, puzzles, goals)
/// - Architectural Fixtures (corridors & classrooms)
/// - Atmospheric Transitions (thresholds, window light spill, bifurcation guides)
/// </summary>
public static class EchoesMasterLightingDesigner
{
    private const string LightingRootName = "Hierarchy_Lights";
    private const string EnvRootName = "--- ENVIRONMENT ---";
    private const string LevelLightingName = "LevelLighting";

    [MenuItem("Echoes of You/Lighting/Apply Master Lighting Slice (N01-N03)", false, 100)]
    public static void ApplyToAllSliceScenes()
    {
        for (int i = 1; i <= 3; i++)
        {
            ApplyToLevel(i);
        }
        AssetDatabase.SaveAssets();
        if (!Application.isBatchMode)
            EditorUtility.DisplayDialog("Master Lighting Applied", "Levels 01, 02, and 03 master lighting hierarchy successfully applied!", "OK");
    }

    public static void ApplyToAllSliceScenesBatch()
    {
        ApplyToAllSliceScenes();
        EditorApplication.Exit(0);
    }

    [MenuItem("Echoes of You/Lighting/Apply Master Lighting to Open Scene", false, 101)]
    public static void ApplyToActiveScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        int level = ParseLevelNumber(scene.name);
        if (level < 1 || level > 3)
        {
            Debug.LogWarning($"[MasterLighting] '{scene.name}' is not Level 1, 2 or 3.");
            return;
        }
        ApplyToSceneDirect(scene, level);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[MasterLighting] Master lighting applied to active scene: Level {level}");
    }

    [MenuItem("Echoes of You/Lighting/Apply N01 Lighting", false, 110)]
    public static void ApplyN01() => ApplyToLevel(1);

    [MenuItem("Echoes of You/Lighting/Apply N02 Lighting", false, 111)]
    public static void ApplyN02() => ApplyToLevel(2);

    [MenuItem("Echoes of You/Lighting/Apply N03 Lighting", false, 112)]
    public static void ApplyN03() => ApplyToLevel(3);

    public static void ApplyToLevel(int level)
    {
        string scenePath = $"Assets/Scenes/Level_{level:D2}.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError($"[MasterLighting] Could not open scene {scenePath}");
            return;
        }

        ApplyToSceneDirect(scene, level);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[MasterLighting] Level_{level:D2} master lighting applied and saved successfully.");
    }

    public static void ApplyToSceneDirect(Scene scene, int level)
    {
        // 1. Configure Post-Processing Volume Profile
        ConfigureVolumeProfile(level);

        // 2. Configure Directional Light and RenderSettings
        ConfigureEnvironmentAndSun(level);

        // 3. Clean and build structured lighting container
        Transform container = GetOrCreateLightingContainer();

        // 4. Build Level-Specific Lighting Hierarchy
        switch (level)
        {
            case 1:
                BuildLevel01Hierarchy(container);
                break;
            case 2:
                BuildLevel02Hierarchy(container);
                break;
            case 3:
                BuildLevel03Hierarchy(container);
                break;
        }

        // 5. Clean up any redundant legacy flat point lights under environment root
        CleanLegacyRedundantLights();

        // 6. Ensure Light counts and shadows conform to PS1/PS2 spec
        ValidateAndEnforceHardShadows();
    }

    private static int ParseLevelNumber(string name)
    {
        if (string.IsNullOrEmpty(name) || !name.StartsWith("Level_")) return -1;
        if (int.TryParse(name.Substring(6, 2), out int n)) return n;
        return -1;
    }

    // =========================================================================
    // 1. VOLUME PROFILE CONFIGURATION
    // =========================================================================

    private static void ConfigureVolumeProfile(int level)
    {
        string profilePath = $"Assets/Settings/Volumes/Slice_N{level:D2}_PostProc.asset";
        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);

        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, profilePath);
        }
        else
        {
            for (int i = profile.components.Count - 1; i >= 0; i--)
            {
                if (profile.components[i] != null)
                    Object.DestroyImmediate(profile.components[i], true);
            }
            profile.components.Clear();
        }

        // Bloom
        var bloom = profile.Add<Bloom>();
        bloom.active = true;
        bloom.highQualityFiltering.Override(false);
        bloom.scatter.Override(0.70f);

        // Color Adjustments
        var ca = profile.Add<ColorAdjustments>();
        ca.active = true;

        // Vignette
        var vignette = profile.Add<Vignette>();
        vignette.active = true;
        vignette.rounded.Override(false);

        // Tonemapping - None for crisp PS1/PS2 lo-fi palette
        var tone = profile.Add<Tonemapping>();
        tone.active = true;
        tone.mode.Override(TonemappingMode.None);

        if (level == 1)
        {
            // Level 1: Familiar, melancholic, institutional realism
            bloom.intensity.Override(0.45f);
            bloom.threshold.Override(0.88f);
            bloom.tint.Override(Hex("#FFFBF0"));

            ca.postExposure.Override(0.30f); // Recover shadows & midtones from black crush
            ca.contrast.Override(20f);
            ca.saturation.Override(-6f);
            ca.colorFilter.Override(Hex("#FAFBF7"));

            vignette.intensity.Override(0.24f);
            vignette.smoothness.Override(0.45f);
            vignette.color.Override(Hex("#0D0F18"));
        }
        else if (level == 2)
        {
            // Level 2: Uncanny repetition, chromatic duality
            bloom.intensity.Override(0.50f);
            bloom.threshold.Override(0.85f);
            bloom.tint.Override(Hex("#FFECC8"));

            ca.postExposure.Override(0.35f);
            ca.contrast.Override(22f);
            ca.saturation.Override(-10f);
            ca.colorFilter.Override(Hex("#F2F6F0"));

            vignette.intensity.Override(0.26f);
            vignette.smoothness.Override(0.48f);
            vignette.color.Override(Hex("#0A0E18"));
        }
        else if (level == 3)
        {
            // Level 3: Perceptual fragmentation, theatrical chiaroscuro
            bloom.intensity.Override(0.60f);
            bloom.threshold.Override(0.82f);
            bloom.tint.Override(Hex("#F0E6FF"));

            ca.postExposure.Override(0.40f);
            ca.contrast.Override(25f);
            ca.saturation.Override(-12f);
            ca.colorFilter.Override(Hex("#F4F0F8"));

            vignette.intensity.Override(0.30f);
            vignette.smoothness.Override(0.50f);
            vignette.color.Override(Hex("#08081A"));
        }

        foreach (var comp in profile.components)
        {
            AssetDatabase.AddObjectToAsset(comp, profile);
        }

        EditorUtility.SetDirty(profile);

        // Ensure scene volume references this profile
        Volume vol = Object.FindAnyObjectByType<Volume>();
        if (vol == null)
        {
            var go = new GameObject("Slice_GlobalVolume");
            vol = go.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.priority = 1f;
        }
        vol.sharedProfile = profile;
        vol.weight = 1f;
        EditorUtility.SetDirty(vol);
    }

    // =========================================================================
    // 2. ENVIRONMENT & SUN SETTINGS
    // =========================================================================

    private static void ConfigureEnvironmentAndSun(int level)
    {
        Color fogColor = Hex("#1C2430");
        float fogDensity = 0.008f;
        Color ambientColor = Hex("#181F28");

        if (level == 1)
        {
            fogColor = Hex("#1C2430");
            fogDensity = 0.008f;
            ambientColor = Hex("#181F28"); // Balanced ambient fill
        }
        else if (level == 2)
        {
            fogColor = Hex("#1C2430");
            fogDensity = 0.008f;
            ambientColor = Hex("#1B1F1C");
        }
        else if (level == 3)
        {
            fogColor = Hex("#161828");
            fogDensity = 0.012f;
            ambientColor = Hex("#181422"); // Deep indigo-violet ambient
        }

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColor;
        RenderSettings.ambientIntensity = 0.20f;
        RenderSettings.reflectionIntensity = 0.15f;

        // Configure Directional Light
        Light sun = null;
        foreach (Light l in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (l.type == LightType.Directional)
            {
                sun = l;
                break;
            }
        }

        if (sun == null)
        {
            GameObject sunObj = new GameObject("Directional Light");
            sun = sunObj.AddComponent<Light>();
            sun.type = LightType.Directional;
        }

        sun.color = Hex("#F2F2FF");
        sun.intensity = 0.85f;
        sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        sun.shadows = LightShadows.Hard;
        sun.shadowStrength = 0.85f;
        EditorUtility.SetDirty(sun);

        // Update LevelLightingSettings component if present
        var lls = Object.FindAnyObjectByType<LevelLightingSettings>();
        if (lls != null)
        {
            lls.fogColor = fogColor;
            lls.fogDensity = fogDensity;
            lls.ambientColor = ambientColor;
            lls.directionalColor = sun.color;
            lls.directionalIntensity = sun.intensity;
            lls.directionalEuler = sun.transform.eulerAngles;
            lls.disableRuntimeFillLights = true;
            EditorUtility.SetDirty(lls);
        }
    }

    // =========================================================================
    // 3. CONTAINER MANAGEMENT
    // =========================================================================

    private static Transform GetOrCreateLightingContainer()
    {
        GameObject env = GameObject.Find(EnvRootName);
        if (env == null) env = new GameObject(EnvRootName);

        GameObject lgtRoot = GameObject.Find(LevelLightingName);
        if (lgtRoot == null)
        {
            lgtRoot = new GameObject(LevelLightingName);
            lgtRoot.transform.SetParent(env.transform, false);
        }

        // Clean previous hierarchy lights under this container for clean idempotency
        Transform existing = lgtRoot.transform.Find(LightingRootName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        GameObject container = new GameObject(LightingRootName);
        container.transform.SetParent(lgtRoot.transform, false);
        return container.transform;
    }

    private static void CleanLegacyRedundantLights()
    {
        // Disable or remove unparented point lights generated by legacy builders outside of our container
        GameObject env = GameObject.Find(EnvRootName);
        if (env == null) return;

        var allLights = env.GetComponentsInChildren<Light>(true);
        foreach (var l in allLights)
        {
            if (l.type == LightType.Directional) continue;

            // Keep lights inside our Hierarchy_Lights container
            Transform p = l.transform.parent;
            bool insideOurHierarchy = false;
            while (p != null)
            {
                if (p.name == LightingRootName) { insideOurHierarchy = true; break; }
                p = p.parent;
            }

            if (!insideOurHierarchy)
            {
                // Deactivate legacy duplicate point lights
                l.enabled = false;
            }
        }
    }

    private static void ValidateAndEnforceHardShadows()
    {
        var lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var l in lights)
        {
            if (l.shadows == LightShadows.Soft)
            {
                l.shadows = LightShadows.Hard;
                EditorUtility.SetDirty(l);
            }
        }
    }

    // =========================================================================
    // 4. HIERARCHY BUILDERS FOR N01, N02, N03
    // =========================================================================

    private static void BuildLevel01Hierarchy(Transform root)
    {
        // Categories
        Transform dom = CreateSubGroup(root, "01_Dominant_Fixtures");
        Transform foc = CreateSubGroup(root, "02_Focal_Objectives");
        Transform fill = CreateSubGroup(root, "03_Ambient_Bounce_Fill");
        Transform trans = CreateSubGroup(root, "04_Atmospheric_Transitions");

        // --- ENTRANCE (z = -4) ---
        // Dominant reception fixture
        AddPointLight(dom, "Lgt_Entrance_ReceptionCeiling", new Vector3(0f, 2.7f, -4f), Hex("#FFE4B5"), 4.5f, 9f, LightShadows.Hard);
        // Moon bounce fill
        AddPointLight(fill, "Fill_Entrance_MoonBounce", new Vector3(0f, 0.4f, -6f), Hex("#2C3E50"), 1.8f, 8f, LightShadows.None);

        // --- CORRIDOR A (z = 6, 20m) ---
        // Light pools creating longitudinal rhythm
        AddPointLight(dom, "Lgt_CorridorA_Pool_1", new Vector3(0f, 2.8f, 0f), Hex("#DCE6D2"), 3.6f, 7.5f, LightShadows.Hard);
        var flickLight = AddPointLight(dom, "Lgt_CorridorA_Pool_2_Flicker", new Vector3(0f, 2.8f, 7f), Hex("#DCE6D2"), 3.4f, 7.5f, LightShadows.Hard);
        var flick = flickLight.gameObject.AddComponent<LightFlicker>();
        flick.baseIntensity = 3.4f;
        flick.minIntensity = 0.6f;
        flick.maxIntensity = 1.15f;
        flick.flickerSpeed = 0.09f;

        // --- FOCAL: PRESSURE PLATE (z = 14) ---
        AddPointLight(foc, "Key_Plate_TeachingSpot", new Vector3(0f, 2.9f, 14f), Hex("#FFBF00"), 5.5f, 6.5f, LightShadows.Hard);
        AddPointLight(foc, "Fill_Plate_GroundHalo", new Vector3(0f, 0.3f, 14f), Hex("#FFBF00"), 2.2f, 3.5f, LightShadows.None);

        // --- CORRIDOR B (z = 24, 18m) ---
        AddPointLight(dom, "Lgt_CorridorB_Pool_3", new Vector3(0f, 2.8f, 21f), Hex("#DCE6D2"), 3.6f, 7.5f, LightShadows.Hard);

        // --- TRANSITION: PUERTA AULA (z = 30) ---
        AddPointLight(trans, "Key_Door_InstitutionalAccent", new Vector3(0f, 2.8f, 29.5f), Hex("#B23A3A"), 3.2f, 4.5f, LightShadows.Hard);
        AddPointLight(trans, "Fill_Door_BacklightSpill", new Vector3(0f, 0.4f, 30.5f), Hex("#E8EEF5"), 2.2f, 5.0f, LightShadows.None);

        // --- AULA AUSENTE (z = 38) ---
        AddPointLight(dom, "Key_Classroom_MoonShaft", new Vector3(-2.5f, 2.6f, 38f), Hex("#B0C4DE"), 4.2f, 10f, LightShadows.Hard);
        AddPointLight(fill, "Fill_Classroom_WoodBounce", new Vector3(2.5f, 0.6f, 38f), Hex("#5A4A3A"), 2.0f, 7f, LightShadows.None);

        // --- EXIT BEACON & GOAL (z = 44) ---
        AddPointLight(foc, "Key_Exit_BeaconMaster", new Vector3(0f, 2.5f, 44f), Hex("#FFD27F"), 6.0f, 10f, LightShadows.Hard);
        AddPointLight(foc, "Fill_Exit_GroundRadiance", new Vector3(0f, 0.4f, 44f), Hex("#FFBF00"), 3.0f, 5f, LightShadows.None);
    }

    private static void BuildLevel02Hierarchy(Transform root)
    {
        Transform dom = CreateSubGroup(root, "01_Dominant_Fixtures");
        Transform foc = CreateSubGroup(root, "02_Focal_Objectives");
        Transform fill = CreateSubGroup(root, "03_Ambient_Bounce_Fill");
        Transform trans = CreateSubGroup(root, "04_Atmospheric_Transitions");

        // --- ENTRANCE (z = 0) ---
        AddPointLight(dom, "Lgt_Entrance_ClinicalCeiling", new Vector3(0f, 2.7f, 0f), Hex("#E0E8F5"), 3.8f, 7.5f, LightShadows.Hard);

        // --- PLACA EXPLORACION (z = 7) ---
        AddPointLight(foc, "Key_Plate_ExplorationSpot", new Vector3(0f, 2.8f, 7f), Hex("#FFBF00"), 5.0f, 6.0f, LightShadows.Hard);

        // --- CORREDOR CENTRAL (z = 14, 4m width) ---
        AddPointLight(dom, "Lgt_Corridor_Pool_1", new Vector3(0f, 2.8f, 10.5f), Hex("#C8D6C0"), 3.2f, 6.0f, LightShadows.Hard);
        // Player corridor plate spot
        AddPointLight(foc, "Key_Plate_PlayerCorridor", new Vector3(0f, 2.8f, 14f), Hex("#5DADE2"), 5.2f, 6.0f, LightShadows.Hard);
        AddPointLight(dom, "Lgt_Corridor_Pool_2", new Vector3(0f, 2.8f, 18f), Hex("#C8D6C0"), 3.2f, 6.0f, LightShadows.Hard);

        // --- AULA IZQUIERDA (x = -6, z = 14) - NOSTALGIC WARMTH ---
        AddPointLight(dom, "Key_LeftClassroom_WarmNostalgia", new Vector3(-6f, 2.7f, 14f), Hex("#E6B860"), 4.8f, 9.0f, LightShadows.Hard);
        AddPointLight(foc, "Key_Plate_EchoClassroom", new Vector3(-6f, 2.8f, 14f), Hex("#FFBF00"), 4.5f, 5.0f, LightShadows.Hard);
        AddPointLight(fill, "Fill_LeftClassroom_WoodBounce", new Vector3(-6f, 0.4f, 14f), Hex("#8A6030"), 2.2f, 6.0f, LightShadows.None);

        // --- VENTANAL DE AULAS CROSS-SPILL ---
        AddPointLight(trans, "Spill_Window_WarmToCorridor", new Vector3(-2.2f, 1.6f, 14f), Hex("#E6B860"), 2.6f, 5.0f, LightShadows.None);
        AddPointLight(trans, "Spill_Window_ColdRimRight", new Vector3(2.2f, 1.6f, 14f), Hex("#90AFC5"), 2.0f, 5.0f, LightShadows.None);

        // --- AULA DERECHA (x = 6, z = 14) - CLINICAL COLD ---
        AddPointLight(dom, "Key_RightClassroom_ClinicalCold", new Vector3(6f, 2.7f, 14f), Hex("#90AFC5"), 4.2f, 9.0f, LightShadows.Hard);
        AddPointLight(fill, "Fill_RightClassroom_TilesBounce", new Vector3(6f, 0.4f, 14f), Hex("#2B4A4A"), 1.8f, 6.0f, LightShadows.None);

        // --- PUERTA AULA (z = 27) ---
        AddPointLight(trans, "Key_Door_DintelAccent", new Vector3(0f, 2.8f, 27f), Hex("#B23A3A"), 3.2f, 4.5f, LightShadows.Hard);

        // --- PLATAFORMA RAPIDA (z = 34) ---
        AddPointLight(dom, "Key_MovingPlatform_SafetySpot", new Vector3(0f, 2.8f, 34f), Hex("#F5F5DC"), 4.0f, 8.0f, LightShadows.Hard);
        AddPointLight(fill, "Fill_MovingPlatform_GuideAmber", new Vector3(0f, 0.3f, 34f), Hex("#FFBF00"), 2.0f, 4.0f, LightShadows.None);

        // --- HALL SALIDA (z = 42, 5m height) ---
        AddPointLight(dom, "Key_HallExit_ClerestoryDownlight", new Vector3(0f, 4.5f, 42f), Hex("#6C7A89"), 5.2f, 14.0f, LightShadows.Hard);
        AddPointLight(fill, "Fill_HallExit_FloorIndigoBounce", new Vector3(0f, 0.5f, 42f), Hex("#2C3A4A"), 2.4f, 8.0f, LightShadows.None);
        AddPointLight(foc, "Key_Exit_BeaconMaster_N02", new Vector3(0f, 2.5f, 46f), Hex("#FFC04D"), 6.0f, 10.0f, LightShadows.Hard);
    }

    private static void BuildLevel03Hierarchy(Transform root)
    {
        Transform dom = CreateSubGroup(root, "01_Dominant_Fixtures");
        Transform foc = CreateSubGroup(root, "02_Focal_Objectives");
        Transform fill = CreateSubGroup(root, "03_Ambient_Bounce_Fill");
        Transform trans = CreateSubGroup(root, "04_Atmospheric_Transitions");

        // --- ENTRANCE (z = -2) ---
        AddPointLight(dom, "Lgt_Entrance_LavenderDream", new Vector3(0f, 2.6f, -2f), Hex("#A3A0FB"), 4.0f, 8.0f, LightShadows.Hard);

        // --- BIFURCATION VERTEX (z = 6) ---
        // Left Branch Chromatic Cue (Warm Rose/Amber)
        AddPointLight(trans, "Cue_Fork_LeftRoseLyra", new Vector3(-1.8f, 2.6f, 6f), Hex("#D98880"), 4.4f, 6.0f, LightShadows.Hard);
        // Right Branch Chromatic Cue (Cold Spectral Cyan)
        AddPointLight(trans, "Cue_Fork_RightCyanEcho", new Vector3(1.8f, 2.6f, 6f), Hex("#5DADE2"), 4.4f, 6.0f, LightShadows.Hard);
        // Shadow Eco Plate
        AddPointLight(foc, "Key_Plate_ShadowEcoVertex", new Vector3(0f, 2.8f, 6f), Hex("#FFBF00"), 4.5f, 5.0f, LightShadows.Hard);

        // --- RAMA IZQUIERDA & AULA DE LYRA (x = -5, z = 16) ---
        AddPointLight(dom, "Key_LyraClassroom_WarmDustyRose", new Vector3(-5f, 2.7f, 16f), Hex("#E8A87C"), 5.0f, 8.5f, LightShadows.Hard);
        AddPointLight(foc, "Key_Plate_PlayerLeftBranch", new Vector3(-5f, 2.8f, 16f), Hex("#FFBF00"), 4.6f, 5.0f, LightShadows.Hard);
        AddPointLight(fill, "Fill_LyraMemory_Bounce", new Vector3(-5f, 0.4f, 16f), Hex("#4A3438"), 2.2f, 5.0f, LightShadows.None);
        AddPointLight(trans, "Key_Door_LeftBranchDintel", new Vector3(-5f, 2.7f, 22f), Hex("#B23A3A"), 3.0f, 4.0f, LightShadows.Hard);

        // --- RAMA DERECHA & AULA ECO (x = 5, z = 16) ---
        AddPointLight(dom, "Key_EchoClassroom_SpectralCyan", new Vector3(5f, 2.7f, 16f), Hex("#4FC3E8"), 4.6f, 8.5f, LightShadows.Hard);
        AddPointLight(fill, "Fill_EchoResonance_FloorCyan", new Vector3(5f, 0.4f, 16f), Hex("#0080FF"), 2.2f, 6.0f, LightShadows.None);
        AddPointLight(trans, "Key_Door_RightBranchDintel", new Vector3(5f, 2.7f, 22f), Hex("#4FC3E8"), 3.0f, 4.0f, LightShadows.Hard);
        AddPointLight(foc, "Key_Plate_EchoRightBranch", new Vector3(4f, 2.8f, 30f), Hex("#FFBF00"), 4.5f, 5.0f, LightShadows.Hard);

        // --- HALL DE LA ESTATUA (z = 30, 10x5x10m Monumental Climax) ---
        // Master Theatrical Key directly over the founder statue
        AddPointLight(dom, "Key_Statue_TheatricalSpotlight", new Vector3(0f, 4.8f, 30f), Hex("#FFF8DC"), 7.2f, 14.0f, LightShadows.Hard);
        // Statue Rim / Backlight for crisp silhouette separation
        AddPointLight(dom, "Rim_Statue_SilhouetteBacklight", new Vector3(0f, 2.5f, 33f), Hex("#A3A0FB"), 3.6f, 7.0f, LightShadows.Hard);
        // Indigo perimeter bounce fill to eliminate pitch black in corners
        AddPointLight(fill, "Fill_StatueHall_IndigoBounce", new Vector3(0f, 0.6f, 30f), Hex("#1A1C2E"), 2.5f, 12.0f, LightShadows.None);

        // --- EXIT BEACON & LEVEL 4 TRANSITION (z = 36) ---
        AddPointLight(foc, "Key_Exit_BeaconMaster_N03", new Vector3(0f, 2.5f, 36f), Hex("#FFD700"), 6.5f, 10.0f, LightShadows.Hard);
        AddPointLight(foc, "Fill_Exit_GroundGold", new Vector3(0f, 0.4f, 36f), Hex("#FFBF00"), 3.2f, 5.0f, LightShadows.None);
    }

    // =========================================================================
    // HELPER METHODS
    // =========================================================================

    private static Transform CreateSubGroup(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static Light AddPointLight(Transform parent, string name, Vector3 position, Color color, float intensity, float range, LightShadows shadows)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = position;

        Light l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = color;
        l.intensity = intensity;
        l.range = range;
        l.shadows = shadows;
        l.lightmapBakeType = LightmapBakeType.Mixed;
        return l;
    }

    private static Color Hex(string hex)
    {
        if (ColorUtility.TryParseHtmlString("#" + hex.TrimStart('#'), out Color c))
            return c;
        return Color.white;
    }
}
