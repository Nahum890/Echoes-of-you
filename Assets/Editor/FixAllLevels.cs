using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.Cinemachine;

public static class FixAllLevels
{
    [MenuItem("Tools/Fix All Levels")]
    static void FixAll()
    {
        string[] nums = { "04","05","06","07","08","09","10","11","12","13","14","15" };
        foreach (string num in nums)
        {
            string path = "Assets/Scenes/Level_" + num + ".unity";
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            var pv = GameObject.Find("PlayerVCam");
            if (pv != null)
            {
                var fw = pv.GetComponent<CinemachineFollow>();
                if (fw != null) fw.FollowOffset = new Vector3(0f, 1.5f, -4f);
                var cm = pv.GetComponent<CinemachineCamera>();
                if (cm != null)
                {
                    var player = GameObject.Find("Player");
                    if (player != null) { cm.Target.LookAtTarget = player.transform; cm.Target.CustomLookAtTarget = false; }
                }
            }

            var env = GameObject.Find("--- ENVIRONMENT ---");
            if (env != null)
            {
                foreach (Transform c in env.transform)
                {
                    if (c.name.Contains("Fluorescente") || c.name.StartsWith("Luz"))
                    {
                        Light l = c.GetComponent<Light>();
                        if (l == null) foreach (Transform ch in c) { l = ch.GetComponent<Light>(); if (l != null) break; }
                        if (l != null && l.range <= 0.01f) { l.range = 20f; l.intensity = Mathf.Min(l.intensity, 10f); }
                    }
                }
            }

            var dl = GameObject.Find("Directional Light");
            if (dl != null)
            {
                Undo.RecordObject(dl.GetComponent<Light>(), "Fix intensity");
                dl.GetComponent<Light>().intensity = 1.5f;
            }

            var cam = Camera.main;
            if (cam != null) cam.backgroundColor = new Color(0.08f, 0.10f, 0.15f);

            EditorSceneManager.SaveScene(scene);
            Debug.Log("Fixed " + path);
        }
        EditorSceneManager.OpenScene("Assets/Scenes/Level_01.unity", OpenSceneMode.Single);
        Debug.Log("All levels fixed!");
    }
}
