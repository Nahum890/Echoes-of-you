using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Echoes.EnvironmentPass;

public static class PropScaleAndMaterialFixer
{
    private const float TargetMaxDimension = 1.5f;
    public const string ExtintorMaterialPath = "Assets/Materials/Echoes/Mat_Extintor.mat";
    private const string ExtintorTexturePath = "Assets/Textures/ExtintorTexture.png";

    [MenuItem("Tools/Apply Prop Scales and Extintor Fix")]
    public static void RunFixes()
    {
        EnsureExtintorMaterial();
        var prefabScaleMap = ComputePrefabScaleFactors();
        UpdatePropPlacements(prefabScaleMap);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Prop scaling and Extintor material fix applied.");
    }

    private static void EnsureExtintorMaterial()
    {
        if (!AssetDatabase.LoadAssetAtPath<Texture2D>(ExtintorTexturePath))
        {
            var tex = new Texture2D(256, 256);
            for (int y = 0; y < 256; y++)
                for (int x = 0; x < 256; x++)
                    tex.SetPixel(x, y, Color.red);
            tex.Apply();
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(ExtintorTexturePath));
            System.IO.File.WriteAllBytes(ExtintorTexturePath, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(ExtintorTexturePath, ImportAssetOptions.ForceUpdate);
        }

        var mat = AssetDatabase.LoadAssetAtPath<Material>(ExtintorMaterialPath);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(ExtintorTexturePath);
            if (tex != null) mat.SetTexture("_BaseMap", tex);
            mat.color = Color.red;
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(ExtintorMaterialPath));
            AssetDatabase.CreateAsset(mat, ExtintorMaterialPath);
            AssetDatabase.ImportAsset(ExtintorMaterialPath, ImportAssetOptions.ForceUpdate);
        }
    }

    public static Dictionary<string, Vector3> ComputePrefabScaleFactors()
    {
        var map = new Dictionary<string, Vector3>();
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Props" });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;
            var renderers = prefab.GetComponentsInChildren<MeshRenderer>(true);
            if (renderers.Length == 0) continue;
            var combinedBounds = new Bounds(renderers[0].bounds.center, Vector3.zero);
            foreach (var rend in renderers)
                combinedBounds.Encapsulate(rend.bounds);
            var maxDim = Mathf.Max(combinedBounds.size.x, combinedBounds.size.y, combinedBounds.size.z);
            if (maxDim <= 0.001f) continue;
            var scaleFactor = TargetMaxDimension / maxDim;
            map[prefab.name] = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        }
        return map;
    }

    private static void UpdatePropPlacements(Dictionary<string, Vector3> scaleMap)
    {
        string[] placementGuids = AssetDatabase.FindAssets("t:PropPlacementSO", new[] { "Assets/ScriptableObjects/EnvironmentPass" });
        Material extintorMat = AssetDatabase.LoadAssetAtPath<Material>(ExtintorMaterialPath);
        int updated = 0;
        foreach (var guid in placementGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var placement = AssetDatabase.LoadAssetAtPath<PropPlacementSO>(path);
            if (placement == null) continue;
            if (!scaleMap.TryGetValue(placement.prefabName, out var newScale)) continue;
            placement.scale = newScale;
            if (placement.prefabName == "Extintor" && extintorMat != null)
                placement.materialOverride = extintorMat;
            EditorUtility.SetDirty(placement);
            updated++;
        }
        Debug.Log($"Updated {updated} PropPlacementSO assets.");
    }
}