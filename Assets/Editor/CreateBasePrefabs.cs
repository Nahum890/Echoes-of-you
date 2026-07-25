using UnityEditor;
using UnityEngine;

public static class CreateBasePrefabs
{
    [MenuItem("Echoes of You/Tools/Create Base Prefabs")]
    public static void CreateAllBasePrefabs()
    {
        PrefabBatchBuilder.BuildAllPrefabs();
        DecalPrefabGenerator.GenerateAllDecals();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CreateBasePrefabs] ✓ All base prefabs and decals created successfully.");
    }
}
