#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class InspectClips
{
    [MenuItem("Echoes/Inspect Clips")]
    public static void Inspect()
    {
        string pathNoRM = "Assets/3D Models/Animaciones/Universal Animation Library[Standard]/Universal Animation Library[Standard]/Unity/UAL1_Standard.fbx";
        string pathRM = "Assets/3D Models/Animaciones/Universal Animation Library[Standard]/Universal Animation Library[Standard]/Unity/UAL1_Standard_RM.fbx";
        string outputPath = "Assets/Editor/clips_list.txt";

        using (StreamWriter writer = new StreamWriter(outputPath))
        {
            writer.WriteLine("=== UAL1_Standard.fbx ===");
            Object[] assetsNoRM = AssetDatabase.LoadAllAssetsAtPath(pathNoRM);
            writer.WriteLine($"Total sub-assets: {assetsNoRM.Length}");
            foreach (Object asset in assetsNoRM)
            {
                if (asset is AnimationClip clip)
                {
                    writer.WriteLine($"Clip: {clip.name} (Length: {clip.length}s, IsLoop: {clip.isLooping})");
                }
            }

            writer.WriteLine("\n=== UAL1_Standard_RM.fbx ===");
            Object[] assetsRM = AssetDatabase.LoadAllAssetsAtPath(pathRM);
            writer.WriteLine($"Total sub-assets: {assetsRM.Length}");
            foreach (Object asset in assetsRM)
            {
                if (asset is AnimationClip clip)
                {
                    writer.WriteLine($"Clip: {clip.name} (Length: {clip.length}s, IsLoop: {clip.isLooping})");
                }
            }
        }
        Debug.Log("Clips inspected and saved to " + outputPath);
    }
}
#endif
