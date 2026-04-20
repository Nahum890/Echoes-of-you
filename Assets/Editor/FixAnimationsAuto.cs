using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class FixAnimationsAuto 
{
    static FixAnimationsAuto()
    {
        if (EditorPrefs.GetBool("AnimationsFixed", false)) return;

        string[] paths = new string[] {
            "Assets/3D Models/Character Base/characterBase.fbx",
            "Assets/3D Models/Animaciones/Locomotion/idle.fbx",
            "Assets/3D Models/Animaciones/Locomotion/walking.fbx",
            "Assets/3D Models/Animaciones/Locomotion/running.fbx",
            "Assets/3D Models/Animaciones/Locomotion/jump.fbx"
        };

        bool anyFixed = false;
        foreach (var p in paths)
        {
            var importer = AssetImporter.GetAtPath(p) as ModelImporter;
            if (importer != null && importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.SaveAndReimport();
                Debug.Log("[Echoes] Reimported to Humanoid: " + p);
                anyFixed = true;
            }
        }

        if (anyFixed)
        {
            Debug.Log("[Echoes] Successfully configured Character and Animations as Humanoid.");
        }
        EditorPrefs.SetBool("AnimationsFixed", true);
    }
}
