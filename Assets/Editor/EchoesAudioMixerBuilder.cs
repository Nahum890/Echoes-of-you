#if UNITY_EDITOR
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Editor utility that programmatically creates and maintains the Echoes AudioMixer asset.
/// Generates a mixer with Master, Music, SFX, and Echo groups, each with exposed volume parameters.
/// Bypasses protection level issues (internal AudioMixerController APIs) via Reflection.
/// Runs automatically on editor load/compile via [InitializeOnLoad].
/// </summary>
[InitializeOnLoad]
public static class EchoesAudioMixerBuilder
{
    public const string MixerAssetPath = "Assets/Resources/EchoesAudioMixer.mixer";
    const string MixerFolderPath = "Assets/Resources";

    // Exposed parameter names — used by EchoesAudioManager at runtime
    public const string MasterVolumeParam = "MasterVolume";
    public const string MusicVolumeParam = "MusicVolume";
    public const string SFXVolumeParam = "SFXVolume";
    public const string EchoVolumeParam = "EchoVolume";

    static EchoesAudioMixerBuilder()
    {
        // Automatically ensure the mixer is generated when the editor compiles or starts up
        EditorApplication.delayCall += () =>
        {
            EnsureAudioMixer();
        };
    }

    [MenuItem("Echoes of You/Production/Ensure Audio Mixer", false, 160)]
    public static AudioMixer EnsureAudioMixer()
    {
        if (!AssetDatabase.IsValidFolder(MixerFolderPath))
            AssetDatabase.CreateFolder("Assets", "Resources");

        AudioMixer existing = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerAssetPath);
        if (existing != null)
        {
            // Failsafe: if the mixer exists but has empty/corrupted snapshots (which caused console errors), regenerate it
            if (existing.FindMatchingGroups("Master").Length == 0)
            {
                Debug.LogWarning("[Echoes Audio] Existing mixer is invalid or corrupted. Deleting to regenerate.");
                AssetDatabase.DeleteAsset(MixerAssetPath);
            }
            else
            {
                return existing;
            }
        }

        if (File.Exists(MixerAssetPath))
        {
            Debug.LogWarning("[Echoes Audio] Existing mixer asset file was found but could not be loaded. Deleting it to regenerate.");
            AssetDatabase.DeleteAsset(MixerAssetPath);
        }

        // Get Editor types via Reflection since they are internal to UnityEditor.Audio
        Assembly editorAssembly = typeof(UnityEditor.Editor).Assembly;
        System.Type tController = editorAssembly.GetType("UnityEditor.Audio.AudioMixerController");
        System.Type tGroupController = editorAssembly.GetType("UnityEditor.Audio.AudioMixerGroupController");
        System.Type tExposedParam = editorAssembly.GetType("UnityEditor.Audio.ExposedAudioParameter");

        if (tController == null || tGroupController == null || tExposedParam == null)
        {
            Debug.LogError("[Echoes Audio] Failed to retrieve internal Unity AudioMixer editor types via Reflection.");
            return null;
        }

        // Create the mixer controller using Unity's native editor helper method to ensure it's natively initialized!
        MethodInfo createMethod = tController.GetMethod("CreateMixerControllerAtPath", 
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        if (createMethod == null)
        {
            Debug.LogError("[Echoes Audio] Failed to find internal CreateMixerControllerAtPath method on AudioMixerController.");
            return null;
        }

        // Invoke: public static AudioMixerController CreateMixerControllerAtPath(string path)
        UnityEngine.Object controller = (UnityEngine.Object)createMethod.Invoke(null, new object[] { MixerAssetPath });

        if (controller == null)
        {
            Debug.LogError("[Echoes Audio] Failed to create natively initialized AudioMixerController using CreateMixerControllerAtPath.");
            return null;
        }

        // Get natively initialized masterGroup
        PropertyInfo masterGroupProp = tController.GetProperty("masterGroup", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        UnityEngine.Object masterGroup = (UnityEngine.Object)masterGroupProp.GetValue(controller, null);
        if (masterGroup == null)
        {
            Debug.LogError("[Echoes Audio] Native masterGroup was null after calling CreateMixerControllerAtPath.");
            return null;
        }
        masterGroup.name = "Master";

        // Create child groups under Master
        UnityEngine.Object musicGroup = CreateChildGroup(tController, tGroupController, controller, masterGroup, "Music");
        UnityEngine.Object sfxGroup = CreateChildGroup(tController, tGroupController, controller, masterGroup, "SFX");
        UnityEngine.Object echoGroup = CreateChildGroup(tController, tGroupController, controller, masterGroup, "Echo");

        // Expose volume parameters on each group
        ExposeVolumeParameter(tController, tGroupController, tExposedParam, controller, masterGroup, MasterVolumeParam);
        ExposeVolumeParameter(tController, tGroupController, tExposedParam, controller, musicGroup, MusicVolumeParam);
        ExposeVolumeParameter(tController, tGroupController, tExposedParam, controller, sfxGroup, SFXVolumeParam);
        ExposeVolumeParameter(tController, tGroupController, tExposedParam, controller, echoGroup, EchoVolumeParam);

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[Echoes Audio] Created natively initialized AudioMixer at " + MixerAssetPath +
                  " with groups: Master, Music, SFX, Echo. " +
                  "Exposed parameters: " + MasterVolumeParam + ", " + MusicVolumeParam +
                  ", " + SFXVolumeParam + ", " + EchoVolumeParam);

        return (AudioMixer)controller;
    }

    static UnityEngine.Object CreateChildGroup(System.Type tController, System.Type tGroupController, UnityEngine.Object mixer, UnityEngine.Object parent, string groupName)
    {
        UnityEngine.Object group = (UnityEngine.Object)System.Activator.CreateInstance(tGroupController, new object[] { mixer });
        group.name = groupName;

        AssetDatabase.AddObjectToAsset(group, mixer);

        MethodInfo addChildMethod = tController.GetMethod("AddChildToParent", 
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, 
            null, 
            new System.Type[] { tGroupController, tGroupController }, 
            null);
        addChildMethod.Invoke(mixer, new object[] { group, parent });

        return group;
    }

    static void ExposeVolumeParameter(System.Type tController, System.Type tGroupController, System.Type tExposedParam, UnityEngine.Object mixer, UnityEngine.Object group, string exposedName)
    {
        MethodInfo getGuidMethod = tGroupController.GetMethod("GetGUIDForVolume", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        GUID volumeGUID = (GUID)getGuidMethod.Invoke(group, null);

        object param = System.Activator.CreateInstance(tExposedParam);
        tExposedParam.GetField("guid", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(param, volumeGUID);
        tExposedParam.GetField("name", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(param, exposedName);

        FieldInfo fieldInfo = tController.GetField("exposedParameters", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        PropertyInfo propInfo = tController.GetProperty("exposedParameters", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        System.Array currentArray = null;
        if (fieldInfo != null)
            currentArray = (System.Array)fieldInfo.GetValue(mixer);
        else if (propInfo != null)
            currentArray = (System.Array)propInfo.GetValue(mixer, null);

        int currentLength = currentArray != null ? currentArray.Length : 0;
        System.Array newArray = System.Array.CreateInstance(tExposedParam, currentLength + 1);
        if (currentArray != null)
            System.Array.Copy(currentArray, newArray, currentLength);
        newArray.SetValue(param, currentLength);

        if (fieldInfo != null)
            fieldInfo.SetValue(mixer, newArray);
        else if (propInfo != null)
            propInfo.SetValue(mixer, newArray, null);
    }

    public static AudioMixer LoadMixer()
    {
        return AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerAssetPath);
    }
}
#endif
