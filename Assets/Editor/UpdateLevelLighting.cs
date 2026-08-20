using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class UpdateLevelLighting
{
    struct ChapterProfile
    {
        public Color fogColor;
        public float fogDensity;
        public Color ambientColor;
        public float sunIntensity;
        public Color sunColor;
        public Vector3 sunEuler;
    }

    static ChapterProfile GetChapter(int levelNum)
    {
        string ch;
        if (levelNum <= 3) ch = "I";
        else if (levelNum == 4 || levelNum == 5 || levelNum == 8) ch = "II";
        else if (levelNum <= 9) ch = "III";
        else if (levelNum <= 11) ch = "IV";
        else if (levelNum <= 13) ch = "V";
        else ch = "VI";

        switch (ch)
        {
            case "I":
                return new ChapterProfile
                {
                    fogColor = new Color(0.109f, 0.141f, 0.188f, 1f),
                    fogDensity = 0.008f,
                    ambientColor = new Color(0.059f, 0.078f, 0.102f, 1f),
                    sunIntensity = 0.85f,
                    sunColor = new Color(0.949f, 0.949f, 1f, 1f),
                    sunEuler = new Vector3(50f, -30f, 0f)
                };
            case "II":
                return new ChapterProfile
                {
                    fogColor = new Color(0.180f, 0.188f, 0.141f, 1f),
                    fogDensity = 0.010f,
                    ambientColor = new Color(0.102f, 0.110f, 0.078f, 1f),
                    sunIntensity = 0.85f,
                    sunColor = new Color(0.949f, 0.949f, 1f, 1f),
                    sunEuler = new Vector3(50f, -30f, 0f)
                };
            case "III":
                return new ChapterProfile
                {
                    fogColor = new Color(0.165f, 0.118f, 0.118f, 1f),
                    fogDensity = 0.012f,
                    ambientColor = new Color(0.078f, 0.055f, 0.055f, 1f),
                    sunIntensity = 0.85f,
                    sunColor = new Color(0.949f, 0.949f, 1f, 1f),
                    sunEuler = new Vector3(50f, -30f, 0f)
                };
            case "IV":
                return new ChapterProfile
                {
                    fogColor = new Color(0.231f, 0.188f, 0.141f, 1f),
                    fogDensity = 0.015f,
                    ambientColor = new Color(0.118f, 0.094f, 0.071f, 1f),
                    sunIntensity = 0.85f,
                    sunColor = new Color(0.949f, 0.949f, 1f, 1f),
                    sunEuler = new Vector3(50f, -30f, 0f)
                };
            case "V":
                return new ChapterProfile
                {
                    fogColor = new Color(0.102f, 0.063f, 0.125f, 1f),
                    fogDensity = 0.020f,
                    ambientColor = new Color(0.047f, 0.031f, 0.063f, 1f),
                    sunIntensity = 0.85f,
                    sunColor = new Color(0.949f, 0.949f, 1f, 1f),
                    sunEuler = new Vector3(50f, -30f, 0f)
                };
            default:
                return new ChapterProfile
                {
                    fogColor = new Color(0.941f, 0.957f, 1f, 1f),
                    fogDensity = 0.002f,
                    ambientColor = new Color(1f, 1f, 1f, 1f),
                    sunIntensity = 0.85f,
                    sunColor = new Color(0.949f, 0.949f, 1f, 1f),
                    sunEuler = new Vector3(50f, -30f, 0f)
                };
        }
    }

    [MenuItem("Echoes of You/Production/Update All Level Lighting to Spec")]
    public static void UpdateAllScenes()
    {
        int updated = 0;
        for (int i = 1; i <= 15; i++)
        {
            string scenePath = $"Assets/Scenes/Level_{i:D2}.unity";
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath))
            {
                Debug.LogWarning($"Scene not found: {scenePath}");
                continue;
            }

            var profile = GetChapter(i);
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            var lls = Object.FindAnyObjectByType<LevelLightingSettings>();
            if (lls == null)
            {
                Debug.LogWarning($"[{scenePath}] LevelLightingSettings not found - creating on LevelLighting");
                var go = GameObject.Find("LevelLighting");
                if (go == null)
                {
                    var env = GameObject.Find("--- ENVIRONMENT ---");
                    go = new GameObject("LevelLighting");
                    if (env != null) go.transform.SetParent(env.transform, true);
                }
                lls = go.AddComponent<LevelLightingSettings>();
            }

            var so = new SerializedObject(lls);
            so.FindProperty("directionalIntensity").floatValue = profile.sunIntensity;
            so.FindProperty("directionalColor").colorValue = profile.sunColor;
            so.FindProperty("directionalEuler").vector3Value = profile.sunEuler;
            so.FindProperty("ambientColor").colorValue = profile.ambientColor;
            so.FindProperty("reflectionIntensity").floatValue = 0.18f;
            so.FindProperty("enableFog").boolValue = true;
            so.FindProperty("fogColor").colorValue = profile.fogColor;
            so.FindProperty("fogDensity").floatValue = profile.fogDensity;
            so.FindProperty("overrideDirectional").boolValue = true;
            so.FindProperty("overrideAmbient").boolValue = true;
            so.FindProperty("disableRuntimeFillLights").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            updated++;
            Debug.Log($"[{scenePath}] Lighting updated: fog={profile.fogDensity} amb={profile.ambientColor} sun={profile.sunIntensity}");
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Level Lighting Updated", $"Updated {updated}/15 scenes with spec lighting profiles.", "OK");
    }
}
