using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEnvironmentBootstrap : MonoBehaviour
{
    static readonly HashSet<string> ScaledScenes = new HashSet<string>();
    static Material _runtimeEchoPlateMaterial;

    static readonly string[] LegacyDressingPrefixes =
    {
        "FogSlab_",
        "FarArch_",
        "FarCantilever",
        "OuterPillar_",
        "SkyPillar_"
    };

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
        CullLegacyOverlappingDressing();
        AlignPressurePlates();
        ApplyEchoPlateVisuals();
        ApplyCameraProfile();
        ApplyLighting();
        ApplyEchoPlateVisuals();
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

    static void ApplyEchoPlateVisuals()
    {
        PressurePlate[] plates = Object.FindObjectsByType<PressurePlate>(FindObjectsSortMode.None);
        for (int i = 0; i < plates.Length; i++)
        {
            PressurePlate plate = plates[i];
            if (plate == null)
                continue;

            PressurePlateAlignment alignment = plate.GetComponent<PressurePlateAlignment>();
            bool isEchoPlate = alignment != null && alignment.echoProjectionPlate;
            isEchoPlate |= plate.name.Contains("Eco") || plate.name.Contains("Echo");
            if (!isEchoPlate)
                continue;

            Renderer[] renderers = plate.GetComponentsInChildren<Renderer>(true);
            for (int r = 0; r < renderers.Length; r++)
            {
                if (renderers[r] != null)
                    renderers[r].sharedMaterial = GetRuntimeEchoPlateMaterial();
            }

            Light glow = null;
            Transform existing = plate.transform.Find("EchoPlateBlueLight");
            if (existing != null)
                glow = existing.GetComponent<Light>();

            if (glow == null)
            {
                GameObject lightObject = new GameObject("EchoPlateBlueLight");
                lightObject.transform.SetParent(plate.transform, false);
                lightObject.transform.localPosition = new Vector3(0f, 0.65f, 0f);
                glow = lightObject.AddComponent<Light>();
            }

            glow.type = LightType.Point;
            glow.color = new Color(0.06f, 0.82f, 1f, 1f);
            glow.intensity = 3.2f;
            glow.range = 6.5f;
            glow.shadows = LightShadows.Soft;
        }
    }

    static Material GetRuntimeEchoPlateMaterial()
    {
        if (_runtimeEchoPlateMaterial != null)
            return _runtimeEchoPlateMaterial;

        Shader shader = Shader.Find(EchoesUrpMaterials.LitShaderName);
        _runtimeEchoPlateMaterial = new Material(shader);
        _runtimeEchoPlateMaterial.name = "Runtime_EchoPlate_Blue";
        _runtimeEchoPlateMaterial.color = new Color(0.03f, 0.14f, 0.24f, 1f);
        if (_runtimeEchoPlateMaterial.HasProperty("_EmissionColor"))
        {
            _runtimeEchoPlateMaterial.EnableKeyword("_EMISSION");
            _runtimeEchoPlateMaterial.SetColor("_EmissionColor", new Color(0.06f, 0.82f, 1f, 1f) * 3.5f);
        }
        _runtimeEchoPlateMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        return _runtimeEchoPlateMaterial;
    }

    static void CullLegacyOverlappingDressing()
    {
        Transform[] transforms = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform item = transforms[i];
            if (item == null)
                continue;

            string itemName = item.name;
            bool shouldHide = false;
            for (int p = 0; p < LegacyDressingPrefixes.Length; p++)
            {
                if (itemName.StartsWith(LegacyDressingPrefixes[p]))
                {
                    shouldHide = true;
                    break;
                }
            }

            if (shouldHide)
                item.gameObject.SetActive(false);
        }
    }

    public static void ApplyLighting()
    {
        float fogDensity = EchoesPresentationSettings.GameFogDensity;
        float sunIntensity = EchoesPresentationSettings.GameSunIntensity;
        float pointMul = EchoesPresentationSettings.GamePointLightMultiplier;
        float ambientMul = EchoesPresentationSettings.GameAmbientMultiplier;
        const float globalLightScale = 0.72f;
        sunIntensity *= globalLightScale;
        pointMul *= globalLightScale;
        ambientMul *= globalLightScale;

        LevelLightingSettings custom = Object.FindAnyObjectByType<LevelLightingSettings>();
        if (custom != null)
        {
            custom.fogDensity = fogDensity;
            custom.directionalIntensity = sunIntensity;
            custom.pointLightIntensityMultiplier = pointMul;
            custom.directionalColor = new Color(0.44f, 0.54f, 0.7f, 1f);
            custom.ambientSky = new Color(0.035f, 0.05f, 0.09f, 1f) * ambientMul;
            custom.ambientEquator = new Color(0.018f, 0.026f, 0.048f, 1f) * ambientMul;
            custom.ambientGround = new Color(0.006f, 0.008f, 0.014f, 1f) * ambientMul;
            custom.fogColor = new Color(0.018f, 0.024f, 0.038f, 1f);
            custom.reflectionIntensity = 0.18f;
            custom.ApplyNow();
            if (custom.disableRuntimeFillLights)
            {
                // Clean up any stray fill lights if fill lights are disabled
                Light[] strayFills = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
                for (int i = 0; i < strayFills.Length; i++)
                {
                    if (strayFills[i] != null && strayFills[i].name.StartsWith("EchoesFill_"))
                    {
                        if (Application.isPlaying) Object.Destroy(strayFills[i].gameObject);
                        else Object.DestroyImmediate(strayFills[i].gameObject);
                    }
                }
                return;
            }
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.035f, 0.05f, 0.09f) * ambientMul;
        RenderSettings.ambientEquatorColor = new Color(0.018f, 0.026f, 0.048f) * ambientMul;
        RenderSettings.ambientGroundColor = new Color(0.006f, 0.008f, 0.014f) * ambientMul;
        RenderSettings.reflectionIntensity = 0.18f;
        RenderSettings.fog = fogDensity > 0.0001f;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.018f, 0.024f, 0.038f);
        RenderSettings.fogDensity = fogDensity;

        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        for (int i = 0; i < lights.Length; i++)
        {
            Light light = lights[i];
            if (light == null)
                continue;

            if (light.type == LightType.Directional)
            {
                light.intensity = sunIntensity;
                light.color = new Color(0.44f, 0.54f, 0.7f);
                continue;
            }

            if (light.type == LightType.Point && !light.name.StartsWith("EchoesFill_"))
            {
                light.intensity = Mathf.Clamp(light.intensity * pointMul, 0.25f, 3.4f);
                light.range = Mathf.Clamp(light.range * 1.05f, 4f, 16f);
            }
        }

        // Remove existing fill lights to ensure live updates work instantly
        Light[] allLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        for (int i = 0; i < allLights.Length; i++)
        {
            if (allLights[i] != null && allLights[i].name.StartsWith("EchoesFill_"))
            {
                if (Application.isPlaying) Object.Destroy(allLights[i].gameObject);
                else Object.DestroyImmediate(allLights[i].gameObject);
            }
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 origin = player != null ? player.transform.position : Vector3.zero;
        float spread = 18f * EchoesWorldMetrics.LevelGeometryScale;

        SpawnFillLight("EchoesFill_Rim", origin + new Vector3(-spread * 0.45f, 7f, spread * 0.35f), new Color(0.3f, 0.46f, 0.68f), 1.35f * pointMul, spread * 1.55f);
        SpawnFillLight("EchoesFill_Overhead", origin + new Vector3(0f, 14f, 0f), new Color(0.28f, 0.34f, 0.48f), 0.85f * pointMul, spread * 1.8f);
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
