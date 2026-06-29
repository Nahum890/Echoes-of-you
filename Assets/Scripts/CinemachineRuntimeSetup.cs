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

        Type virtualCameraType = ResolveType("Cinemachine.CinemachineVirtualCamera");
        Type brainType = ResolveType("Cinemachine.CinemachineBrain");
        if (virtualCameraType == null || brainType == null)
            return;

        if (cameraRef.GetComponent(brainType) == null)
            cameraRef.gameObject.AddComponent(brainType);

        if (FindExisting(virtualCameraType) != null)
            return;

        Transform followTarget = player.transform.Find("CameraFocus") ?? player.transform;
        GameObject rig = new GameObject("CinematicPlayerVCam");
        Component vcam = rig.AddComponent(virtualCameraType);
        SetProperty(vcam, "Follow", followTarget);
        SetProperty(vcam, "LookAt", followTarget);
        SetProperty(vcam, "Priority", 20);
        SetProperty(vcam, "m_Lens", CreateLensSettings(virtualCameraType, 52f));

        Component transposer = InvokeGenericComponentGetter(vcam, "Cinemachine.CinemachineTransposer");
        if (transposer != null)
        {
            SetField(transposer, "m_FollowOffset", new Vector3(-5.5f, 3.2f, -9.5f));
            SetField(transposer, "m_XDamping", 0.55f);
            SetField(transposer, "m_YDamping", 0.65f);
            SetField(transposer, "m_ZDamping", 0.5f);
        }

        Component composer = InvokeGenericComponentGetter(vcam, "Cinemachine.CinemachineComposer");
        if (composer != null)
        {
            SetField(composer, "m_TrackedObjectOffset", new Vector3(0f, 0.35f, 0f));
            SetField(composer, "m_ScreenX", 0.48f);
            SetField(composer, "m_ScreenY", 0.42f);
            SetField(composer, "m_DeadZoneWidth", 0.12f);
            SetField(composer, "m_DeadZoneHeight", 0.1f);
            SetField(composer, "m_SoftZoneWidth", 0.55f);
            SetField(composer, "m_SoftZoneHeight", 0.48f);
            SetField(composer, "m_HorizontalDamping", 0.45f);
            SetField(composer, "m_VerticalDamping", 0.55f);
        }
    }

    static Type ResolveType(string typeName)
    {
        Type type = Type.GetType(typeName + ", Cinemachine");
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

    static object CreateLensSettings(Type virtualCameraType, float fieldOfView)
    {
        FieldInfo lensField = virtualCameraType.GetField("m_Lens", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (lensField == null)
            return null;

        object lens = Activator.CreateInstance(lensField.FieldType);
        FieldInfo fovField = lensField.FieldType.GetField("FieldOfView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (fovField != null)
            fovField.SetValue(lens, fieldOfView);
        return lens;
    }

    static Component InvokeGenericComponentGetter(Component vcam, string componentTypeName)
    {
        Type componentType = ResolveType(componentTypeName);
        if (componentType == null)
            return null;

        MethodInfo method = vcam.GetType().GetMethod("GetCinemachineComponent", BindingFlags.Instance | BindingFlags.Public);
        if (method == null)
            return null;

        MethodInfo generic = method.MakeGenericMethod(componentType);
        return generic.Invoke(vcam, null) as Component;
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
