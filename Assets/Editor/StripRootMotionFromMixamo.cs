using UnityEngine;
using UnityEditor;

public class StripRootMotionFromMixamo
{
    [MenuItem("Echoes of You/Tools/Strip Root Motion from Mixamo Clips")]
    public static void StripAll()
    {
        string[] paths = {
            "Assets/3D Models/Animaciones/Locomotion/idle.fbx",
            "Assets/3D Models/Animaciones/Locomotion/walking.fbx",
            "Assets/3D Models/Animaciones/Locomotion/running.fbx",
            "Assets/3D Models/Animaciones/Locomotion/jump.fbx",
        };

        int count = 0;
        foreach (var path in paths)
        {
            string fbxName = System.IO.Path.GetFileNameWithoutExtension(path);
            var clips = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in clips)
            {
                if (!(a is AnimationClip src) || src.name.StartsWith("__preview__"))
                    continue;

                string outPath = $"Assets/3D Models/Animaciones/Stripped/{fbxName}_NoRoot.anim";

                // Create the clip with curves minus root motion
                var newClip = new AnimationClip
                {
                    name = src.name + "_NoRoot",
                    legacy = false,
                    wrapMode = src.wrapMode,
                };

                // Set loop time
                var settings = AnimationUtility.GetAnimationClipSettings(src);
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(newClip, settings);

                var bindings = AnimationUtility.GetCurveBindings(src);
                foreach (var b in bindings)
                {
                    // Skip root motion curves
                    if (b.path == "" && (b.propertyName.StartsWith("RootT") || b.propertyName.StartsWith("RootQ")))
                        continue;

                    var curve = AnimationUtility.GetEditorCurve(src, b);
                    if (curve != null)
                        AnimationUtility.SetEditorCurve(newClip, b, curve);
                }

                // Ensure output folder
                if (!AssetDatabase.IsValidFolder("Assets/3D Models/Animaciones/Stripped"))
                    AssetDatabase.CreateFolder("Assets/3D Models/Animaciones", "Stripped");

                AssetDatabase.CreateAsset(newClip, outPath);
                count++;
                Debug.Log($"[StripRootMotion] Created {outPath} (rootT/rootQ removed, looping=true)");
            }
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Root Motion Stripped", $"Created {count} clips without root motion in Assets/3D Models/Animaciones/Stripped/", "OK");
    }
}
