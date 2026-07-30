using System;
using System.Reflection;
using UnityEngine;

public class CinemachineRuntimeSetup : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoSetup()
    {
        Camera cameraRef = Camera.main;
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (cameraRef == null || player == null)
            return;

        // SimpleFollowCamera is the canonical camera system (Cinemachine fully replaced).
        // If present and active, do NOT inject Cinemachine — it would freeze the camera.
        SimpleFollowCamera sfc = cameraRef.GetComponent<SimpleFollowCamera>();
        if (sfc != null && sfc.enabled)
            return;

        // ThirdPersonCamera (legacy fallback) — same guard.
        ThirdPersonCamera tpc = cameraRef.GetComponent<ThirdPersonCamera>();
        if (tpc != null && tpc.enabled)
            return;

        Type cameraType = ResolveType("Unity.Cinemachine.CinemachineCamera");
        Type brainType = ResolveType("Unity.Cinemachine.CinemachineBrain");
        if (cameraType == null || brainType == null)
            return;

        if (cameraRef.GetComponent(brainType) == null)
            cameraRef.gameObject.AddComponent(brainType);

        if (FindExisting(cameraType) != null)
            return; // Ya existe una cámara de Cinemachine en escena

        Transform followTarget = player.transform.Find("CameraFocus") ?? player.transform;
        GameObject rig = new GameObject("CinematicPlayerVCam");
        Component vcam = rig.AddComponent(cameraType);
        SetProperty(vcam, "Follow", followTarget);
        SetProperty(vcam, "LookAt", followTarget);

        // Set Priority using PrioritySettings
        Type prioritySettingsType = ResolveType("Unity.Cinemachine.PrioritySettings");
        if (prioritySettingsType != null)
        {
            object prioritySettings = Activator.CreateInstance(prioritySettingsType);
            FieldInfo valueField = prioritySettingsType.GetField("Value", BindingFlags.Public | BindingFlags.Instance);
            if (valueField != null)
                valueField.SetValue(prioritySettings, 20);
            SetField(vcam, "Priority", prioritySettings);
        }

        // Set Lens
        FieldInfo lensField = cameraType.GetField("Lens", BindingFlags.Instance | BindingFlags.Public);
        if (lensField != null)
        {
            object lens = lensField.GetValue(vcam);
            FieldInfo fovField = lensField.FieldType.GetField("FieldOfView", BindingFlags.Instance | BindingFlags.Public);
            if (fovField != null)
                fovField.SetValue(lens, 52f);
            lensField.SetValue(vcam, lens);
        }

        // Add CinemachineFollow (transposer)
        Type followType = ResolveType("Unity.Cinemachine.CinemachineFollow");
        if (followType != null)
        {
            Component follow = rig.AddComponent(followType);
            SetField(follow, "FollowOffset", new Vector3(-5.5f, 3.2f, -9.5f));

            Type trackerSettingsType = ResolveType("Unity.Cinemachine.TargetTracking.TrackerSettings");
            Type bindingModeType = ResolveType("Unity.Cinemachine.TargetTracking.BindingMode");
            if (trackerSettingsType != null && bindingModeType != null)
            {
                object trackerSettings = Activator.CreateInstance(trackerSettingsType);
                FieldInfo bindingModeField = trackerSettingsType.GetField("BindingMode", BindingFlags.Public | BindingFlags.Instance);
                FieldInfo posDampingField = trackerSettingsType.GetField("PositionDamping", BindingFlags.Public | BindingFlags.Instance);
                
                if (bindingModeField != null)
                    bindingModeField.SetValue(trackerSettings, Enum.Parse(bindingModeType, "WorldSpace"));
                if (posDampingField != null)
                    posDampingField.SetValue(trackerSettings, new Vector3(0.55f, 0.65f, 0.5f));

                SetField(follow, "TrackerSettings", trackerSettings);
            }
        }

        // Add CinemachineRotationComposer (composer)
        Type rotationComposerType = ResolveType("Unity.Cinemachine.CinemachineRotationComposer");
        if (rotationComposerType != null)
        {
            Component composer = rig.AddComponent(rotationComposerType);
            SetField(composer, "TargetOffset", new Vector3(0f, 0.35f, 0f));
            SetField(composer, "Damping", new Vector2(0.45f, 0.55f));

            Type screenComposerSettingsType = ResolveType("Unity.Cinemachine.ScreenComposerSettings");
            if (screenComposerSettingsType != null)
            {
                object screenComposerSettings = Activator.CreateInstance(screenComposerSettingsType);
                FieldInfo screenPosField = screenComposerSettingsType.GetField("ScreenPosition", BindingFlags.Public | BindingFlags.Instance);
                if (screenPosField != null)
                    screenPosField.SetValue(screenComposerSettings, new Vector2(0.48f, 0.42f));
                
                SetField(composer, "Composition", screenComposerSettings);
            }
        }
    }

    static Type ResolveType(string typeName)
    {
        Type type = Type.GetType(typeName + ", Unity.Cinemachine");
        if (type != null)
            return type;

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            type = assemblies[i].GetType(typeName);
            if (type != null)
                return type;
        }

        return null;
    }

    static UnityEngine.Object FindExisting(Type type)
    {
        UnityEngine.Object[] objects = Resources.FindObjectsOfTypeAll(type);
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null && !objects[i].hideFlags.HasFlag(HideFlags.HideAndDontSave))
                return objects[i];
        }

        return null;
    }

    static void SetProperty(Component component, string propertyName, object value)
    {
        PropertyInfo property = component.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property != null && property.CanWrite && value != null)
            property.SetValue(component, value);
    }

    static void SetField(Component component, string fieldName, object value)
    {
        FieldInfo field = component.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null)
            field.SetValue(component, value);
    }
}
