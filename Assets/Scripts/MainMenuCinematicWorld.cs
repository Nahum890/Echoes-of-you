using UnityEngine;

/// <summary>
/// Escena 3D viva detrás del menú: arquitectura flotante, ecos distantes, niebla y cámara orbital.
/// </summary>
public class MainMenuCinematicWorld : MonoBehaviour
{
    [SerializeField] int floatingPlatforms = 14;
    [SerializeField] int distantEchoWalkers = 5;
    [SerializeField] float orbitRadius = 22f;
    [SerializeField] float orbitSpeed = 4.5f;
    [SerializeField] float fogPulseSpeed = 0.15f;

    Transform _cameraPivot;
    float _orbitAngle;
    Material _archMat;
    Material _echoMat;

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

    void Start()
    {
        BuildMaterials();
        BuildVoid();
        BuildArchitecture();
        BuildDistantEchoes();
        BuildCameraRig();
        ApplyLiminalFog();
    }

    void Update()
    {
        if (_cameraPivot != null)
        {
            _orbitAngle += orbitSpeed * Time.deltaTime;
            _cameraPivot.rotation = Quaternion.Euler(12f, _orbitAngle, 0f);
        }

        float fogPulse = 0.028f + Mathf.Sin(Time.time * fogPulseSpeed) * 0.008f;
        RenderSettings.fogDensity = fogPulse;
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

        for (int i = 0; i < floatingPlatforms; i++)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.transform.SetParent(archRoot);
            float angle = i * (360f / floatingPlatforms) * Mathf.Deg2Rad;
            float radius = 8f + (i % 4) * 2.5f;
            float height = Mathf.Sin(i * 1.7f) * 3f;
            block.transform.position = new Vector3(Mathf.Cos(angle) * radius, height, Mathf.Sin(angle) * radius);
            block.transform.localScale = new Vector3(
                Random.Range(2f, 6f),
                Random.Range(0.3f, 1.2f),
                Random.Range(2f, 5f));
            block.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), Random.Range(-8f, 8f));
            block.GetComponent<Renderer>().sharedMaterial = _archMat;
            Destroy(block.GetComponent<Collider>());

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

        void Start()
        {
            _start = transform.position;
            _phase = Random.value * 10f;
        }

        void Update()
        {
            float t = Time.time + _phase;
            transform.position = _start + new Vector3(Mathf.Sin(t * 0.4f) * 4f, Mathf.Sin(t * 0.7f) * 0.3f, Mathf.Cos(t * 0.35f) * 3f);
        }
    }
}
