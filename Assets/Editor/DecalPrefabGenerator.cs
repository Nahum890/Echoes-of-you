using System.IO;
using UnityEditor;
using UnityEngine;

public static class DecalPrefabGenerator
{
    private const string TargetFolder = "Assets/Prefabs/Decals";

    [MenuItem("Echoes of You/Art/Generate Decal Prefabs")]
    public static void GenerateAllDecals()
    {
        EchoesMaterialLibrary.EnsureMaterials();
        EchoesMaterialLibrary.EnsureFolderExists(TargetFolder);

        CreateMoistureLinesPrefab();
        CreateLyraNotesPrefab();
        CreateFloorDragPrefab();
        CreateCrackLiminalPrefab();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Decal Prefabs] Successfully generated 4 polygonal decal prefabs.");
    }

    private static void CreateMoistureLinesPrefab()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = "dec_moisture_lines";
        Object.DestroyImmediate(go.GetComponent<Collider>());
        
        go.transform.localScale = new Vector3(4f, 0.1f, 4f);
        go.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        Material mat = new Material(Shader.Find("Hidden/RetroFlatLit"));
        mat.name = "Mat_Decal_Moisture";
        mat.SetColor("_Color", new Color(0f, 0f, 0f, 0.15f));
        mat.SetFloat("_Surface", 1f);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        string matPath = "Assets/Materials/Echoes/Mat_Decal_Moisture.mat";
        AssetDatabase.CreateAsset(mat, matPath);

        go.GetComponent<MeshRenderer>().sharedMaterial = mat;

        string prefabPath = Path.Combine(TargetFolder, "dec_moisture_lines.prefab").Replace("\\", "/");
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);
    }

    private static void CreateLyraNotesPrefab()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = "dec_lyra_notes";
        Object.DestroyImmediate(go.GetComponent<Collider>());

        go.transform.localScale = new Vector3(2f, 2f, 1f);

        Material mat = EchoesMaterialLibrary.GetMaterial("echo-cyan");
        if (mat == null)
        {
            mat = new Material(Shader.Find("Hidden/AnalogGhost"));
            mat.name = "Mat_Decal_LyraNotes";
            mat.SetColor("_Color", new Color(0.31f, 0.765f, 0.91f, 0.45f));
        }

        go.GetComponent<MeshRenderer>().sharedMaterial = mat;

        string prefabPath = Path.Combine(TargetFolder, "dec_lyra_notes.prefab").Replace("\\", "/");
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);
    }

    private static void CreateFloorDragPrefab()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = "dec_floor_drag";
        Object.DestroyImmediate(go.GetComponent<Collider>());

        go.transform.position = new Vector3(0f, 0.01f, 0f);
        go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        go.transform.localScale = new Vector3(1.5f, 3f, 1f);

        Material mat = new Material(Shader.Find("Hidden/RetroFlatLit"));
        mat.name = "Mat_Decal_FloorDrag";
        mat.SetColor("_Color", new Color(0f, 0f, 0f, 0.3f));
        mat.SetFloat("_Surface", 1f);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        string matPath = "Assets/Materials/Echoes/Mat_Decal_FloorDrag.mat";
        AssetDatabase.CreateAsset(mat, matPath);

        go.GetComponent<MeshRenderer>().sharedMaterial = mat;

        string prefabPath = Path.Combine(TargetFolder, "dec_floor_drag.prefab").Replace("\\", "/");
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);
    }

    private static void CreateCrackLiminalPrefab()
    {
        GameObject go = new GameObject("dec_crack_liminal");
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.startWidth = 0.04f;
        lr.endWidth = 0.01f;
        lr.positionCount = 6;

        Vector3[] positions = new Vector3[]
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(0.2f, 0.5f, 0.01f),
            new Vector3(-0.1f, 1.1f, 0.01f),
            new Vector3(0.3f, 1.6f, 0.01f),
            new Vector3(0.1f, 2.2f, 0.01f),
            new Vector3(0.4f, 2.8f, 0.01f)
        };
        lr.SetPositions(positions);

        Material mat = EchoesMaterialLibrary.GetMaterial("void-black");
        if (mat == null)
        {
            mat = new Material(Shader.Find("Hidden/RetroFlatLit"));
            mat.name = "Mat_Decal_Crack";
            mat.SetColor("_Color", Color.black);
        }

        lr.sharedMaterial = mat;

        string prefabPath = Path.Combine(TargetFolder, "dec_crack_liminal.prefab").Replace("\\", "/");
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);
    }
}
