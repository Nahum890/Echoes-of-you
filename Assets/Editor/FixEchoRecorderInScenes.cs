using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Echoes.UI;

public class FixEchoRecorderInScenes
{
    [MenuItem("Echoes of You/Production/Fix EchoRecorder in All Scenes")]
    public static void FixAll()
    {
        int fixedCount = 0;
        for (int i = 1; i <= 15; i++)
        {
            string scenePath = $"Assets/Scenes/Level_{i:D2}.unity";
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath))
            {
                Debug.LogWarning($"Scene not found: {scenePath}");
                continue;
            }

            string bpPath = $"Assets/Data/Levels/Level_{i:D2}_Blueprint.asset";
            var bp = AssetDatabase.LoadAssetAtPath<LevelBlueprint>(bpPath);
            if (bp == null)
            {
                Debug.LogWarning($"Blueprint not found: {bpPath}");
                continue;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var recorder = Object.FindAnyObjectByType<EchoRecorder>();
            if (recorder == null)
            {
                Debug.LogWarning($"[{scenePath}] EchoRecorder not found");
                continue;
            }

            var so = new SerializedObject(recorder);

            // Apply blueprint values
            int targetMax = bp.echoEnabled ? Mathf.Max(1, bp.maxEchoes) : 0;
            so.FindProperty("maxEchoes").intValue = targetMax;
            so.FindProperty("maxRecordSeconds").floatValue = bp.maxRecordSeconds > 0 ? bp.maxRecordSeconds : 12f;

            // Fix echoPrefab if null
            var epProp = so.FindProperty("echoPrefab");
            if (epProp.objectReferenceValue == null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/EchoPrefab.prefab");
                if (prefab != null) epProp.objectReferenceValue = prefab;
            }

            // Fix HUD if null
            var hudProp = so.FindProperty("hud");
            if (hudProp.objectReferenceValue == null)
            {
                var hud = Object.FindAnyObjectByType<GameHUD>();
                if (hud != null) hudProp.objectReferenceValue = hud;
            }

            // Fix echoSpawnRoot if null
            var esrProp = so.FindProperty("echoSpawnRoot");
            if (esrProp.objectReferenceValue == null)
            {
                var mech = GameObject.Find("--- MECHANICS ---");
                if (mech != null) esrProp.objectReferenceValue = mech.transform;
                else esrProp.objectReferenceValue = recorder.transform;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            fixedCount++;
            Debug.Log($"[{scenePath}] EchoRecorder fixed: maxEchoes={targetMax} maxRecordSeconds={so.FindProperty("maxRecordSeconds").floatValue} echoPrefab={epProp.objectReferenceValue != null} hud={hudProp.objectReferenceValue != null}");
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("EchoRecorder Fixed", $"Fixed {fixedCount}/15 scenes.", "OK");
    }
}
