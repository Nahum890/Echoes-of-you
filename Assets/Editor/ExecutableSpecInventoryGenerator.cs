#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Generates machine-readable evidence from the actual Unity project.
/// The generated files are evidence, not hand-edited source specifications.
/// </summary>
public static class ExecutableSpecInventoryGenerator
{
    private const string OutputDirectory = "Reports/generated";

    [Serializable]
    private sealed class CodeInventory
    {
        public string generated_at;
        public List<CodeEntry> files = new List<CodeEntry>();
    }

    [Serializable]
    private sealed class CodeEntry
    {
        public string path;
        public List<string> types = new List<string>();
        public List<string> serialized_fields = new List<string>();
    }

    [Serializable]
    private sealed class AssetInventory
    {
        public string generated_at;
        public List<AssetEntry> assets = new List<AssetEntry>();
    }

    [Serializable]
    private sealed class AssetEntry
    {
        public string path;
        public string guid;
        public string main_type;
        public List<string> components = new List<string>();
    }

    [Serializable]
    private sealed class BlueprintInventory
    {
        public string generated_at;
        public List<BlueprintEntry> levels = new List<BlueprintEntry>();
    }

    [Serializable]
    private sealed class BlueprintEntry
    {
        public string level_id;
        public string path;
        public bool exists;
        public bool camera_profile_null;
        public bool lighting_profile_missing;
        public float max_record_seconds;
        public int module_count;
        public int player_start_count;
        public int exit_or_goal_count;
        public List<int> module_type_values = new List<int>();
    }

    [MenuItem("Echoes of You/Specs/Generate Project Inventories")]
    public static void GenerateAll()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        Directory.CreateDirectory(Path.Combine(projectRoot, OutputDirectory));
        GenerateCodeInventory(projectRoot);
        GenerateAssetInventory(projectRoot);
        GenerateBlueprintInventory(projectRoot);
        AssetDatabase.Refresh();
        Debug.Log("[ExecutableSpecInventoryGenerator] Project inventories generated under Reports/generated.");
    }

    private static void GenerateCodeInventory(string projectRoot)
    {
        CodeInventory inventory = new CodeInventory { generated_at = DateTime.UtcNow.ToString("o") };
        string assetsRoot = Path.Combine(projectRoot, "Assets");
        Regex typePattern = new Regex(@"\b(class|struct|enum)\s+([A-Za-z_][A-Za-z0-9_]*)");
        Regex fieldPattern = new Regex(@"\[SerializeField\]\s*(?:private|protected|public)?\s*[A-Za-z0-9_<>\[\],.?]+\s+([A-Za-z_][A-Za-z0-9_]*)");

        foreach (string file in Directory.GetFiles(assetsRoot, "*.cs", SearchOption.AllDirectories))
        {
            string contents = File.ReadAllText(file);
            CodeEntry entry = new CodeEntry { path = ToProjectPath(projectRoot, file) };
            foreach (Match match in typePattern.Matches(contents))
                entry.types.Add(match.Groups[2].Value);
            foreach (Match match in fieldPattern.Matches(contents))
                entry.serialized_fields.Add(match.Groups[1].Value);
            inventory.files.Add(entry);
        }

        WriteJson(projectRoot, "code_inventory.json", inventory);
    }

    private static void GenerateAssetInventory(string projectRoot)
    {
        AssetInventory inventory = new AssetInventory { generated_at = DateTime.UtcNow.ToString("o") };
        string[] paths = AssetDatabase.GetAllAssetPaths();
        foreach (string path in paths)
        {
            if (!path.StartsWith("Assets/", StringComparison.Ordinal) || AssetDatabase.IsValidFolder(path))
                continue;

            string extension = Path.GetExtension(path).ToLowerInvariant();
            if (extension != ".prefab" && extension != ".mat" && extension != ".shader" && extension != ".asset" &&
                extension != ".png" && extension != ".jpg" && extension != ".jpeg" && extension != ".wav" &&
                extension != ".mp3" && extension != ".fbx" && extension != ".blend")
                continue;

            UnityEngine.Object mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
            AssetEntry entry = new AssetEntry
            {
                path = path,
                guid = AssetDatabase.AssetPathToGUID(path),
                main_type = mainAsset != null ? mainAsset.GetType().Name : "missing"
            };

            if (extension == ".prefab")
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    foreach (Component component in prefab.GetComponentsInChildren<Component>(true))
                        if (component != null && !entry.components.Contains(component.GetType().Name))
                            entry.components.Add(component.GetType().Name);
                }
            }

            inventory.assets.Add(entry);
        }

        WriteJson(projectRoot, "asset_inventory.json", inventory);
    }

    private static void GenerateBlueprintInventory(string projectRoot)
    {
        BlueprintInventory inventory = new BlueprintInventory { generated_at = DateTime.UtcNow.ToString("o") };
        for (int index = 1; index <= 15; index++)
        {
            string relative = $"Assets/Data/Levels/Level_{index:00}_Blueprint.asset";
            string absolute = Path.Combine(projectRoot, relative.Replace('/', Path.DirectorySeparatorChar));
            BlueprintEntry entry = new BlueprintEntry
            {
                level_id = $"N{index:00}",
                path = relative,
                exists = File.Exists(absolute)
            };

            if (entry.exists)
            {
                string contents = File.ReadAllText(absolute);
                entry.camera_profile_null = Regex.IsMatch(contents, @"^\s*cameraProfile:\s*\{fileID:\s*0\s*\}", RegexOptions.Multiline);
                entry.lighting_profile_missing = !Regex.IsMatch(contents, @"^\s*lightingProfile:\s*", RegexOptions.Multiline);
                Match duration = Regex.Match(contents, @"^\s*maxRecordSeconds:\s*([0-9.]+)", RegexOptions.Multiline);
                if (duration.Success)
                    float.TryParse(duration.Groups[1].Value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out entry.max_record_seconds);

                entry.player_start_count = Regex.Matches(contents, @"^\s*type:\s*7\s*$", RegexOptions.Multiline).Count;
                entry.exit_or_goal_count = Regex.Matches(contents, @"^\s*type:\s*(?:6|13)\s*$", RegexOptions.Multiline).Count;
                foreach (Match module in Regex.Matches(contents, @"^\s*type:\s*(-?[0-9]+)\s*$", RegexOptions.Multiline))
                {
                    if (int.TryParse(module.Groups[1].Value, out int typeValue))
                        entry.module_type_values.Add(typeValue);
                }
                entry.module_count = entry.module_type_values.Count;
            }

            inventory.levels.Add(entry);
        }

        WriteJson(projectRoot, "blueprint_inventory.json", inventory);
    }

    private static void WriteJson<T>(string projectRoot, string fileName, T value)
    {
        string path = Path.Combine(projectRoot, OutputDirectory, fileName);
        File.WriteAllText(path, JsonUtility.ToJson(value, true));
    }

    private static string ToProjectPath(string projectRoot, string absolutePath)
    {
        return absolutePath.Substring(projectRoot.Length + 1).Replace('\\', '/');
    }
}
#endif
