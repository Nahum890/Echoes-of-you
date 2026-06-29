using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Escena 3D viva detrás del menú: arquitectura y elementos interactivos procedimentales.
/// Implementa un sistema de ambientes dinámicos estilo PS2 con iluminación volumétrica simulada y sombras suaves.
/// Recrea las escenas visuales de "Vortex" y "Crystals/Circuit" de forma tridimensional, animada y de alta fidelidad.
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

    [SerializeField] int distantEchoWalkers = 5;
    [SerializeField] float orbitRadius = 22f;
    [SerializeField] float orbitSpeed = 4.5f;
    [SerializeField] float fogPulseSpeed = 0.15f;

    public static MainMenuCinematicWorld Instance { get; private set; }

    Transform _cameraPivot;
    float _orbitAngle;
    Material _archMat;
    Material _echoMat;
    Material _circuitMat;
    Material _packetMat;

    Light _dirLight;
    Light _vortexLight;

    // Ambience Targets
    MenuAmbience _targetAmbience = MenuAmbience.Void;
    float _targetOrbitSpeed;
    float _targetOrbitRadius;
    float _targetFogDensity;
    Color _targetFogColor;
    Color _targetAmbientLight;
    Color _targetArchColor;
    Color _targetEchoColor;
    float _targetEchoSpeed;

    // Light target settings
    float _targetLightIntensity = 0.65f;
    Color _targetLightColor = new Color(0.55f, 0.7f, 0.95f, 1f);
    float _currentLightIntensity = 0.65f;
    Color _currentLightColor = new Color(0.55f, 0.7f, 0.95f, 1f);

    // Camera target settings
    float _targetCameraPitch = 12f;
    float _targetCameraYOffset = 3f;
    float _currentCameraPitch = 12f;
    float _currentCameraYOffset = 3f;

    // Container Scale targets
    float _targetVoidScale = 1f;
    float _targetStabilityScale = 0f;
    float _targetSystemScale = 0f;
    float _targetDisconnectScale = 0f;

    float _currentVoidScale = 1f;
    float _currentStabilityScale = 0f;
    float _currentSystemScale = 0f;
    float _currentDisconnectScale = 0f;

    // Roots for each category
    Transform _voidRoot;
    Transform _stabilityRoot;
    Transform _systemRoot;
    Transform _disconnectRoot;

    // Interpolation Current values
    float _currentOrbitSpeed;
    float _currentOrbitRadius;
    float _currentFogDensity;
    Color _currentFogColor;
    Color _currentAmbientLight;
    Color _currentArchColor;
    Color _currentEchoColor;
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

        // Build 3D environments procedurally
        BuildVoidVortex();
        BuildStabilityGrid();
        BuildSystemCalibration();
        BuildDisconnect();

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
        _currentEchoSpeed = _targetEchoSpeed;
        _currentCameraPitch = _targetCameraPitch;
        _currentCameraYOffset = _targetCameraYOffset;
        _currentLightIntensity = _targetLightIntensity;
        _currentLightColor = _targetLightColor;

        // Initialize scale states
        _currentVoidScale = 1f;
        _currentStabilityScale = 0f;
        _currentSystemScale = 0f;
        _currentDisconnectScale = 0f;

        if (_voidRoot != null) _voidRoot.localScale = Vector3.one;
        if (_stabilityRoot != null) _stabilityRoot.localScale = Vector3.zero;
        if (_systemRoot != null) _systemRoot.localScale = Vector3.zero;
        if (_disconnectRoot != null) _disconnectRoot.localScale = Vector3.zero;
    }

    public void SetAmbience(MenuAmbience ambience)
    {
        _targetAmbience = ambience;

        // Set all scale targets to 0 first, then enable the active one
        _targetVoidScale = 0f;
        _targetStabilityScale = 0f;
        _targetSystemScale = 0f;
        _targetDisconnectScale = 0f;

        switch (ambience)
        {
            case MenuAmbience.Void:
                _targetVoidScale = 1f;
                _targetOrbitSpeed = orbitSpeed * 1.5f;
                _targetOrbitRadius = 14f;
                _targetFogDensity = 0.045f;
                _targetFogColor = new Color(0.01f, 0.03f, 0.05f, 1f);
                _targetAmbientLight = new Color(0.05f, 0.15f, 0.2f, 1f);
                _targetArchColor = new Color(0.08f, 0.12f, 0.18f, 1f);
                _targetEchoColor = new Color(0.35f, 0.85f, 1f, 0.8f);
                _targetEchoSpeed = 2.0f;
                _targetCameraPitch = 25f;
                _targetCameraYOffset = 2f;
                _targetLightIntensity = 0.15f; // Dim the moon, let center vortex glow dominate
                _targetLightColor = new Color(0.3f, 0.5f, 0.8f, 1f);
                break;

            case MenuAmbience.Stability:
                _targetStabilityScale = 1f;
                _targetOrbitSpeed = 1.5f;
                _targetOrbitRadius = 20f;
                _targetFogDensity = 0.02f;
                _targetFogColor = new Color(0.02f, 0.03f, 0.06f, 1f);
                _targetAmbientLight = new Color(0.1f, 0.15f, 0.25f, 1f);
                _targetArchColor = new Color(0.2f, 0.3f, 0.5f, 1f);
                _targetEchoColor = new Color(0.4f, 0.6f, 0.9f, 0.6f);
                _targetEchoSpeed = 0.5f;
                _targetCameraPitch = 55f;
                _targetCameraYOffset = 6f;
                _targetLightIntensity = 0.5f;
                _targetLightColor = new Color(0.5f, 0.6f, 0.8f, 1f);
                break;

            case MenuAmbience.System:
                _targetSystemScale = 1f;
                _targetOrbitSpeed = 1.0f;
                _targetOrbitRadius = 16f;
                _targetFogDensity = 0.015f;
                _targetFogColor = new Color(0.01f, 0.04f, 0.04f, 1f);
                _targetAmbientLight = new Color(0.05f, 0.2f, 0.18f, 1f);
                _targetArchColor = new Color(0.15f, 0.25f, 0.22f, 1f);
                _targetEchoColor = new Color(0.25f, 0.9f, 0.7f, 0.7f);
                _targetEchoSpeed = 0.8f;
                _targetCameraPitch = 15f;
                _targetCameraYOffset = 1.2f;
                _targetLightIntensity = 0.1f; // Dim the moon, let the calibration spotlights shine
                _targetLightColor = new Color(0.2f, 0.7f, 0.6f, 1f);
                break;

            case MenuAmbience.Disconnect:
                _targetDisconnectScale = 1f;
                _targetOrbitSpeed = 0.4f;
                _targetOrbitRadius = 24f;
                _targetFogDensity = 0.06f;
                _targetFogColor = new Color(0.06f, 0.02f, 0.02f, 1f);
                _targetAmbientLight = new Color(0.15f, 0.05f, 0.05f, 1f);
                _targetArchColor = new Color(0.4f, 0.15f, 0.15f, 1f);
                _targetEchoColor = new Color(0.8f, 0.2f, 0.2f, 0.3f);
                _targetEchoSpeed = 0.2f;
                _targetCameraPitch = 15f;
                _targetCameraYOffset = 3f;
                _targetLightIntensity = 0.25f;
                _targetLightColor = new Color(0.8f, 0.2f, 0.2f, 1f);
                break;
        }
    }

    void Update()
    {
        float lerpFactor = (_targetAmbience == MenuAmbience.Void) ? 2.5f : 3.75f;
        float dt = Time.deltaTime;

        _currentOrbitSpeed = Mathf.Lerp(_currentOrbitSpeed, _targetOrbitSpeed, lerpFactor * dt);
        _currentOrbitRadius = Mathf.Lerp(_currentOrbitRadius, _targetOrbitRadius, lerpFactor * dt);
        _currentFogDensity = Mathf.Lerp(_currentFogDensity, _targetFogDensity, lerpFactor * dt);
        _currentFogColor = Color.Lerp(_currentFogColor, _targetFogColor, lerpFactor * dt);
        _currentAmbientLight = Color.Lerp(_currentAmbientLight, _targetAmbientLight, lerpFactor * dt);
        _currentArchColor = Color.Lerp(_currentArchColor, _targetArchColor, lerpFactor * dt);
        _currentEchoColor = Color.Lerp(_currentEchoColor, _targetEchoColor, lerpFactor * dt);
        _currentEchoSpeed = Mathf.Lerp(_currentEchoSpeed, _targetEchoSpeed, lerpFactor * dt);

        // Directional Light interpolation
        _currentLightIntensity = Mathf.Lerp(_currentLightIntensity, _targetLightIntensity, lerpFactor * dt);
        _currentLightColor = Color.Lerp(_currentLightColor, _targetLightColor, lerpFactor * dt);
        if (_dirLight != null)
        {
            _dirLight.intensity = _currentLightIntensity;
            _dirLight.color = _currentLightColor;
        }

        // Camera rig interpolation
        _currentCameraPitch = Mathf.Lerp(_currentCameraPitch, _targetCameraPitch, lerpFactor * dt);
        _currentCameraYOffset = Mathf.Lerp(_currentCameraYOffset, _targetCameraYOffset, lerpFactor * dt);

        // Container scale interpolation
        _currentVoidScale = Mathf.Lerp(_currentVoidScale, _targetVoidScale, lerpFactor * dt);
        _currentStabilityScale = Mathf.Lerp(_currentStabilityScale, _targetStabilityScale, lerpFactor * dt);
        _currentSystemScale = Mathf.Lerp(_currentSystemScale, _targetSystemScale, lerpFactor * dt);
        _currentDisconnectScale = Mathf.Lerp(_currentDisconnectScale, _targetDisconnectScale, lerpFactor * dt);

        // Update scales
        if (_voidRoot != null) _voidRoot.localScale = Vector3.one * _currentVoidScale;
        if (_stabilityRoot != null) _stabilityRoot.localScale = Vector3.one * _currentStabilityScale;
        if (_systemRoot != null) _systemRoot.localScale = Vector3.one * _currentSystemScale;
        if (_disconnectRoot != null) _disconnectRoot.localScale = Vector3.one * _currentDisconnectScale;

        // Active scene animations
        if (_voidRoot != null && _currentVoidScale > 0.01f)
        {
            // Spin the vortex
            _voidRoot.Rotate(Vector3.up, 24f * dt * _currentEchoSpeed);

            // Pulse center light intensity to create a breathing void effect
            if (_vortexLight != null)
            {
                _vortexLight.intensity = (4.5f + Mathf.Sin(Time.time * 4.5f) * 1.5f) * _currentVoidScale;
                _vortexLight.range = 15f + Mathf.Cos(Time.time * 2.5f) * 3f;
            }
        }
        if (_stabilityRoot != null && _currentStabilityScale > 0.01f)
        {
            // Slow rotation of the grid
            _stabilityRoot.Rotate(Vector3.up, 6f * dt);
        }
        if (_systemRoot != null && _currentSystemScale > 0.01f)
        {
            // Slow rotation of the crystals and circuit board
            _systemRoot.Rotate(Vector3.up, -8f * dt);
        }
        if (_disconnectRoot != null && _currentDisconnectScale > 0.01f)
        {
            // Drifting blocks
            _disconnectRoot.Rotate(Vector3.up, 2f * dt);
        }

        // Update Camera Rig rotation
        if (_cameraPivot != null)
        {
            _orbitAngle += _currentOrbitSpeed * dt;
            _cameraPivot.rotation = Quaternion.Euler(_currentCameraPitch, _orbitAngle, 0f);
        }

        // Update Camera Rig local zoom
        Camera main = Camera.main;
        if (main != null && main.transform.parent == _cameraPivot)
        {
            main.transform.localPosition = new Vector3(0f, _currentCameraYOffset, -_currentOrbitRadius);
        }

        // Update Materials
        if (_archMat != null)
        {
            _archMat.color = _currentArchColor;
        }
        if (_echoMat != null)
        {
            _echoMat.color = _currentEchoColor;
            _echoMat.SetColor("_EmissionColor", _currentEchoColor * 1.5f);
        }

        // Update Fog & Ambient lighting
        float fogPulse = Mathf.Sin(Time.time * fogPulseSpeed) * (_currentFogDensity * 0.15f);
        RenderSettings.fogDensity = _currentFogDensity + fogPulse;
        RenderSettings.fogColor = _currentFogColor;
        RenderSettings.ambientLight = _currentAmbientLight;
    }

    void BuildMaterials()
    {
        Shader shader = Shader.Find("Standard");
        _archMat = new Material(shader);
        _archMat.color = new Color(0.22f, 0.26f, 0.34f, 1f);
        _archMat.SetFloat("_Metallic", 0.3f);
        _archMat.SetFloat("_Glossiness", 0.45f);

        _echoMat = new Material(shader);
        _echoMat.color = new Color(0.35f, 0.85f, 1f, 0.55f);
        _echoMat.SetFloat("_Mode", 3f);
        _echoMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        _echoMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        _echoMat.SetInt("_ZWrite", 0);
        _echoMat.EnableKeyword("_ALPHABLEND_ON");
        _echoMat.EnableKeyword("_EMISSION");
        _echoMat.SetColor("_EmissionColor", new Color(0.35f, 0.85f, 1f) * 1.5f);
        _echoMat.renderQueue = 3000;

        _circuitMat = new Material(shader);
        _circuitMat.color = new Color(0.02f, 0.15f, 0.12f, 1f);
        _circuitMat.SetFloat("_Metallic", 0.8f);
        _circuitMat.SetFloat("_Glossiness", 0.7f);
        _circuitMat.EnableKeyword("_EMISSION");
        _circuitMat.SetColor("_EmissionColor", new Color(0.08f, 0.45f, 0.35f) * 0.9f);

        _packetMat = new Material(shader);
        _packetMat.color = new Color(0.25f, 0.9f, 0.7f, 1f);
        _packetMat.EnableKeyword("_EMISSION");
        _packetMat.SetColor("_EmissionColor", new Color(0.25f, 0.9f, 0.7f) * 3f);
    }

    void BuildVoid()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(0.04f, 0.06f, 0.1f, 1f);
        RenderSettings.ambientLight = new Color(0.18f, 0.22f, 0.3f, 1f);

        GameObject lightObj = new GameObject("MenuMoon");
        _dirLight = lightObj.AddComponent<Light>();
        _dirLight.type = LightType.Directional;
        _dirLight.color = new Color(0.55f, 0.7f, 0.95f, 1f);
        _dirLight.intensity = 0.65f;
        _dirLight.shadows = LightShadows.Soft;
        lightObj.transform.rotation = Quaternion.Euler(28f, -40f, 0f);
    }

    // --- Procedural 3D Environment Builders ---

    void BuildVoidVortex()
    {
        _voidRoot = new GameObject("VoidVortexContainer").transform;
        _voidRoot.SetParent(transform);

        // 1. Inner core: glowing, fast swirling emissive particles
        int innerCount = 50;
        for (int i = 0; i < innerCount; i++)
        {
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Cube);
            p.transform.SetParent(_voidRoot);

            float t = (float)i / innerCount;
            float angle = t * Mathf.PI * 8f;
            float radius = 0.5f + t * 4f;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(t * Mathf.PI * 2f) * 1f - 0.5f, Mathf.Sin(angle) * radius);

            p.transform.position = pos;
            p.transform.localScale = Vector3.one * Random.Range(0.15f, 0.35f);
            p.transform.rotation = Random.rotation;

            p.GetComponent<Renderer>().sharedMaterial = _echoMat;
            p.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Destroy(p.GetComponent<Collider>());
        }

        // 2. Outer ring: dark, shadow-casting tendril blocks
        int outerCount = 45;
        for (int i = 0; i < outerCount; i++)
        {
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Cube);
            p.transform.SetParent(_voidRoot);

            float t = (float)i / outerCount;
            float angle = t * Mathf.PI * 6f + Mathf.PI;
            float radius = 3.5f + t * 11f;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, Random.Range(-1.6f, 1.6f), Mathf.Sin(angle) * radius);

            p.transform.position = pos;
            p.transform.localScale = new Vector3(Random.Range(0.25f, 0.65f), Random.Range(1.8f, 3.8f), Random.Range(0.25f, 0.65f));
            p.transform.rotation = Quaternion.LookRotation(pos) * Quaternion.Euler(Random.Range(-15f, 15f), Random.Range(-15f, 15f), Random.Range(-15f, 15f));

            p.GetComponent<Renderer>().sharedMaterial = _archMat;
            p.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            p.GetComponent<Renderer>().receiveShadows = true;
            Destroy(p.GetComponent<Collider>());
        }

        // 3. Central pulsing light
        GameObject centerLight = new GameObject("VortexCenterLight");
        centerLight.transform.SetParent(_voidRoot);
        centerLight.transform.position = new Vector3(0f, -0.5f, 0f);
        _vortexLight = centerLight.AddComponent<Light>();
        _vortexLight.type = LightType.Point;
        _vortexLight.color = new Color(0.35f, 0.85f, 1f);
        _vortexLight.intensity = 4.5f;
        _vortexLight.range = 18f;
        _vortexLight.shadows = LightShadows.Soft;
    }

    void BuildStabilityGrid()
    {
        _stabilityRoot = new GameObject("StabilityGridContainer").transform;
        _stabilityRoot.SetParent(transform);

        int nodeCount = 10;
        Vector3[] nodePositions = new Vector3[nodeCount];

        // Place nodes in a circuit/mental-map layout
        for (int i = 0; i < nodeCount; i++)
        {
            float angle = i * (360f / nodeCount) * Mathf.Deg2Rad;
            float radius = 4.5f + Mathf.Sin(i * 2f) * 1.5f;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, Random.Range(-0.8f, 0.8f), Mathf.Sin(angle) * radius);
            nodePositions[i] = pos;

            GameObject node = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            node.transform.SetParent(_stabilityRoot);
            node.transform.position = pos;
            node.transform.localScale = Vector3.one * 0.55f;
            node.GetComponent<Renderer>().sharedMaterial = _echoMat;
            Destroy(node.GetComponent<Collider>());
        }

        // Connect nodes with line-like cylinders
        for (int i = 0; i < nodeCount; i++)
        {
            int nextIndex = (i + 1) % nodeCount;
            CreateConnection(nodePositions[i], nodePositions[nextIndex], _stabilityRoot);

            if (i % 3 == 0)
            {
                int otherIndex = (i + 4) % nodeCount;
                CreateConnection(nodePositions[i], nodePositions[otherIndex], _stabilityRoot);
            }
        }
    }

    void CreateConnection(Vector3 p1, Vector3 p2, Transform parent)
    {
        GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        line.transform.SetParent(parent);

        Vector3 dir = p2 - p1;
        float distance = dir.magnitude;
        line.transform.position = p1 + dir * 0.5f;
        line.transform.up = dir.normalized;
        line.transform.localScale = new Vector3(0.06f, distance * 0.5f, 0.06f);

        line.GetComponent<Renderer>().sharedMaterial = _archMat;
        Destroy(line.GetComponent<Collider>());
    }

    void BuildSystemCalibration()
    {
        _systemRoot = new GameObject("SystemCalibrationContainer").transform;
        _systemRoot.SetParent(transform);

        // Spawn central composite crystals (glowing nodes + shadows)
        int crystalCount = 6;
        for (int i = 0; i < crystalCount; i++)
        {
            GameObject crystalGroup = new GameObject($"CrystalCluster_{i}");
            crystalGroup.transform.SetParent(_systemRoot);

            float angle = i * (360f / crystalCount) * Mathf.Deg2Rad;
            float radius = Random.Range(0.2f, 0.6f);
            Vector3 centerPos = new Vector3(Mathf.Cos(angle) * radius, -0.5f, Mathf.Sin(angle) * radius);
            crystalGroup.transform.position = centerPos;

            // Make a cluster of 3 crystal pillars per group
            int shards = 3;
            for (int s = 0; s < shards; s++)
            {
                GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shard.transform.SetParent(crystalGroup.transform);

                float shardHeight = Random.Range(1.2f, 2.6f);
                shard.transform.localPosition = new Vector3(Random.Range(-0.15f, 0.15f), shardHeight * 0.5f - 0.5f, Random.Range(-0.15f, 0.15f));

                // Rotated to look like quartz/crystal facets
                shard.transform.localRotation = Quaternion.Euler(Random.Range(-12f, 12f), Random.Range(0f, 360f), Random.Range(-12f, 12f));
                shard.transform.localScale = new Vector3(0.2f, shardHeight, 0.2f);

                shard.GetComponent<Renderer>().sharedMaterial = _echoMat;
                shard.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                shard.GetComponent<Renderer>().receiveShadows = true;
                Destroy(shard.GetComponent<Collider>());
            }
        }

        // Circuit board floor tracks & packets
        int lineCount = 12;
        for (int i = 0; i < lineCount; i++)
        {
            float angle = i * (360f / lineCount) * Mathf.Deg2Rad;
            float length = Random.Range(4f, 8f);
            float width = 0.08f;

            GameObject track = GameObject.CreatePrimitive(PrimitiveType.Cube);
            track.transform.SetParent(_systemRoot);

            Vector3 startPos = new Vector3(Mathf.Cos(angle) * 0.8f, -0.98f, Mathf.Sin(angle) * 0.8f);
            Vector3 endPos = new Vector3(Mathf.Cos(angle) * (length + 0.8f), -0.98f, Mathf.Sin(angle) * (length + 0.8f));

            track.transform.position = startPos + (endPos - startPos) * 0.5f;
            track.transform.rotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg, 0f);
            track.transform.localScale = new Vector3(width, 0.01f, length);

            track.GetComponent<Renderer>().sharedMaterial = _circuitMat;
            Destroy(track.GetComponent<Collider>());

            // Spawn a packet traveler on this track
            GameObject packet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            packet.transform.SetParent(_systemRoot);
            packet.transform.localScale = Vector3.one * 0.15f;
            packet.GetComponent<Renderer>().sharedMaterial = _packetMat;
            Destroy(packet.GetComponent<Collider>());

            CircuitPacketMover mover = packet.AddComponent<CircuitPacketMover>();
            mover.start = startPos;
            mover.end = endPos;
            mover.speed = Random.Range(0.4f, 0.8f);
            mover.delay = Random.Range(0f, 1.5f);
        }

        // Spotlight pointing up on the crystals
        GameObject spotLight = new GameObject("CrystalSpotLight");
        spotLight.transform.SetParent(_systemRoot);
        spotLight.transform.position = new Vector3(0f, -0.9f, 0f);
        spotLight.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        Light l = spotLight.AddComponent<Light>();
        l.type = LightType.Spot;
        l.color = new Color(0.25f, 0.9f, 0.7f);
        l.intensity = 6f;
        l.range = 10f;
        l.spotAngle = 55f;
        l.shadows = LightShadows.Soft;

        // Fill Light pointing from the side to create beautiful dual-tone shadows and specular highlights
        GameObject fillLightObj = new GameObject("CrystalFillLight");
        fillLightObj.transform.SetParent(_systemRoot);
        fillLightObj.transform.position = new Vector3(3f, 2f, 3f);
        Light fillLight = fillLightObj.AddComponent<Light>();
        fillLight.type = LightType.Spot;
        fillLight.color = new Color(0.1f, 0.5f, 0.9f); // Blue fill
        fillLight.intensity = 4f;
        fillLight.range = 12f;
        fillLight.spotAngle = 50f;
        fillLight.shadows = LightShadows.Soft;
        fillLightObj.transform.LookAt(Vector3.up * 0.5f);
    }

    void BuildDisconnect()
    {
        _disconnectRoot = new GameObject("DisconnectContainer").transform;
        _disconnectRoot.SetParent(transform);

        // Fractured alert pieces drifting apart
        int pieces = 14;
        for (int i = 0; i < pieces; i++)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.transform.SetParent(_disconnectRoot);

            float angle = i * (360f / pieces) * Mathf.Deg2Rad;
            float radius = Random.Range(3.5f, 9f);
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, Random.Range(-1.8f, 1.8f), Mathf.Sin(angle) * radius);
            block.transform.position = pos;
            block.transform.rotation = Random.rotation;
            block.transform.localScale = new Vector3(Random.Range(0.4f, 1.8f), Random.Range(0.4f, 1.8f), Random.Range(0.4f, 1.8f));

            block.GetComponent<Renderer>().sharedMaterial = _archMat;
            block.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            block.GetComponent<Renderer>().receiveShadows = true;
            Destroy(block.GetComponent<Collider>());
        }

        // Crimson pulsing light
        GameObject alarmLight = new GameObject("AlarmLight");
        alarmLight.transform.SetParent(_disconnectRoot);
        alarmLight.transform.position = Vector3.zero;
        Light l = alarmLight.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(0.9f, 0.15f, 0.15f);
        l.intensity = 3.5f;
        l.range = 18f;
    }

    void BuildDistantEchoes()
    {
        Transform echoRoot = new GameObject("DistantEchoes").transform;
        echoRoot.SetParent(transform);
        GameObject echoModelSource = Resources.Load<GameObject>("EchoesCharacterVisual");

        for (int i = 0; i < distantEchoWalkers; i++)
        {
            GameObject echo = echoModelSource != null
                ? Instantiate(echoModelSource, echoRoot)
                : new GameObject("DistantEchoModelMissing");
            echo.name = $"DistantEcho_{i}";
            echo.transform.position = new Vector3(Random.Range(-30f, 30f), Random.Range(-2f, 4f), Random.Range(15f, 40f));
            echo.transform.localScale = Vector3.one * 0.85f;
            ApplyDistantEchoMaterial(echo);
            echo.AddComponent<MenuEchoWalker>();
        }
    }

    void ApplyDistantEchoMaterial(GameObject echo)
    {
        if (echo == null)
            return;

        foreach (Collider col in echo.GetComponentsInChildren<Collider>(true))
            Destroy(col);

        foreach (Renderer rendererRef in echo.GetComponentsInChildren<Renderer>(true))
            rendererRef.sharedMaterial = _echoMat;
    }

    void BuildCameraRig()
    {
        Camera main = Camera.main;
        if (main == null)
            return;

        _cameraPivot = new GameObject("MenuCameraOrbit").transform;
        _cameraPivot.position = Vector3.zero;
        main.transform.SetParent(_cameraPivot);
        main.transform.localPosition = new Vector3(0f, _currentCameraYOffset, -orbitRadius);
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

    class CircuitPacketMover : MonoBehaviour
    {
        public Vector3 start;
        public Vector3 end;
        public float speed;
        public float delay;
        private float _t;
        private float _delayTimer;

        void Start()
        {
            _delayTimer = delay;
            transform.position = start;
        }

        void Update()
        {
            if (_delayTimer > 0f)
            {
                _delayTimer -= Time.deltaTime;
                return;
            }

            _t += Time.deltaTime * speed;
            if (_t >= 1f)
            {
                _t = 0f;
                _delayTimer = delay; // reset delay
            }
            transform.position = Vector3.Lerp(start, end, _t);
        }
    }
}
