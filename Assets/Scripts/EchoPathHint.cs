using UnityEngine;

/// <summary>
/// Guía visual sutil para indicar rutas donde el eco debería moverse.
/// Genera puntos de luz flotantes entre waypoints para guiar al jugador.
/// </summary>
public class EchoPathHint : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] Vector3[] waypoints;

    [Header("Visual")]
    [SerializeField] Color hintColor = new Color(0.35f, 0.65f, 1f, 0.6f);
    [SerializeField] float particleSize = 0.18f;
    [SerializeField] float glowIntensity = 1.2f;
    [SerializeField] float pulseSpeed = 1.5f;

    Light[] _lights;
    float _phase;

    public void SetWaypoints(Vector3[] points)
    {
        waypoints = points;
    }

    void Start()
    {
        if (waypoints == null || waypoints.Length < 2)
            return;

        _lights = new Light[waypoints.Length];

        for (int i = 0; i < waypoints.Length; i++)
        {
            GameObject hint = new GameObject($"EchoHint_{i}");
            hint.transform.SetParent(transform, false);
            hint.transform.position = waypoints[i] + Vector3.up * 0.5f;

            Light light = hint.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = hintColor;
            light.intensity = glowIntensity;
            light.range = 3f;
            light.shadows = LightShadows.None;
            _lights[i] = light;

            // Pequeña esfera visual
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "HintOrb";
            sphere.transform.SetParent(hint.transform, false);
            sphere.transform.localScale = Vector3.one * particleSize;

            Collider col = sphere.GetComponent<Collider>();
            if (col != null) Destroy(col);

            Renderer rend = sphere.GetComponent<Renderer>();
            if (rend != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = hintColor;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", hintColor * 2f);
                mat.SetFloat("_Mode", 3f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                rend.sharedMaterial = mat;
            }
        }
    }

    void Update()
    {
        if (_lights == null) return;

        _phase += Time.deltaTime * pulseSpeed;
        for (int i = 0; i < _lights.Length; i++)
        {
            if (_lights[i] == null) continue;
            float offset = i * 0.8f;
            float pulse = (Mathf.Sin(_phase + offset) * 0.5f + 0.5f) * 0.4f + 0.6f;
            _lights[i].intensity = glowIntensity * pulse;
            _lights[i].transform.localPosition = new Vector3(0f, Mathf.Sin(_phase * 0.7f + offset) * 0.15f, 0f);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = new Color(0.35f, 0.65f, 1f, 0.7f);
        for (int i = 0; i < waypoints.Length; i++)
        {
            Gizmos.DrawWireSphere(waypoints[i] + Vector3.up * 0.5f, 0.3f);
            if (i > 0)
            {
                Gizmos.DrawLine(
                    waypoints[i - 1] + Vector3.up * 0.5f,
                    waypoints[i] + Vector3.up * 0.5f);
            }
        }
    }
#endif
}
