using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Pasillo escolar detrás del menú principal, iluminado según la sección activa.</summary>
public class MainMenuCinematicWorld : MonoBehaviour
{
    public enum MenuAmbience { Void, Stability, System, Disconnect }
    public enum SchoolProp { Door, Notebook, Lockers, Clock, Notice, Projector, Exit }

    readonly Dictionary<SchoolProp, (Transform moving, Vector3 offset, Vector3 rotation, Material ink)> _props = new();
    readonly Dictionary<SchoolProp, float> _propAmounts = new();
    SchoolProp? _hoverProp;
    static readonly int SchoolSpot = Shader.PropertyToID("_EchoesSchoolSpot");
    Vector3 _spotPosition;
    float _spotAmount;

    public static MainMenuCinematicWorld Instance { get; private set; }

    readonly List<Material> _materials = new();
    Camera _camera;
    float _cameraTime;
    Color _lightColor = new(1f, 0.91f, 0.72f);

    const float SecondsBetweenViews = 12f;
    static readonly Pose[] CameraViews =
    {
        new(new Vector3(-1.45f, 1.52f, -4.2f), Quaternion.Euler(2.5f, 4.5f, 0f)),
        new(new Vector3(0.1f, 1.73f, -1.4f), Quaternion.Euler(-4f, 9f, 0f)),
        new(new Vector3(1.7f, 1.25f, 1.15f), Quaternion.Euler(2f, -9f, 0f)),
        new(new Vector3(0.35f, 1.43f, -1.8f), Quaternion.Euler(1f, -3.5f, 0f)),
    };
    Color _targetFog = new(0.023f, 0.032f, 0.028f);
    Color _targetLight = new(1f, 0.91f, 0.72f);

    void Awake() => Instance = this;

