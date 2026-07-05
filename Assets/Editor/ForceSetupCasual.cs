using UnityEngine;
using UnityEditor;

/// <summary>
/// One-click setup: reimports Casual.fbx (Humanoid), fixes scale,
/// patches EchoesLocomotionSettings and EchoesCharacterVisual prefab.
/// Menu: Tools > Echoes > Force Setup Casual
/// </summary>
public static class ForceSetupCasual
{
    const string CasualFbxPath  = "Assets/3D Models/Animated Woman/Casual.fbx";
    const string SettingsPath   = "Assets/Resources/EchoesLocomotionSettings.asset";
    const string PrefabPath     = "Assets/Resources/EchoesCharacterVisual.prefab";
    const string ControllerPath = "Assets/Prefabs/PlayerAnimController.controller";

    // Set scale to 1.75f to match standard Unity CharacterController height.
    const float ModelScale = 1.75f;

    static readonly string[] LoopAnimationPaths = new string[]
    {
        "Assets/3D Models/Animaciones/Locomotion/idle.fbx",
        "Assets/3D Models/Animaciones/Locomotion/walking.fbx",
        "Assets/3D Models/Animaciones/Locomotion/running.fbx",
        "Assets/3D Models/Animaciones/Locomotion/left strafe.fbx",
        "Assets/3D Models/Animaciones/Locomotion/right strafe.fbx",
        "Assets/3D Models/Animaciones/Locomotion/left strafe walking.fbx",
        "Assets/3D Models/Animaciones/Locomotion/right strafe walking.fbx"
    };

