using UnityEngine;
using UnityEditor;
using System.IO;

public static class DumpHierarchy
{
    const string CasualFbxPath = "Assets/3D Models/Animated Woman/Casual.fbx";

    [MenuItem("Tools/Echoes/Dump Hierarchy")]
    static void Run()
    {
        GameObject casualFbx = AssetDatabase.LoadAssetAtPath<GameObject>(CasualFbxPath);
        if (casualFbx == null)
        {
            Debug.LogError("Could not load Casual.fbx");
            return;
        }

        GameObject temp = Object.Instantiate(casualFbx);
        try
        {
            string report = "Casual.fbx Bone Hierarchy:\n";
            foreach (Transform t in temp.GetComponentsInChildren<Transform>(true))
            {
                string parentName = t.parent != null ? t.parent.name : "NONE";
                // Get full path relative to the instantiated root
                string path = t.name;
                Transform curr = t.parent;
                while (curr != null && curr != temp.transform)
                {
                    path = curr.name + "/" + path;
                    curr = curr.parent;
                }
                report += string.Format("• Name: {0} | Parent: {1} | Path: {2}\n", t.name, parentName, path);
            }

            string destPath = Path.Combine(Application.dataPath, "../CasualHierarchy.txt");
            File.WriteAllText(destPath, report);
            Debug.Log("Hierarchy dumped to: " + destPath);
            EditorUtility.DisplayDialog("Dump Complete", "Hierarchy written to CasualHierarchy.txt in project root folder.", "OK");
        }
        finally
        {
            Object.DestroyImmediate(temp);
        }
    }
}