    void Start()
    {
        _camera = Camera.main;
        if (_camera == null) return;

        Pose initial = CameraViews[0];
        _camera.transform.SetPositionAndRotation(initial.position, initial.rotation);
        _camera.fieldOfView = 53f;
        _camera.clearFlags = CameraClearFlags.SolidColor;
        _camera.backgroundColor = _targetFog;

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.025f, 0.03f, 0.028f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.046f;
        RenderSettings.fogColor = _targetFog;

        foreach (var root in gameObject.scene.GetRootGameObjects())
            foreach (var light in root.GetComponentsInChildren<Light>())
                light.enabled = false;

        BuildHallway();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            Shader.SetGlobalVector(SchoolSpot, Vector4.zero);
        }
        foreach (var material in _materials) Destroy(material);
    }

    public void PreviewProp(SchoolProp? prop) => _hoverProp = prop;

    public void SetAmbience(MenuAmbience ambience)
    {
        (_targetFog, _targetLight) = ambience switch
        {
            MenuAmbience.Stability => (new Color(0.027f, 0.035f, 0.029f), new Color(0.90f, 0.95f, 0.81f)),
            MenuAmbience.System => (new Color(0.021f, 0.028f, 0.031f), new Color(0.79f, 0.90f, 0.89f)),
            MenuAmbience.Disconnect => (new Color(0.036f, 0.024f, 0.022f), new Color(0.97f, 0.75f, 0.63f)),
            _ => (new Color(0.023f, 0.032f, 0.028f), new Color(1f, 0.91f, 0.72f)),
        };
    }

    void LateUpdate()
    {
        if (_camera == null) return;

        bool reduceMotion = EchoesSettings.ReduceMotion;
        var highlighted = default((Transform moving, Vector3 offset, Vector3 rotation, Material ink));
        bool highlight = _hoverProp.HasValue && _props.TryGetValue(_hoverProp.Value, out highlighted);
        if (!reduceMotion)
        {
            if (!highlight)
                _cameraTime = Mathf.Repeat(_cameraTime + Time.unscaledDeltaTime, SecondsBetweenViews * CameraViews.Length);

            Pose view = highlight ? GetPropView(_hoverProp.Value, highlighted.moving.parent.position) : GetHallwayView();
            float delta = Time.unscaledDeltaTime;
            float cameraBlend = 1f - Mathf.Exp(-3.9f * delta);
            Vector3 currentPosition = _camera.transform.position;
            float step = Mathf.Min(Vector3.Distance(currentPosition, view.position) * cameraBlend, 8.5f * delta);
            _camera.transform.SetPositionAndRotation(
                Vector3.MoveTowards(currentPosition, view.position, step),
                Quaternion.Slerp(_camera.transform.rotation, view.rotation, cameraBlend));
        }

        float blend = reduceMotion ? 1f : 1f - Mathf.Exp(-5f * Time.unscaledDeltaTime);
        if (highlight)
        {
            Vector3 propPosition = highlighted.moving.parent.position;
            Vector3 targetSpot = propPosition + new Vector3(-0.65f, 0.55f, -0.45f);
            _spotPosition = _spotAmount <= 0.01f ? targetSpot : Vector3.Lerp(_spotPosition, targetSpot, blend);
        }
        _spotAmount = Mathf.Lerp(_spotAmount, highlight ? 1f : 0f, blend);
        Shader.SetGlobalVector(SchoolSpot, new Vector4(_spotPosition.x, _spotPosition.y, _spotPosition.z, _spotAmount));
        foreach (var (prop, detail) in _props)
        {
            float amount = _propAmounts[prop];
            float target = _hoverProp == prop ? 1f : 0f;
            if (Mathf.Approximately(amount, target)) continue;

            amount = Mathf.Lerp(amount, target, blend);
            _propAmounts[prop] = amount;
            float movement = reduceMotion ? 0f : amount;
            detail.moving.localPosition = detail.offset * movement;
            detail.moving.localRotation = Quaternion.Euler(detail.rotation * movement);
            detail.ink.SetColor("_EmissionColor", new Color(0.61f, 0.45f, 0.24f) * Mathf.Lerp(0.045f, 0.68f, amount));
        }
        RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, _targetFog, blend);
        _camera.backgroundColor = RenderSettings.fogColor;

        if (_lightColor == _targetLight) return;
        _lightColor = Color.Lerp(_lightColor, _targetLight, blend);
        foreach (var material in _materials)
            if (material.HasProperty("_HallwayLightColor"))
                material.SetColor("_HallwayLightColor", _lightColor);
    }

    Pose GetHallwayView()
    {
        float segment = _cameraTime / SecondsBetweenViews;
        int index = Mathf.Min(Mathf.FloorToInt(segment), CameraViews.Length - 1);
        Pose from = CameraViews[index];
        Pose to = CameraViews[(index + 1) % CameraViews.Length];
        float blend = Mathf.SmoothStep(0f, 1f, segment - index);
        return new Pose(
            Vector3.Lerp(from.position, to.position, blend),
            Quaternion.Slerp(from.rotation, to.rotation, blend));
    }

    Pose GetPropView(SchoolProp prop, Vector3 subject)
    {
        // Las tomas se quedan en el centro del pasillo, fuera de los muebles.
        Vector3 offset = prop switch
        {
            SchoolProp.Notebook => new(-2.35f, 0.90f, -2.55f),
            SchoolProp.Clock => new(-2.75f, -0.56f, -2.95f),
            SchoolProp.Lockers => new(-2.60f, 0.48f, -3.40f),
            SchoolProp.Exit => new(-1.40f, -0.75f, -3.35f),
            _ => new(-2.70f, 0.02f, -3.05f),
        };

        Vector3 position = subject + offset;
        Quaternion aim = Quaternion.LookRotation(subject - position, Vector3.up);
        // Deja el objeto a la derecha de la columna de navegación.
        float rightOfCenter = Mathf.Atan(0.30f * Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * _camera.aspect) * Mathf.Rad2Deg;
        return new Pose(position, aim * Quaternion.Euler(0f, -rightOfCenter, 0f));
    }

    void BuildHallway()
    {
        Shader shader = Shader.Find("Echoes/PS1World") ?? Shader.Find(EchoesUrpMaterials.LitShaderName);
        Material plaster = Surface(shader, new Color(0.59f, 0.55f, 0.43f));
        Material tile = Surface(shader, new Color(0.70f, 0.61f, 0.46f));
        Material tileDark = Surface(shader, new Color(0.53f, 0.46f, 0.35f));
        Material wainscot = Surface(shader, new Color(0.31f, 0.42f, 0.32f));
        Material wood = Surface(shader, new Color(0.26f, 0.20f, 0.15f));
        Material paint = Surface(shader, new Color(0.43f, 0.34f, 0.23f));
        Material glass = Surface(shader, new Color(0.19f, 0.24f, 0.22f));
        Material paper = Surface(shader, new Color(0.85f, 0.76f, 0.57f));
        Material tube = Surface(shader, new Color(0.95f, 0.90f, 0.70f), emission: true);

        Block("Techo", new Vector3(0, 3.6f, 11), new Vector3(7.5f, 0.24f, 35), plaster);
        Block("Pared del fondo", new Vector3(0, 1.8f, 28), new Vector3(7.5f, 3.6f, 0.25f), plaster);
        Block("Base del piso", new Vector3(0, -0.10f, 11), new Vector3(7.5f, 0.16f, 35), tileDark);

        for (int side = -1; side <= 1; side += 2)
        {
            float wallX = side * 3.7f;
            Block("Pared", new Vector3(wallX, 1.8f, 11), new Vector3(0.22f, 3.6f, 35), plaster);
            Block("Zócalo verde", new Vector3(side * 3.56f, 0.58f, 11), new Vector3(0.08f, 1.16f, 35), wainscot);
            Block("Listón", new Vector3(side * 3.5f, 1.17f, 11), new Vector3(0.08f, 0.09f, 35), wood);

            for (int room = 0; room < 4; room++)
            {
                float z = 1f + room * 6.5f + (side < 0 ? 1.6f : 0f);
                Block("Marco de aula", new Vector3(side * 3.45f, 1.20f, z), new Vector3(0.19f, 2.40f, 1.53f), wood);
                Block("Puerta de aula", new Vector3(side * 3.32f, 1.12f, z), new Vector3(0.12f, 2.24f, 1.31f), paint);
                Block("Visor de aula", new Vector3(side * 3.24f, 1.67f, z), new Vector3(0.05f, 0.55f, 0.43f), glass);
                Block("Manija", new Vector3(side * 3.19f, 1.0f, z - 0.47f), new Vector3(0.08f, 0.07f, 0.14f), paper);
                Block("Placa de aula", new Vector3(side * 3.53f, 2.24f, z + 1.02f), new Vector3(0.08f, 0.18f, 0.32f), paper);
            }
        }

        BuildSchoolProps(shader, wood, paint, glass, paper, wainscot);

        for (int row = 0; row < 19; row++)
        {
            float z = -4.55f + row * 1.75f;
            for (int column = 0; column < 5; column++)
                Block("Baldosa", new Vector3(-2.88f + column * 1.44f, -0.03f, z),
                    new Vector3(1.43f, 0.05f, 1.74f), (row + column) % 2 == 0 ? tile : tileDark);
        }

        for (int i = 0; i < 6; i++)
        {
            float z = -2f + i * 5.2f;
            Block("Carcasa fluorescente", new Vector3(0, 3.45f, z), new Vector3(0.62f, 0.16f, 1.8f), glass);
            Block("Tubo fluorescente", new Vector3(0, 3.34f, z), new Vector3(0.37f, 0.07f, 1.58f), tube);
        }

        Block("Salida", new Vector3(0, 1.33f, 27.78f), new Vector3(1.85f, 2.63f, 0.24f), wood);
        Block("Mirilla de salida", new Vector3(0, 1.84f, 27.62f), new Vector3(0.84f, 0.48f, 0.10f), glass);
        Block("Luz de salida", new Vector3(0, 2.92f, 27.62f), new Vector3(0.65f, 0.14f, 0.1f), tube);
    }

    void BuildSchoolProps(Shader shader, Material wood, Material paint, Material glass, Material paper, Material green)
    {
        // La pared derecha reúne los objetos del menú a lo largo del recorrido de cámara.
        var door = MakeProp(SchoolProp.Door, new(3.12f, 1.63f, 5.9f), shader, Vector3.zero, new(0f, -19f, 0f));
        Block("Placa de aula", new(3.36f, 2.1f, 5.9f), new(0.12f, 0.23f, 0.38f), paper);
        Block("Aula · ventanilla", new(0f, 0f, 0.22f), new(0.09f, 0.48f, 0.44f), Ink(SchoolProp.Door), door);
        Block("Marco de la ventanilla", new(3.22f, 1.63f, 6.12f), new(0.08f, 0.58f, 0.54f), wood);

        Block("Mesa escolar", new(2.96f, 0.84f, 3.8f), new(0.87f, 0.13f, 1.3f), paint);
        Block("Pata de pupitre", new(2.65f, 0.42f, 3.35f), new(0.1f, 0.78f, 0.1f), green);
        Block("Pata de pupitre", new(2.65f, 0.42f, 4.23f), new(0.1f, 0.78f, 0.1f), green);
        Block("Asiento", new(2.10f, 0.43f, 4.0f), new(0.47f, 0.10f, 0.5f), wood);
        Block("Respaldo", new(1.89f, 0.66f, 4.0f), new(0.09f, 0.43f, 0.5f), paint);
        Block("Soporte de silla", new(2.1f, 0.21f, 4.0f), new(0.13f, 0.42f, 0.13f), green);
        var book = MakeProp(SchoolProp.Notebook, new(2.85f, 0.94f, 3.8f), shader, new(-0.14f, 0.08f, 0f), new(0f, 0f, -22f));
        Block("Cuaderno", Vector3.zero, new(0.54f, 0.045f, 0.40f), Ink(SchoolProp.Notebook), book);
        Block("Lomo", new(0.23f, 0.024f, 0f), new(0.045f, 0.03f, 0.40f), wood, book);

        for (int i = 0; i < 3; i++)
        {
            float z = 7.8f + i * 0.68f;
            Block("Casillero", new(3.28f, 0.98f, z), new(0.50f, 1.96f, 0.64f), green);
            for (int grille = 0; grille < 3; grille++)
                Block("Ranura", new(3.018f, 1.63f + grille * 0.066f, z), new(0.03f, 0.02f, 0.38f), glass);
        }
        var locker = MakeProp(SchoolProp.Lockers, new(3.0f, 1.01f, 8.75f), shader, Vector3.zero, new(0f, -31f, 0f));
        Block("Puerta de casillero", new(-0.02f, 0f, -0.27f), new(0.055f, 1.82f, 0.57f), green, locker);
        Block("Etiqueta", new(-0.06f, 0.43f, -0.33f), new(0.03f, 0.12f, 0.27f), Ink(SchoolProp.Lockers), locker);

        var clock = MakeProp(SchoolProp.Clock, new(3.31f, 2.52f, 5.05f), shader, Vector3.zero, new(65f, 0f, 0f));
        Block("Reloj escolar", new(3.39f, 2.52f, 5.05f), new(0.13f, 0.52f, 0.52f), wood);
        Block("Esfera", new(3.312f, 2.52f, 5.05f), new(0.03f, 0.44f, 0.44f), Ink(SchoolProp.Clock));
        Block("Aguja", new(-0.027f, 0.11f, 0f), new(0.03f, 0.22f, 0.022f), glass, clock);
        Block("Aguja corta", new(3.277f, 2.52f, 5.12f), new(0.024f, 0.026f, 0.14f), glass);

        Block("Tablón de anuncios", new(3.49f, 1.9f, 3.1f), new(0.20f, 1.10f, 2.1f), wood);
        Block("Hoja clavada", new(3.37f, 1.76f, 3.42f), new(0.04f, 0.38f, 0.33f), paper);
        var notice = MakeProp(SchoolProp.Notice, new(3.34f, 2.01f, 2.88f), shader, new(-0.12f, 0.08f, 0f), new(8f, -12f, 0f));
        Block("Aviso del tablón", Vector3.zero, new(0.04f, 0.55f, 0.40f), Ink(SchoolProp.Notice), notice);
        for (int i = 0; i < 3; i++)
            Block("Renglón", new(-0.028f, 0.11f - i * 0.075f, 0f), new(0.01f, 0.013f, 0.27f), wood, notice);

        Block("Estante alto", new(3.25f, 1.18f, 10.23f), new(0.58f, 0.09f, 1.2f), wood);
        Block("Proyector escolar", new(3.15f, 1.36f, 10.22f), new(0.62f, 0.31f, 0.51f), glass);
        var projector = MakeProp(SchoolProp.Projector, new(2.81f, 1.37f, 10.22f), shader, new(-0.13f, 0f, 0f), Vector3.zero);
        Block("Lente del proyector", Vector3.zero, new(0.11f, 0.16f, 0.22f), Ink(SchoolProp.Projector), projector);

        Block("Soporte de salida", new(1.55f, 3.03f, 13.4f), new(0.035f, 1.05f, 0.035f), wood);
        Block("Soporte de salida", new(2.05f, 3.03f, 13.4f), new(0.035f, 1.05f, 0.035f), wood);
        var exit = MakeProp(SchoolProp.Exit, new(1.8f, 2.45f, 13.4f), shader, new(0f, 0.08f, 0f), new(0f, 0f, 6f));
        Block("Placa de salida", Vector3.zero, new(0.70f, 0.27f, 0.095f), Ink(SchoolProp.Exit), exit);
        Block("Flecha de salida", new(0f, 0f, -0.056f), new(0.31f, 0.036f, 0.015f), glass, exit);
        Block("Extremo de flecha", new(0.15f, 0f, -0.056f), new(0.035f, 0.12f, 0.015f), glass, exit);
    }

    Transform MakeProp(SchoolProp prop, Vector3 position, Shader shader, Vector3 offset, Vector3 rotation)
    {
        var anchor = new GameObject(prop.ToString()).transform;
        anchor.SetParent(transform, false);
        anchor.localPosition = position;
        var moving = new GameObject("Detalle móvil").transform;
        moving.SetParent(anchor, false);
        Material ink = Surface(shader, new Color(0.58f, 0.51f, 0.35f), emission: true);
        ink.SetColor("_EmissionColor", new Color(0.61f, 0.45f, 0.24f) * 0.045f);
        _props.Add(prop, (moving, offset, rotation, ink));
        _propAmounts.Add(prop, 0f);
        return moving;
    }

    Material Ink(SchoolProp prop) => _props[prop].ink;

    Material Surface(Shader shader, Color color, bool emission = false)
    {
        var material = new Material(shader);
        material.SetColor("_BaseColor", color);
        if (material.HasProperty("_StainStrength"))
        {
            material.SetFloat("_StainStrength", 0.23f);
            material.SetFloat("_FlickerStrength", 0f);
            material.SetFloat("_SnapPixelSize", 1f);
            material.SetFloat("_AmbientFloor", 0.23f);
            material.SetFloat("_FlatAmbient", 0.24f);
            material.SetFloat("_FluorescentHum", 0f);
            material.SetFloat("_CeilingFill", 0f);
            material.SetFloat("_HallwayLightStrength", 1f);
            material.SetColor("_HallwayLightColor", _lightColor);
        }
        if (emission)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 1.5f);
        }
        _materials.Add(material);
        return material;
    }

    void Block(string label, Vector3 position, Vector3 size, Material material, Transform parent = null)
    {
        var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = label;
        block.transform.SetParent(parent != null ? parent : transform, false);
        block.transform.localPosition = position;
        block.transform.localScale = size;
        block.GetComponent<Renderer>().sharedMaterial = material;
        Destroy(block.GetComponent<Collider>());
    }
}
