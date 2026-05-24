#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PlayerAnimationRepair
{
    const string ControllerPath = "Assets/Prefabs/PlayerAnimController.controller";
    const string CharacterAvatarPath = "Assets/3D Models/lowpoly-character-freerigged-/source/LowPolyCharacterModel/FBX/LowPolyCharacter.fbx";

    static readonly string[] RigPaths =
    {
        CharacterAvatarPath,
        "Assets/3D Models/Animaciones/Locomotion/idle.fbx",
        "Assets/3D Models/Animaciones/Locomotion/walking.fbx",
        "Assets/3D Models/Animaciones/Locomotion/running.fbx",
        "Assets/3D Models/Animaciones/Locomotion/jump.fbx",
        "Assets/3D Models/Animaciones/Locomotion/left turn 90.fbx",
        "Assets/3D Models/Animaciones/Locomotion/right turn 90.fbx",
        "Assets/3D Models/Animaciones/Locomotion/left strafe walking.fbx",
        "Assets/3D Models/Animaciones/Locomotion/right strafe walking.fbx"
    };

    [MenuItem("Echoes/Repair Player Animation Setup", false, 83)]
    public static void RepairProjectAnimationSetup()
    {
        ConfigureHumanoidImports();
        SetupPlayerAnimator.Setup();
        PatchEchoPrefab();
        PatchBuildScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Echoes] Player animation setup repaired: rigs, controller, scenes and echo prefab.");
    }

    static void ConfigureHumanoidImports()
    {
        for (int i = 0; i < RigPaths.Length; i++)
        {
            string path = RigPaths[i];
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
                continue;

            bool changed = false;
            if (importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                changed = true;
            }

            if (!Mathf.Approximately(importer.globalScale, 1f))
            {
                importer.globalScale = 1f;
                changed = true;
            }

            if (importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
            {
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
                Debug.Log("[Echoes] Reimported humanoid animation/model: " + path);
            }
        }
    }

    static void PatchEchoPrefab()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EchoPrefab.prefab");
        if (prefab == null)
            return;

        GameObject instance = PrefabUtility.LoadPrefabContents("Assets/Prefabs/EchoPrefab.prefab");
        PatchAnimatorInHierarchy(instance);
        PrefabUtility.SaveAsPrefabAsset(instance, "Assets/Prefabs/EchoPrefab.prefab");
        PrefabUtility.UnloadPrefabContents(instance);
    }

    static void PatchBuildScenes()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        string activePath = activeScene.path;
        bool hadActiveScene = !string.IsNullOrEmpty(activePath);

        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        for (int i = 0; i < scenes.Length; i++)
        {
            string scenePath = scenes[i].path;
            if (string.IsNullOrEmpty(scenePath) || !scenePath.StartsWith("Assets/Scenes/"))
                continue;

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool changed = false;
            PlayerController[] players = Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int p = 0; p < players.Length; p++)
                changed |= PatchAnimatorInHierarchy(players[p].gameObject);

            EchoPlayback[] echoes = Object.FindObjectsByType<EchoPlayback>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int e = 0; e < echoes.Length; e++)
                changed |= PatchAnimatorInHierarchy(echoes[e].gameObject);

            if (changed)
                EditorSceneManager.SaveScene(scene);
        }

        if (hadActiveScene)
            EditorSceneManager.OpenScene(activePath, OpenSceneMode.Single);
    }

    static bool PatchAnimatorInHierarchy(GameObject root)
    {
        RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
        Avatar avatar = LoadHumanoidAvatar();
        if (controller == null)
        {
            Debug.LogWarning("[Echoes] Missing PlayerAnimController at " + ControllerPath);
            return false;
        }

        Transform visualRoot = root.transform.Find("PlayerVisual");
        if (visualRoot == null)
            visualRoot = root.transform.Find("Visual");
        Transform searchRoot = visualRoot != null ? visualRoot : root.transform;

        Animator animator = searchRoot.GetComponentInChildren<Animator>(true);
        if (animator == null)
        {
            Transform model = searchRoot.childCount > 0 ? searchRoot.GetChild(0) : searchRoot;
            animator = model.gameObject.AddComponent<Animator>();
        }

        bool changed = false;
        if (animator.runtimeAnimatorController != controller)
        {
            animator.runtimeAnimatorController = controller;
            changed = true;
        }

        if (avatar != null && animator.avatar != avatar)
        {
            animator.avatar = avatar;
            changed = true;
        }

        if (animator.applyRootMotion)
        {
            animator.applyRootMotion = false;
            changed = true;
        }

        if (animator.cullingMode != AnimatorCullingMode.AlwaysAnimate)
        {
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            changed = true;
        }

        if (changed)
            EditorUtility.SetDirty(animator);

        return changed;
    }

    static Avatar LoadHumanoidAvatar()
    {
        Avatar saved = AssetDatabase.LoadAssetAtPath<Avatar>("Assets/Prefabs/PlayerHumanoidAvatar.asset");
        if (saved != null && saved.isValid)
            return saved;

        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(CharacterAvatarPath);
        for (int i = 0; i < assets.Length; i++)
        {
            if (assets[i] is Avatar avatar && avatar.isValid)
                return avatar;
        }

        return null;
    }
}
#endif
