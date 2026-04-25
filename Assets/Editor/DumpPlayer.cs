using UnityEditor;
using UnityEngine;
using System.Text;

public static class DumpPlayer
{
    [MenuItem("Echoes of You/Dump Player Info")]
    public static void Run()
    {
        var player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogError("Player not found in scene!");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== PLAYER HIERARCHY DUMP ===");
        DumpRecursive(player.transform, sb, 0);
        
        Debug.Log(sb.ToString());
    }

    static void DumpRecursive(Transform t, StringBuilder sb, int indent)
    {
        string ind = new string('-', indent * 2);
        
        var renderers = t.GetComponents<Renderer>();
        string rInfo = "";
        if (renderers.Length > 0)
        {
            rInfo = $" [Renderers: {renderers.Length}]";
            foreach (var r in renderers)
            {
                rInfo += $" (Bounds: {r.bounds.size}, Enabled: {r.enabled}, Visible: {r.isVisible})";
                if (r is SkinnedMeshRenderer smr)
                {
                    rInfo += $" (RootBone: {(smr.rootBone ? smr.rootBone.name : "NULL")})";
                    if (smr.sharedMesh != null)
                        rInfo += $" (Mesh Bounds: {smr.sharedMesh.bounds.size})";
                }
            }
        }

        sb.AppendLine($"{ind}> {t.name} (Pos: {t.localPosition}, Scale: {t.localScale}){rInfo}");

        for (int i = 0; i < t.childCount; i++)
        {
            DumpRecursive(t.GetChild(i), sb, indent + 1);
        }
    }
}
