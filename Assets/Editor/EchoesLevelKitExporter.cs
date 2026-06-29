using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Genera prefabs editables en Assets/Prefabs/LevelKit para paths, placas y tutoriales.
/// </summary>
public static class EchoesLevelKitExporter
{
    const string KitRoot = "Assets/Prefabs/LevelKit";

    [MenuItem("Echoes of You/Production/Export Level Kit Prefabs", false, 210)]
    public static void ExportLevelKitPrefabs()
    {
        if (!Directory.Exists(KitRoot))
            Directory.CreateDirectory(KitRoot);

        SavePrefab(CreateEchoPathHint(), $"{KitRoot}/EchoPathHint.prefab");
        SavePrefab(CreatePressurePlate("Placa_Eco", true), $"{KitRoot}/PressurePlate_Eco.prefab");
        SavePrefab(CreatePressurePlate("Placa_Jugador", false), $"{KitRoot}/PressurePlate_Player.prefab");
        SavePrefab(CreateTutorialTrigger(), $"{KitRoot}/TutorialTrigger.prefab");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[Echoes] Level Kit prefabs exported to {KitRoot}. Arrástralos a tus niveles para editarlos.");
    }

    static GameObject CreateEchoPathHint()
    {
        GameObject root = new GameObject("EchoPathHint");
        EchoPathHint hint = root.AddComponent<EchoPathHint>();
        hint.SetWaypoints(new[]
        {
            new Vector3(0f, 0.5f, 0f),
            new Vector3(2f, 0.5f, 2f),
            new Vector3(4f, 0.5f, 4f)
        });
        return root;
    }

    static GameObject CreatePressurePlate(string name, bool echoPlate)
    {
        GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        plate.name = name;
        plate.transform.localScale = new Vector3(2f, 0.12f, 2f);
        Object.DestroyImmediate(plate.GetComponent<Collider>());
        plate.AddComponent<PressurePlate>();
        PressurePlateAlignment align = plate.AddComponent<PressurePlateAlignment>();
        align.echoProjectionPlate = echoPlate;
        return plate;
    }

    static GameObject CreateTutorialTrigger()
    {
        GameObject root = new GameObject("TutorialTrigger");
        BoxCollider box = root.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(8f, 4f, 8f);
        root.AddComponent<TutorialTrigger>();
        return root;
    }

    static void SavePrefab(GameObject source, string path)
    {
        PrefabUtility.SaveAsPrefabAsset(source, path);
        Object.DestroyImmediate(source);
    }
}
