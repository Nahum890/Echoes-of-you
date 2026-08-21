using UnityEngine;

/// <summary>
/// Escala las UV de un objeto segun su tamano en el mundo, para que una textura
/// de 1 tile cubra 1 unidad de Unity y no se estire a lo largo de toda la pared.
///
/// Los cubos de Unity tienen UV 0..1 por cara, asi que sin esto una pared de 20 m
/// y un locker de 0.4 m muestran la textura al mismo tamano.
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
public class KenneyTiling : MonoBehaviour
{
    // Los shaders Echoes/* usan _BaseTex. Antes solo se miraba _MainTex y
    // _BaseMap, asi que este componente salia por el early-out en TODA la
    // geometria del juego y el tiling por tamano no se aplicaba nunca.
    private static readonly string[] TextureProperties = { "_BaseTex", "_BaseMap", "_MainTex" };

    [Tooltip("Tiles por unidad de mundo. Subelo para que la textura se repita mas.")]
    public float tilesPerUnit = 1f;

    private MaterialPropertyBlock _block;

    void Start()
    {
        UpdateTiling();
    }

#if UNITY_EDITOR
    void Update()
    {
        if (!Application.isPlaying && transform.hasChanged)
        {
            UpdateTiling();
            transform.hasChanged = false;
        }
    }
#endif

    public void UpdateTiling()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr == null || mr.sharedMaterial == null)
        {
            return;
        }

        Material shared = mr.sharedMaterial;
        Vector3 scale = transform.lossyScale;

        // Para una pared (alta y delgada) el eje horizontal util es el mayor de
        // X/Z y el vertical es Y; para un suelo son X y Z.
        Vector2 tiling;
        if (scale.y > scale.x && scale.y > scale.z)
        {
            tiling = new Vector2(Mathf.Max(scale.x, scale.z), scale.y);
        }
        else
        {
            tiling = new Vector2(scale.x, scale.z);
        }

        tiling *= Mathf.Max(0.001f, tilesPerUnit);

        // MaterialPropertyBlock en vez de `mr.material = new Material(shared)`:
        // aquello instanciaba un material nuevo en cada Update del editor y los
        // iba dejando colgados en la escena.
        _block ??= new MaterialPropertyBlock();
        mr.GetPropertyBlock(_block);

        bool applied = false;
        foreach (string prop in TextureProperties)
        {
            if (!shared.HasProperty(prop) || shared.GetTexture(prop) == null)
            {
                continue;
            }

            Vector4 st = shared.GetVector(prop + "_ST");
            _block.SetVector(prop + "_ST", new Vector4(tiling.x, tiling.y, st.z, st.w));
            applied = true;
        }

        if (applied)
        {
            mr.SetPropertyBlock(_block);
        }
    }
}
