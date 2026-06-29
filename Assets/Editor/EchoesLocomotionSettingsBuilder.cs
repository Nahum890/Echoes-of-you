#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class EchoesLocomotionSettingsBuilder
{
    const string CharacterFbxPath = "Assets/3D Models/lowpoly-character-freerigged-/source/LowPolyCharacterModel/FBX/LowPolyCharacter.fbx";
    const string ControllerPath = "Assets/Prefabs/PlayerAnimController.controller";
    const string SettingsPath = "Assets/Resources/EchoesLocomotionSettings.asset";

    [MenuItem("Echoes of You/Production/Ensure Locomotion Settings (Animations)", false, 150)]
    public static void EnsureLocomotionSettings()
    {
        SetupPlayerAnimator.Setup();

        Avatar avatar = AssetDatabase.LoadAssetAtPath<Avatar>("Assets/Prefabs/PlayerHumanoidAvatar.asset");
        if (avatar == null || !avatar.isValid)
            avatar = LoadAvatarFromModel(CharacterFbxPath);
        RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        EchoesLocomotionSettings settings = AssetDatabase.LoadAssetAtPath<EchoesLocomotionSettings>(SettingsPath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<EchoesLocomotionSettings>();
            AssetDatabase.CreateAsset(settings, SettingsPath);
        }

        settings.animatorController = controller;
        settings.humanoidAvatar = avatar;
        settings.characterModelPrefab = EnsureCharacterResourcePrefab();
        EditorUtility.SetDirty(settings);

        PlayerAnimationRepair.RepairProjectAnimationSetup();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Echoes] Locomotion settings ready. Assign Resources/EchoesLocomotionSettings.asset at runtime.");
    }

    static Avatar LoadAvatarFromModel(string modelPath)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(modelPath);
        for (int i = 0; i < assets.Length; i++)
        {
            if (assets[i] is Avatar avatar && avatar.isValid)
                return avatar;
        }

        Debug.LogWarning("[Echoes] No Avatar sub-asset in " + modelPath);
        return null;
    }

    const string CharacterResourcePrefabPath = "Assets/Resources/EchoesCharacterVisual.prefab";

    static GameObject EnsureCharacterResourcePrefab()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterFbxPath);
        if (source == null)
        {
            Debug.LogWarning("[Echoes] Character FBX missing at " + CharacterFbxPath);
            return null;
        }

        GameObject instance = PrefabUtility.InstantiatePrefab(source) as GameObject;
        if (instance == null)
            return null;

        instance.name = "EchoesCharacterVisual";
        foreach (Collider col in instance.GetComponentsInChildren<Collider>(true))
            Object.DestroyImmediate(col);

        Animator anim = instance.GetComponent<Animator>();
        if (anim == null)
            anim = instance.AddComponent<Animator>();
        anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
        Avatar avatar = LoadAvatarFromModel(CharacterFbxPath);
        if (avatar != null && avatar.isValid)
            anim.avatar = avatar;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, CharacterResourcePrefabPath);
        Object.DestroyImmediate(instance);
        return prefab;
    }
}
#endif