    [MenuItem("Tools/Echoes/Force Setup Casual", priority = 1)]
    static void Run()
    {
        EditorUtility.DisplayProgressBar("Setting up Casual...", "Configuring FBX importer...", 0.05f);
        try
        {
            // ── 0. Rebuild and optimize the Animator Controller ──────────────────
            SetupPlayerAnimator.Setup();

            // ── 1. Configure the ModelImporter ──────────────────────────────────
            ModelImporter importer = AssetImporter.GetAtPath(CasualFbxPath) as ModelImporter;
            if (importer == null)
            {
                Debug.LogError("[ForceSetupCasual] Casual.fbx not found at: " + CasualFbxPath);
                EditorUtility.DisplayDialog("Error", "Casual.fbx not found!\nPath: " + CasualFbxPath, "OK");
                return;
            }

            bool dirty = false;

            // Humanoid rig
            if (importer.animationType != ModelImporterAnimationType.Human)
            { importer.animationType = ModelImporterAnimationType.Human; dirty = true; }

            // Create avatar from this model (not Copy)
            if (importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
            { importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel; dirty = true; }

            // Fix scale: Mixamo exports in cm; we need 0.01 to get metres
            if (!Mathf.Approximately(importer.globalScale, ModelScale))
            { importer.globalScale = ModelScale; dirty = true; }

            // Disable "bake axis conversion" to avoid double-rotation
            if (importer.bakeAxisConversion)
            { importer.bakeAxisConversion = false; dirty = true; }

            // Don't import animations from the T-pose file itself
            if (importer.importAnimation)
            { importer.importAnimation = false; dirty = true; }

            // ALWAYS clear any old cached humanDescription mapping in the .meta file
            // and force a clean SaveAndReimport so Unity's auto-mapper runs on the new standard bones.
            importer.humanDescription = new HumanDescription();
            dirty = true;

            if (dirty)
            {
                EditorUtility.DisplayProgressBar("Setting up Casual...", "Reimporting FBX...", 0.20f);
                importer.SaveAndReimport();
            }
            Debug.Log("[ForceSetupCasual] Casual.fbx reimported and mapping cleared.");

            // ── 2. Validate avatar ───────────────────────────────────────────────
            EditorUtility.DisplayProgressBar("Setting up Casual...", "Validating avatar...", 0.40f);

            // Give Unity a moment to process the import
            AssetDatabase.Refresh();

            Avatar casualAvatar = null;
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(CasualFbxPath);
            foreach (Object a in allAssets)
            {
                if (a is Avatar av && av.isValid && av.isHuman)
                { casualAvatar = av; break; }
            }

            if (casualAvatar == null)
            {
                Debug.LogError("[ForceSetupCasual] No valid Humanoid avatar found in Casual.fbx after import. " +
                               "Open Casual.fbx Inspector → Rig tab → ensure bones map correctly, then retry.");
                EditorUtility.DisplayDialog("Avatar Error",
                    "Casual.fbx does not have a valid Humanoid avatar.\n\n" +
                    "In the Project window: select Casual.fbx → Inspector → Rig tab → " +
                    "Animation Type = Humanoid → Avatar Definition = Create From This Model → Apply.\n\n" +
                    "Then run this tool again.", "OK");
                return;
            }
            Debug.Log("[ForceSetupCasual] Avatar OK: " + casualAvatar.name);

            // ── 3. Load animator controller ─────────────────────────────────────
            RuntimeAnimatorController controller =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            if (controller == null)
                Debug.LogWarning("[ForceSetupCasual] PlayerAnimController not found at: " + ControllerPath);

            // ── 4. Patch EchoesLocomotionSettings ───────────────────────────────
            EditorUtility.DisplayProgressBar("Setting up Casual...", "Patching settings...", 0.60f);
            EchoesLocomotionSettings settings =
                AssetDatabase.LoadAssetAtPath<EchoesLocomotionSettings>(SettingsPath);

            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<EchoesLocomotionSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }

            settings.humanoidAvatar = casualAvatar;
            if (controller != null) settings.animatorController = controller;

            // ── 5. Rebuild EchoesCharacterVisual prefab ──────────────────────────
            EditorUtility.DisplayProgressBar("Setting up Casual...", "Rebuilding prefab...", 0.75f);
            GameObject prefab = RebuildVisualPrefab(casualAvatar, controller);
            if (prefab != null)
                settings.characterModelPrefab = prefab;

            // ── 5.5 Configure animation looping ──────────────────────────
            EditorUtility.DisplayProgressBar("Setting up Casual...", "Configuring animation looping...", 0.85f);
            ConfigureAnimationLooping();

            EditorUtility.SetDirty(settings);

            // ── 6. Save everything ───────────────────────────────────────────────
            EditorUtility.DisplayProgressBar("Setting up Casual...", "Saving...", 0.92f);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[ForceSetupCasual] ✅ Complete! Press Play to test.");
            EditorUtility.DisplayDialog("Casual Setup Complete ✅",
                "Character setup done!\n\n" +
                "• Avatar: " + casualAvatar.name + "\n" +
                "• Scale: " + ModelScale + " (Mixamo cm→m)\n" +
                "• Controller: " + (controller != null ? controller.name : "MISSING") + "\n\n" +
                "Press Play to see Casual with animations.", "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    static GameObject RebuildVisualPrefab(Avatar avatar, RuntimeAnimatorController controller)
    {
        GameObject casualFbx = AssetDatabase.LoadAssetAtPath<GameObject>(CasualFbxPath);
        if (casualFbx == null)
        {
            Debug.LogError("[ForceSetupCasual] LoadAssetAtPath failed for Casual.fbx.");
            return null;
        }

        // Instantiate a clean copy
        GameObject instance = Object.Instantiate(casualFbx);
        instance.name = "EchoesCharacterVisual";

        // Scale is already handled in the importer (globalScale = 0.01),
        // so reset local scale to 1 to avoid double-scaling.
        instance.transform.localScale = Vector3.one;

        // Wire up Animator
        Animator anim = instance.GetComponent<Animator>();
        if (anim == null) anim = instance.GetComponentInChildren<Animator>(true);
        if (anim == null) anim = instance.AddComponent<Animator>();

        anim.avatar = avatar;
        if (controller != null) anim.runtimeAnimatorController = controller;
        anim.applyRootMotion = false;
        anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        anim.updateMode = AnimatorUpdateMode.Normal;

        // Remove physics colliders (visual-only prefab)
        foreach (Collider col in instance.GetComponentsInChildren<Collider>(true))
            Object.DestroyImmediate(col);

        // Save or overwrite
        bool ok;
        GameObject saved = PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath, out ok);
        Object.DestroyImmediate(instance);

        if (ok)
            Debug.Log("[ForceSetupCasual] EchoesCharacterVisual.prefab saved.");
        else
            Debug.LogError("[ForceSetupCasual] Failed to save EchoesCharacterVisual.prefab.");

        return ok ? saved : null;
    }

    static void ConfigureAnimationLooping()
    {
        foreach (string path in LoopAnimationPaths)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                Debug.LogWarning("[ForceSetupCasual] Animation file not found for looping: " + path);
                continue;
            }

            // Mixamo files usually have only 1 clip, but we loop through all defaults
            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            if (clips == null || clips.Length == 0)
                clips = importer.clipAnimations;

            if (clips != null && clips.Length > 0)
            {
                bool changed = false;
                for (int i = 0; i < clips.Length; i++)
                {
                    if (!clips[i].loopTime || !clips[i].loopPose)
                    {
                        clips[i].loopTime = true;
                        clips[i].loopPose = true;
                        changed = true;
                    }
                }

                if (changed)
                {
                    importer.clipAnimations = clips;
                    importer.SaveAndReimport();
                    Debug.Log("[ForceSetupCasual] Configured loop time for: " + path);
                }
            }
        }
    }
}
