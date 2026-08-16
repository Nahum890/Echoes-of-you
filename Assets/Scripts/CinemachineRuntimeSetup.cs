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

        // Skip if SimpleFollowCamera exists on main camera (gameplay camera)
        var simpleCam = cameraRef.GetComponent<ThirdPersonCamera>();
        if (simpleCam != null)
            return;

        // Skip if this is a gameplay level (Level_XX)
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName.StartsWith("Level_"))
            return;

        // Ensure CinemachineBrain on main camera
        Type brainType = ResolveType("Unity.Cinemachine.CinemachineBrain");
        if (brainType == null) return;
        
        if (cameraRef.GetComponent(brainType) == null)
            cameraRef.gameObject.AddComponent(brainType);

        // Check if PlayerVCam already exists
        Type vcamType = ResolveType("Unity.Cinemachine.CinemachineCamera");
        if (vcamType == null) return;

        Component existingVCam = FindExistingVCam(vcamType);
        if (existingVCam != null)
        {
            SetProperty(existingVCam, "Priority", 10);
            return;
        }

        // Create PlayerVCam
        Transform followTarget = player.transform.Find("CameraFocus") ?? player.transform;
        GameObject rig = new GameObject("PlayerVCam");
        Component vcam = rig.AddComponent(vcamType);
        SetProperty(vcam, "Priority", 10);

        // Transposer/Follow component (CinemachineFollow in v3)
        Type followType = ResolveType("Unity.Cinemachine.CinemachineFollow");
        if (followType != null)
        {
            Component follow = rig.AddComponent(followType);
            SetProperty(follow, "Follow", followTarget);
            SetProperty(follow, "FollowOffset", new Vector3(0f, 1.4f, -4.1f));
            SetProperty(follow, "Damping", new Vector3(25f, 25f, 25f));
            
            // BindingMode
            Type bindingModeType = ResolveType("Unity.Cinemachine.TargetTracking.BindingMode");
            if (bindingModeType != null)
            {
                object bindingMode = Enum.Parse(bindingModeType, "LockToTargetOnAssign");
                SetProperty(follow, "BindingMode", bindingMode);
            }
        }

        // Composer (CinemachineRotationComposer in v3)
        Type composerType = ResolveType("Unity.Cinemachine.CinemachineRotationComposer");
        if (composerType != null)
        {
            Component composer = rig.AddComponent(composerType);
            SetProperty(composer, "TargetOffset", new Vector3(0f, 0f, 0f));
            SetProperty(composer, "Damping", new Vector2(8f, 8f));
            SetProperty(composer, "DeadZoneWidth", 0.1f);
            SetProperty(composer, "DeadZoneHeight", 0.1f);
            SetProperty(composer, "SoftZoneWidth", 0.8f);
            SetProperty(composer, "SoftZoneHeight", 0.8f);
            SetProperty(composer, "ScreenX", 0.5f);
            SetProperty(composer, "ScreenY", 0.5f);
            SetProperty(composer, "LookaheadTime", 0.1f);
            SetProperty(composer, "LookaheadSmoothing", 8f);
        }

        // Lens
        PropertyInfo lensProperty = vcamType.GetProperty("Lens", BindingFlags.Instance | BindingFlags.Public);
        if (lensProperty != null)
        {
            object lens = lensProperty.GetValue(vcam);
            Type lensType = lensProperty.PropertyType;
            
            SetProperty(lens, "FieldOfView", 52f);
            SetProperty(lens, "NearClipPlane", 0.3f);
            SetProperty(lens, "FarClipPlane", 300f);
            SetProperty(lens, "Dutch", 0f);
            
            lensProperty.SetValue(vcam, lens);
        }

        Debug.Log("[CinemachineRuntimeSetup] PlayerVCam created with PS1 settings (v3 API)");
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

    static Component FindExistingVCam(Type vcamType)
    {
        UnityEngine.Object[] objects = Resources.FindObjectsOfTypeAll(vcamType);
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null && !objects[i].hideFlags.HasFlag(HideFlags.HideAndDontSave))
                return objects[i] as Component;
        }
        return null;
    }

    static void SetProperty(Component component, string propertyName, object value)
    {
        PropertyInfo property = component.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property != null && property.CanWrite && value != null)
            property.SetValue(component, value);
    }

    static void SetProperty(object obj, string propertyName, object value)
    {
        PropertyInfo property = obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property != null && property.CanWrite && value != null)
            property.SetValue(obj, value);
    }
}