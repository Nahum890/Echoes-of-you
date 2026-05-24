using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEnvironmentBootstrap : MonoBehaviour
{
    static readonly HashSet<string> ScaledScenes = new HashSet<string>();

    static readonly string[] LevelRootNames =
    {
        "--- ENVIRONMENT ---",
        "--- MECHANICS ---",
        "--- DRESSING ---",
        "--- DECOR ---"
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetCaches()
    {
        ScaledScenes.Clear();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void BootstrapAfterLoad()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        string scene = SceneManager.GetActiveScene().name;
        if (!scene.StartsWith("Level_"))
            return;

        ApplyLevelGeometryScale(scene);
        ApplyPlayerAnimationSetup();
        UnlockPlayerControl();
        ApplyHazardScales();
        ApplyObstacleHeights();
        AlignPressurePlates();
        ApplyCameraProfile();
        ApplyLighting();
        EnsureExperienceSystems();
    }

    static void EnsureExperienceSystems()
    {
        if (Object.FindAnyObjectByType<LevelExperienceBlueprint>() == null)
        {
            GameObject blueprint = new GameObject("LevelExperienceBlueprint");
            blueprint.AddComponent<LevelExperienceBlueprint>();
        }

        if (Object.FindAnyObjectByType<LevelEscapeSequence>() == null)
        {
            GameObject escape = new GameObject("LevelEscapeSequence");
            escape.AddComponent<LevelEscapeSequence>();
        }
    }

    static void OnSceneUnloaded(Scene scene)
    {
        if (scene.name.StartsWith("Level_"))
            ScaledScenes.Remove(scene.name);
    }

    static void ApplyLevelGeometryScale(string sceneName)
    {
        if (ScaledScenes.Contains(sceneName))
            return;

        float scale = EchoesWorldMetrics.LevelGeometryScale;
        bool scaledAny = false;

        for (int i = 0; i < LevelRootNames.Length; i++)
        {
            GameObject rootObject = GameObject.Find(LevelRootNames[i]);
            if (rootObject == null)
                continue;

            Transform root = rootObject.transform;
            if (ScaleRootChildrenOnce(root, scale))
                scaledAny = true;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            LevelSpawnMarker marker = player.GetComponent<LevelSpawnMarker>();
            if (marker == null)
            {
                marker = player.AddComponent<LevelSpawnMarker>();
                marker.OriginalPosition = player.transform.position;
            }

            if (!marker.PositionScaled)
            {
                Vector3 original = marker.OriginalPosition;
                player.transform.position = new Vector3(
                    original.x * scale,
                    original.y,
                    original.z * scale);
                marker.PositionScaled = true;
            }

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.height = EchoesWorldMetrics.PlayerHeight;
                cc.radius = EchoesWorldMetrics.PlayerRadius;
                cc.center = new Vector3(0f, EchoesWorldMetrics.PlayerCenterY, 0f);
                cc.enabled = true;
            }
        }

        if (scaledAny)
            ScaledScenes.Add(sceneName);
    }

    static void UnlockPlayerControl()
    {
        PlayerController player = Object.FindAnyObjectByType<PlayerController>();
        if (player != null)
            player.ForceUnlockAndReset();

        EchoRecorder recorder = Object.FindAnyObjectByType<EchoRecorder>();
        if (recorder != null)
            recorder.ForceUnlockPlayer();
    }

    static void ApplyPlayerAnimationSetup()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        if (player.GetComponent<PlayerLocomotionAnimator>() == null)
            player.AddComponent<PlayerLocomotionAnimator>();
        if (player.GetComponent<PlayerAnimationRuntimeBootstrap>() == null)
            player.AddComponent<PlayerAnimationRuntimeBootstrap>();

        PlayerAnimationRuntimeBootstrap.ApplyToHierarchy(player);

        PlayerProceduralAnimator procedural = player.GetComponentInChildren<PlayerProceduralAnimator>(true);
        if (procedural != null)
            procedural.enabled = EchoesPresentationSettings.ProceduralMotionEnabled;
    }

    static void ApplyHazardScales()
    {
        EchoShieldField[] fields = Object.FindObjectsByType<EchoShieldField>(FindObjectsSortMode.None);
        for (int i = 0; i < fields.Length; i++)
        {
            BoxCollider box = fields[i].GetComponent<BoxCollider>();
            if (box == null)
                continue;

            Vector3 size = box.size;
            size.x *= EchoesWorldMetrics.HazardThicknessScale;
            size.z *= EchoesWorldMetrics.HazardThicknessScale;
            size.y = Mathf.Max(size.y, EchoesWorldMetrics.HazardMinHeight);
            box.size = size;
        }
    }

    static void ApplyCameraProfile()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (!LevelCameraProfiles.TryGet(sceneName, out LevelCameraProfiles.Profile profile))
            return;

        float world = EchoesWorldMetrics.LevelGeometryScale;
        profile.followOffset *= world;

        FixedPuzzleCameraController fixedCam = FixedPuzzleCameraController.ResolveActive();
        if (fixedCam != null)
        {
            fixedCam.baseFov = profile.fov;
            fixedCam.playerWeight = profile.playerWeight;
            fixedCam.goalWeight = profile.goalWeight;
        }

        CinematicCameraDynamics dynamics = Object.FindAnyObjectByType<CinematicCameraDynamics>();
        if (dynamics != null)
            dynamics.ApplyProfile(profile);
    }

    static void ApplyObstacleHeights()
    {
        DoorController[] doors = Object.FindObjectsByType<DoorController>(FindObjectsSortMode.None);
        for (int i = 0; i < doors.Length; i++)
        {
            Transform doorTransform = doors[i].transform;
            Vector3 scale = doorTransform.localScale;
            if (scale.y < EchoesWorldMetrics.MinDoorHeight)
            {
                float delta = EchoesWorldMetrics.MinDoorHeight - scale.y;
                doorTransform.localScale = new Vector3(scale.x, EchoesWorldMetrics.MinDoorHeight, scale.z);
                doorTransform.position += new Vector3(0f, delta * 0.5f, 0f);
            }
        }

        KillVolume[] killVolumes = Object.FindObjectsByType<KillVolume>(FindObjectsSortMode.None);
        for (int i = 0; i < killVolumes.Length; i++)
        {
            BoxCollider box = killVolumes[i].GetComponent<BoxCollider>();
            if (box == null)
                continue;

            Vector3 size = box.size;
            size.y = Mathf.Max(size.y, EchoesWorldMetrics.MinBarrierHeight);
            box.size = size;
        }
    }

    static void AlignPressurePlates()
    {
        PressurePlate[] plates = Object.FindObjectsByType<PressurePlate>(FindObjectsSortMode.None);
        for (int i = 0; i < plates.Length; i++)
        {
            if (plates[i] == null)
                continue;

            PressurePlateAlignment alignment = plates[i].GetComponent<PressurePlateAlignment>();
            if (alignment == null)
                alignment = plates[i].gameObject.AddComponent<PressurePlateAlignment>();

            alignment.SnapToSurface();
            if (alignment.echoProjectionPlate)
                alignment.ExpandTriggerForProjection();
        }
    }

    static void ApplyLighting()
    {
        float fogDensity = EchoesPresentationSettings.GameFogDensity;
        float sunIntensity = EchoesPresentationSettings.GameSunIntensity;
        float pointMul = EchoesPresentationSettings.GamePointLightMultiplier;
        float ambientMul = EchoesPresentationSettings.GameAmbientMultiplier;
        const float globalLightScale = 0.92f;
        sunIntensity *= globalLightScale;
        pointMul *= globalLightScale;
        ambientMul *= globalLightScale;

        LevelLightingSettings custom = Object.FindAnyObjectByType<LevelLightingSettings>();
        if (custom != null)
        {
            custom.fogDensity = fogDensity;
            custom.directionalIntensity = sunIntensity;
            custom.pointLightIntensityMultiplier = pointMul;
            custom.ApplyNow();
            if (custom.disableRuntimeFillLights || HasEchoesFillLights())
                return;
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.34f, 0.36f, 0.44f) * ambientMul;
        RenderSettings.ambientEquatorColor = new Color(0.24f, 0.26f, 0.32f) * ambientMul;
        RenderSettings.ambientGroundColor = new Color(0.14f, 0.15f, 0.19f) * ambientMul;
        RenderSettings.reflectionIntensity = 0.42f;
        RenderSettings.fog = fogDensity > 0.0001f;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(0.18f, 0.2f, 0.28f);
        RenderSettings.fogDensity = fogDensity;

        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        for (int i = 0; i < lights.Length; i++)
        {
            Light light = lights[i];
            if (light == null)
                continue;

            if (light.type == LightType.Directional)
            {
                light.intensity = Mathf.Max(light.intensity, sunIntensity);
                light.color = Color.Lerp(light.color, new Color(0.9f, 0.94f, 1f), 0.4f);
                continue;
            }

            if (light.type == LightType.Point)
            {
                light.intensity = Mathf.Max(light.intensity * pointMul, 3.2f * pointMul);
                light.range = Mathf.Max(light.range * 1.15f, 16f);
            }
        }

        if (HasEchoesFillLights())
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 origin = player != null ? player.transform.position : Vector3.zero;
        float spread = 18f * EchoesWorldMetrics.LevelGeometryScale;

        SpawnFillLight("EchoesFill_Key", origin + new Vector3(spread * 0.4f, 10f, -spread * 0.3f), new Color(0.78f, 0.8f, 0.9f), 4.2f * pointMul, spread * 2f);
        SpawnFillLight("EchoesFill_Rim", origin + new Vector3(-spread * 0.45f, 7f, spread * 0.35f), new Color(0.62f, 0.7f, 0.84f), 3.4f * pointMul, spread * 1.7f);
        SpawnFillLight("EchoesFill_Warm", origin + new Vector3(-spread * 0.2f, 5f, -spread * 0.5f), new Color(0.88f, 0.68f, 0.5f), 2.8f * pointMul, spread * 1.5f);
        SpawnFillLight("EchoesFill_Cool", origin + new Vector3(spread * 0.3f, 6f, spread * 0.45f), new Color(0.52f, 0.74f, 0.88f), 3f * pointMul, spread * 1.6f);
        SpawnFillLight("EchoesFill_Overhead", origin + new Vector3(0f, 14f, 0f), new Color(0.88f, 0.9f, 0.95f), 4f * pointMul, spread * 2.2f);
        SpawnFillLight("EchoesFill_Ground", origin + new Vector3(0f, 2f, 0f), new Color(0.42f, 0.46f, 0.55f), 1.8f * pointMul, spread * 1.4f);
    }

    static bool HasEchoesFillLights()
    {
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null && lights[i].name.StartsWith("EchoesFill_"))
                return true;
        }

        return false;
    }

    static bool ScaleRootChildrenOnce(Transform root, float scale)
    {
        if (root == null || Mathf.Approximately(scale, 1f))
            return false;

        if (root.localScale.x > 1.05f)
            root.localScale = Vector3.one;

        bool scaledAny = false;
        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child == null)
                continue;

            LevelScaledChild marker = child.GetComponent<LevelScaledChild>();
            if (marker == null)
                marker = child.gameObject.AddComponent<LevelScaledChild>();
            if (marker.Scaled)
                continue;

            Vector3 localPosition = child.localPosition;
            child.localPosition = new Vector3(localPosition.x * scale, localPosition.y, localPosition.z * scale);
            child.localScale = Vector3.Scale(child.localScale, Vector3.one * scale);
            marker.Scaled = true;
            scaledAny = true;
        }

        return scaledAny;
    }

    static void SpawnFillLight(string name, Vector3 position, Color color, float intensity, float range)
    {
        GameObject lightObject = new GameObject(name);
        lightObject.transform.position = position;
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.None;
    }
}
