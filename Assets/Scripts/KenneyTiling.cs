using UnityEngine;

[ExecuteInEditMode]
public class KenneyTiling : MonoBehaviour
{
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
            return;

        Material shared = mr.sharedMaterial;

        bool hasMainTex = shared.HasProperty("_MainTex") && shared.GetTexture("_MainTex") != null;
        bool hasBaseMap = shared.HasProperty("_BaseMap") && shared.GetTexture("_BaseMap") != null;

        if (!hasMainTex && !hasBaseMap)
            return;

        // Creamos una instancia local del material en tiempo de ejecucion/editor
        // para no afectar al material base compartido por todos
        Material localMat = new Material(shared);

        // Calculamos el tiling basado en la escala (1 unidad de Unity = 1 tile de Kenney)
        Vector2 tiling = new Vector2(transform.lossyScale.x, transform.lossyScale.z);

        // Si es una pared (escala en Y grande y Z/X pequeno), ajustamos el tiling
        if (transform.lossyScale.y > transform.lossyScale.x && transform.lossyScale.y > transform.lossyScale.z)
        {
            tiling = new Vector2(Mathf.Max(transform.lossyScale.x, transform.lossyScale.z), transform.lossyScale.y);
        }

        if (hasMainTex)
        {
            localMat.SetTextureScale("_MainTex", tiling);
        }
        if (hasBaseMap)
        {
            localMat.SetTextureScale("_BaseMap", tiling);
        }

        mr.material = localMat;
    }
}
