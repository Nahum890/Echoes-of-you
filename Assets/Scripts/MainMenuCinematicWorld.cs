using UnityEngine;

/// <summary>
/// Escena 3D viva detrás del menú: arquitectura flotante, ecos distantes, niebla y cámara orbital.
/// Implementa un sistema de ambientes dinámicos estilo PS2 que cambian según la selección.
/// </summary>
public class MainMenuCinematicWorld : MonoBehaviour
{
    public enum MenuAmbience
    {
        Void,
        Stability,
        System,
        Disconnect
    }

    [SerializeField] int floatingPlatforms = 14;
    [SerializeField] int distantEchoWalkers = 5;
    [SerializeField] float orbitRadius = 22f;
    [SerializeField] float orbitSpeed = 4.5f;
    [SerializeField] float fogPulseSpeed = 0.15f;

    public static MainMenuCinematicWorld Instance { get; private set; }

    Transform _cameraPivot;
    float _orbitAngle;
    Material _archMat;
    Material _echoMat;

    struct PlatformData
    {
        public Transform transform;
        public Vector3 basePosition;
        public Vector3 baseScale;
    }
    System.Collections.Generic.List<PlatformData> _platforms = new System.Collections.Generic.List<PlatformData>();

    // Ambience Targets
    MenuAmbience _targetAmbience = MenuAmbience.Void;
    float _targetOrbitSpeed;
    float _targetOrbitRadius;
    float _targetFogDensity;
    Color _targetFogColor;
    Color _targetAmbientLight;
    Color _targetArchColor;
    Color _targetEchoColor;
    float _targetPlatformHeightScatter;
    float _targetPlatformRadiusScatter;
    float _targetPlatformScaleMul;
    float _targetEchoSpeed;

    // Interpolation Current values
    float _currentOrbitSpeed;
    float _currentOrbitRadius;
    float _currentFogDensity;
    Color _currentFogColor;
    Color _currentAmbientLight;
    Color _currentArchColor;
    Color _currentEchoColor;
    float _currentPlatformHeightScatter;
    float _currentPlatformRadiusScatter;
    float _currentPlatformScaleMul;
    float _currentEchoSpeed;

    public float CurrentEchoSpeed => _currentEchoSpeed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MainMenu")
            return;

        if (FindAnyObjectByType<MainMenuCinematicWorld>() != null)
            return;

        GameObject root = new GameObject("MainMenuCinematicWorld");
        root.AddComponent<MainMenuCinematicWorld>();
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        BuildMaterials();
        BuildVoid();
        BuildArchitecture();
        BuildDistantEchoes();
        BuildCameraRig();
        ApplyLiminalFog();

