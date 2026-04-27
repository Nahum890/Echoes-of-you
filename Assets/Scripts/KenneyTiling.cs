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
        if (mr != null && mr.sharedMaterial != null && mr.sharedMaterial.mainTexture != null)
        {
            // Creamos una instancia local del material en tiempo de ejecucion/editor
            // para no afectar al material base compartido por todos
            Material localMat = new Material(mr.sharedMaterial);
            
            // Calculamos el tiling basado en la escala (1 unidad de Unity = 1 tile de Kenney)
            Vector2 tiling = new Vector2(transform.lossyScale.x, transform.lossyScale.z);
            
            // Si es una pared (escala en Y grande y Z/X pequeno), ajustamos el tiling.
            // Una heuristica simple:
            if (transform.lossyScale.y > transform.lossyScale.x && transform.lossyScale.y > transform.lossyScale.z)
            {
                tiling = new Vector2(Mathf.Max(transform.lossyScale.x, transform.lossyScale.z), transform.lossyScale.y);
            }
            
            localMat.mainTextureScale = tiling;
            mr.material = localMat;
        }
    }
}
