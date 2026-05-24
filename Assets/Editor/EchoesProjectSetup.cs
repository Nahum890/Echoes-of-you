using UnityEditor;

[InitializeOnLoad]
public static class EchoesProjectSetup
{
    const string EchoProjectionTag = "EchoProjection";

    static EchoesProjectSetup()
    {
        EnsureTag(EchoProjectionTag);
    }

    [MenuItem("Echoes of You/Production/Ensure Project Tags", false, 50)]
    public static void EnsureProjectTagsMenu()
    {
        EnsureTag(EchoProjectionTag);
        AssetDatabase.SaveAssets();
        UnityEngine.Debug.Log("[Echoes] Tags verified (EchoProjection).");
    }

    static void EnsureTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
        SerializedProperty tags = tagManager.FindProperty("tags");

        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                return;
        }

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }
}