        // Initialize targets & currents to Void ambience immediately to prevent startup pops
        SetAmbience(MenuAmbience.Void);
        _currentOrbitSpeed = _targetOrbitSpeed;
        _currentOrbitRadius = _targetOrbitRadius;
        _currentFogDensity = _targetFogDensity;
        _currentFogColor = _targetFogColor;
        _currentAmbientLight = _targetAmbientLight;
        _currentArchColor = _targetArchColor;
        _currentEchoColor = _targetEchoColor;
        _currentPlatformHeightScatter = _targetPlatformHeightScatter;
        _currentPlatformRadiusScatter = _targetPlatformRadiusScatter;
        _currentPlatformScaleMul = _targetPlatformScaleMul;
        _currentEchoSpeed = _targetEchoSpeed;
    }

    public void SetAmbience(MenuAmbience ambience)
    {
        _targetAmbience = ambience;

        switch (ambience)
        {
            case MenuAmbience.Void:
                _targetOrbitSpeed = orbitSpeed * 1.44f;
                _targetOrbitRadius = 16f;
                _targetFogDensity = 0.045f;
                _targetFogColor = new Color(0.02f, 0.05f, 0.08f, 1f);
                _targetAmbientLight = new Color(0.12f, 0.22f, 0.35f, 1f);
                _targetArchColor = new Color(0.18f, 0.45f, 0.65f, 1f);
                _targetEchoColor = new Color(0.35f, 0.85f, 1f, 0.65f);
                _targetPlatformHeightScatter = 1.8f;
                _targetPlatformRadiusScatter = 1.2f;
                _targetPlatformScaleMul = 1.0f;
                _targetEchoSpeed = 1.8f;
                break;

            case MenuAmbience.Stability:
                _targetOrbitSpeed = 3.0f;
                _targetOrbitRadius = 24f;
                _targetFogDensity = 0.022f;
                _targetFogColor = new Color(0.05f, 0.07f, 0.12f, 1f);
                _targetAmbientLight = new Color(0.2f, 0.24f, 0.34f, 1f);
                _targetArchColor = new Color(0.25f, 0.32f, 0.48f, 1f);
                _targetEchoColor = new Color(0.4f, 0.7f, 0.95f, 0.45f);
                _targetPlatformHeightScatter = 0.4f;
                _targetPlatformRadiusScatter = 0.8f;
                _targetPlatformScaleMul = 1.1f;
                _targetEchoSpeed = 0.8f;
                break;

            case MenuAmbience.System:
                _targetOrbitSpeed = 1.5f;
                _targetOrbitRadius = 18f;
                _targetFogDensity = 0.012f;
                _targetFogColor = new Color(0.02f, 0.06f, 0.06f, 1f);
                _targetAmbientLight = new Color(0.1f, 0.25f, 0.22f, 1f);
                _targetArchColor = new Color(0.12f, 0.5f, 0.45f, 1f);
                _targetEchoColor = new Color(0.2f, 0.8f, 0.6f, 0.5f);
                _targetPlatformHeightScatter = 0.1f;
                _targetPlatformRadiusScatter = 0.6f;
                _targetPlatformScaleMul = 0.8f;
                _targetEchoSpeed = 0.4f;
                break;

            case MenuAmbience.Disconnect:
                _targetOrbitSpeed = 0.5f;
                _targetOrbitRadius = 32f;
                _targetFogDensity = 0.075f;
                _targetFogColor = new Color(0.08f, 0.03f, 0.03f, 1f);
                _targetAmbientLight = new Color(0.18f, 0.08f, 0.08f, 1f);
                _targetArchColor = new Color(0.48f, 0.18f, 0.2f, 1f);
                _targetEchoColor = new Color(0.6f, 0.2f, 0.2f, 0.08f);
                _targetPlatformHeightScatter = 3.0f;
                _targetPlatformRadiusScatter = 2.0f;
                _targetPlatformScaleMul = 0.6f;
                _targetEchoSpeed = 0.1f;
                break;
        }
    }

    void Update()
    {
        // Smoothly interpolate parameters — target Void transitions take 1.2s, specialized targets take 0.8s
        float lerpFactor = (_targetAmbience == MenuAmbience.Void) ? 2.5f : 3.75f;
        float dt = Time.deltaTime;

        _currentOrbitSpeed = Mathf.Lerp(_currentOrbitSpeed, _targetOrbitSpeed, lerpFactor * dt);
        _currentOrbitRadius = Mathf.Lerp(_currentOrbitRadius, _targetOrbitRadius, lerpFactor * dt);
        _currentFogDensity = Mathf.Lerp(_currentFogDensity, _targetFogDensity, lerpFactor * dt);
        _currentFogColor = Color.Lerp(_currentFogColor, _targetFogColor, lerpFactor * dt);
        _currentAmbientLight = Color.Lerp(_currentAmbientLight, _targetAmbientLight, lerpFactor * dt);
        _currentArchColor = Color.Lerp(_currentArchColor, _targetArchColor, lerpFactor * dt);
        _currentEchoColor = Color.Lerp(_currentEchoColor, _targetEchoColor, lerpFactor * dt);
        _currentPlatformHeightScatter = Mathf.Lerp(_currentPlatformHeightScatter, _targetPlatformHeightScatter, lerpFactor * dt);
        _currentPlatformRadiusScatter = Mathf.Lerp(_currentPlatformRadiusScatter, _targetPlatformRadiusScatter, lerpFactor * dt);
        _currentPlatformScaleMul = Mathf.Lerp(_currentPlatformScaleMul, _targetPlatformScaleMul, lerpFactor * dt);
        _currentEchoSpeed = Mathf.Lerp(_currentEchoSpeed, _targetEchoSpeed, lerpFactor * dt);

        // Update Camera Rig rotation
        if (_cameraPivot != null)
        {
            _orbitAngle += _currentOrbitSpeed * dt;
            _cameraPivot.rotation = Quaternion.Euler(12f, _orbitAngle, 0f);
        }

        // Update Camera Rig local zoom
        Camera main = Camera.main;
        if (main != null && main.transform.parent == _cameraPivot)
        {
            main.transform.localPosition = new Vector3(0f, 3f, -_currentOrbitRadius);
        }

        // Update Materials
        if (_archMat != null)
        {
            _archMat.color = _currentArchColor;
        }
        if (_echoMat != null)
        {
            _echoMat.color = _currentEchoColor;
        }

        // Update Fog & Ambient lighting
        float fogPulse = Mathf.Sin(Time.time * fogPulseSpeed) * (_currentFogDensity * 0.15f);
        RenderSettings.fogDensity = _currentFogDensity + fogPulse;
        RenderSettings.fogColor = _currentFogColor;
        RenderSettings.ambientLight = _currentAmbientLight;

        // Update Platform Positions, Scales, and physical float wobbling
        for (int i = 0; i < _platforms.Count; i++)
        {
            PlatformData platform = _platforms[i];
            if (platform.transform == null) continue;

            Vector3 horizDir = new Vector3(platform.basePosition.x, 0f, platform.basePosition.z);
            float baseRadius = horizDir.magnitude;
            Vector3 horizNorm = baseRadius > 0.001f ? horizDir.normalized : Vector3.forward;

            float radius = baseRadius * _currentPlatformRadiusScatter;
            float height = platform.basePosition.y * _currentPlatformHeightScatter;

            // Physical hover/wobble animation
            float wobble = Mathf.Sin(Time.time * 0.8f + i) * 0.2f;

            platform.transform.position = horizNorm * radius + new Vector3(0f, height + wobble, 0f);
            platform.transform.localScale = platform.baseScale * _currentPlatformScaleMul;
        }
    }

    void BuildMaterials()
    {
        Shader shader = Shader.Find("Standard");
        _archMat = new Material(shader);
        _archMat.color = new Color(0.22f, 0.26f, 0.34f, 1f);
        _archMat.SetFloat("_Metallic", 0.15f);
        _archMat.SetFloat("_Glossiness", 0.35f);

        _echoMat = new Material(shader);
        _echoMat.color = new Color(0.35f, 0.85f, 1f, 0.55f);
        _echoMat.SetFloat("_Mode", 3f);
        _echoMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        _echoMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        _echoMat.SetInt("_ZWrite", 0);
        _echoMat.EnableKeyword("_ALPHABLEND_ON");
        _echoMat.renderQueue = 3000;
    }

    void BuildVoid()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(0.04f, 0.06f, 0.1f, 1f);
        RenderSettings.ambientLight = new Color(0.18f, 0.22f, 0.3f, 1f);

        GameObject lightObj = new GameObject("MenuMoon");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(0.55f, 0.7f, 0.95f, 1f);
        light.intensity = 0.65f;
        lightObj.transform.rotation = Quaternion.Euler(28f, -40f, 0f);
    }

    void BuildArchitecture()
    {
        Transform archRoot = new GameObject("FloatingArchitecture").transform;
        archRoot.SetParent(transform);
        _platforms.Clear();

        for (int i = 0; i < floatingPlatforms; i++)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.transform.SetParent(archRoot);
            float angle = i * (360f / floatingPlatforms) * Mathf.Deg2Rad;
            float radius = 8f + (i % 4) * 2.5f;
            float height = Mathf.Sin(i * 1.7f) * 3f;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, height, Mathf.Sin(angle) * radius);
            Vector3 scale = new Vector3(
                Random.Range(2f, 6f),
                Random.Range(0.3f, 1.2f),
                Random.Range(2f, 5f));
            block.transform.position = pos;
            block.transform.localScale = scale;
            block.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), Random.Range(-8f, 8f));
            block.GetComponent<Renderer>().sharedMaterial = _archMat;
            Destroy(block.GetComponent<Collider>());

            _platforms.Add(new PlatformData { transform = block.transform, basePosition = pos, baseScale = scale });

            if (i % 3 == 0)
            {
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.transform.SetParent(block.transform);
                pillar.transform.localPosition = new Vector3(0f, 2.5f, 0f);
                pillar.transform.localScale = new Vector3(0.4f, 3f, 0.4f);
                pillar.GetComponent<Renderer>().sharedMaterial = _archMat;
                Destroy(pillar.GetComponent<Collider>());
            }
        }
    }

    void BuildDistantEchoes()
    {
        Transform echoRoot = new GameObject("DistantEchoes").transform;
        echoRoot.SetParent(transform);

        for (int i = 0; i < distantEchoWalkers; i++)
        {
            GameObject echo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            echo.name = $"DistantEcho_{i}";
            echo.transform.SetParent(echoRoot);
            echo.transform.position = new Vector3(Random.Range(-30f, 30f), Random.Range(-2f, 4f), Random.Range(15f, 40f));
            echo.transform.localScale = Vector3.one * 0.85f;
            echo.GetComponent<Renderer>().sharedMaterial = _echoMat;
            Destroy(echo.GetComponent<Collider>());
            echo.AddComponent<MenuEchoWalker>();
        }
    }

    void BuildCameraRig()
    {
        Camera main = Camera.main;
        if (main == null)
            return;

        _cameraPivot = new GameObject("MenuCameraOrbit").transform;
        _cameraPivot.position = Vector3.zero;
        main.transform.SetParent(_cameraPivot);
        main.transform.localPosition = new Vector3(0f, 3f, -orbitRadius);
        main.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);
    }

    void ApplyLiminalFog()
    {
        EchoesPresentationSettings.ApplyLightingPreset("liminal");
    }

    class MenuEchoWalker : MonoBehaviour
    {
        Vector3 _start;
        float _phase;
        float _timeAccumulator;

        void Start()
        {
            _start = transform.position;
            _phase = Random.value * 10f;
        }

        void Update()
        {
            float speed = MainMenuCinematicWorld.Instance != null ? MainMenuCinematicWorld.Instance.CurrentEchoSpeed : 1f;
            _timeAccumulator += Time.deltaTime * speed;
            float t = _timeAccumulator + _phase;
            transform.position = _start + new Vector3(Mathf.Sin(t * 0.4f) * 4f, Mathf.Sin(t * 0.7f) * 0.3f, Mathf.Cos(t * 0.35f) * 3f);
        }
    }
}
