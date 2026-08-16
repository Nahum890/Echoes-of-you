using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class FixLightingAllScenes
{
    [MenuItem("Echoes of You/Production/Fix Lighting All Scenes (Spec)")]
    public static void Fix()
    {
        string[] scenePaths = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        foreach (string guid in scenePaths)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.Contains("Level_")) continue;

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            // Fix LevelLightingSettings if present
            var lighting = Object.FindAnyObjectByType<LevelLightingSettings>();
            if (lighting != null)
            {
                lighting.directionalIntensity = 0.85f;
                lighting.directionalColor = new Color(0.95f, 0.95f, 1f, 1f);
                lighting.directionalEuler = new Vector3(50f, -30f, 0f);
                lighting.ambientColor = new Color(0.15f, 0.15f, 0.15f, 1f);
                lighting.fogColor = new Color(0.1f, 0.1f, 0.12f, 1f);
                lighting.fogDensity = 0.008f;
                lighting.disableRuntimeFillLights = true;
                EditorUtility.SetDirty(lighting);
            }

            // Fix PlayerController groundProbeDistance + spawn position
            var playerCtrl = Object.FindAnyObjectByType<PlayerController>();
            if (playerCtrl != null)
            {
                playerCtrl.groundProbeDistance = 0.6f;
                // Place player so capsule bottom sits just above floor (y=0.02)
                var pos = playerCtrl.transform.position;
                playerCtrl.transform.position = new Vector3(pos.x, 0.02f, pos.z);
                EditorUtility.SetDirty(playerCtrl);
            }

            // Fix Directional Light directly
            foreach (var light in Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (light.type == LightType.Directional)
                {
                    light.intensity = 0.85f;
                    light.color = new Color(0.95f, 0.95f, 1f, 1f);
                    light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                    light.shadows = LightShadows.Hard;
                    EditorUtility.SetDirty(light);
                    break;
                }
            }

            // Fix ambient + fog in RenderSettings
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.15f, 0.15f, 0.15f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = new Color(0.1f, 0.1f, 0.12f, 1f);
            RenderSettings.fogDensity = 0.008f;
            QualitySettings.shadowDistance = 40f;

            // Fix Bloom volumes
            foreach (var vol in Object.FindObjectsByType<UnityEngine.Rendering.Volume>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (vol.profile != null && vol.profile.TryGet<UnityEngine.Rendering.Universal.Bloom>(out var bloom))
                {
                    bloom.intensity.value = 0.005f;
                    bloom.threshold.value = 1.2f;
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[FixLighting] Fixed: " + path);
        }

        EditorSceneManager.OpenScene("Assets/Scenes/Level_01.unity", OpenSceneMode.Single);
        AssetDatabase.SaveAssets();
        Debug.Log("[FixLighting] All scenes fixed.");
    }
}
