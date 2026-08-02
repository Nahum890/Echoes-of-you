using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class FixScenePostBuild
{
    [MenuItem("Echoes of You/Production/Fix Scenes Post-Build", false, 210)]
    public static void FixAll()
    {
        string[] scenePaths = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        foreach (string guid in scenePaths)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.Contains("Level_")) continue;
            FixScene(path);
        }
        EditorSceneManager.OpenScene("Assets/Scenes/Level_01.unity", OpenSceneMode.Single);
        AssetDatabase.SaveAssets();
        Debug.Log("[FixPostBuild] All scenes fixed.");
    }

    static void FixScene(string path)
    {
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

        // 1. Fix Player layer
        var playerCtrl = Object.FindAnyObjectByType<PlayerController>();
        if (playerCtrl != null)
        {
            playerCtrl.gameObject.layer = 8;
            playerCtrl.gameObject.tag = "Player";
            EditorUtility.SetDirty(playerCtrl);
        }

        // 2. Fix Point Lights: shadows=None, max intensity=1.0
        foreach (var light in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (light.type == LightType.Point)
            {
                bool changed = false;
                if (light.shadows != LightShadows.None)
                {
                    light.shadows = LightShadows.None;
                    changed = true;
                }
                if (light.intensity > 1.0f)
                {
                    light.intensity = Mathf.Min(light.intensity, 1.0f);
                    changed = true;
                }
                if (changed) EditorUtility.SetDirty(light);
            }
        }

        // 3. Fix groundProbeDistance and groundedStickForce on player
        if (playerCtrl != null)
        {
            var so = new SerializedObject(playerCtrl);
            so.Update();
            var pDist = so.FindProperty("groundProbeDistance");
            if (pDist != null) { pDist.floatValue = 0.6f; }
            var pStick = so.FindProperty("groundedStickForce");
            if (pStick != null) { pStick.floatValue = 0.5f; }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[FixPostBuild] Fixed: " + path);
    }
}