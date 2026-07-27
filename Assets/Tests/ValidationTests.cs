using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class ValidationTests
{
    private string GetProjectRoot()
    {
        return Directory.GetParent(Application.dataPath).FullName;
    }

    [Test]
    public void TestYamlSpecsExistAndHaveUniqueIds()
    {
        string projectRoot = GetProjectRoot();
        string docsSpecs = Path.Combine(projectRoot, "Docs", "Specs");
        string docsExecSpecs = Path.Combine(projectRoot, "Docs", "ExecutableSpecs");

        Assert.IsTrue(Directory.Exists(docsSpecs), "Docs/Specs directory must exist.");

        string[] requiredSpecs = new[]
        {
            Path.Combine(docsSpecs, "room_types.yaml"),
            Path.Combine(docsSpecs, "prop_rules.yaml"),
            Path.Combine(docsSpecs, "camera_profiles.yaml"),
            Path.Combine(docsSpecs, "lighting_profiles.yaml"),
            Path.Combine(docsSpecs, "puzzle_mechanics.yaml"),
            Path.Combine(docsSpecs, "validation_rules.yaml")
        };

        foreach (string specPath in requiredSpecs)
        {
            Assert.IsTrue(File.Exists(specPath), $"Required spec file missing: {specPath}");
        }

        // Global ID Uniqueness Check
        Dictionary<string, string> seenIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Regex idPattern = new Regex(@"^\s*(?:-\s*)?id:\s*([A-Z][A-Z0-9-]+)\s*$", RegexOptions.Multiline);

        List<string> searchPaths = new List<string>();
        if (Directory.Exists(docsSpecs)) searchPaths.AddRange(Directory.GetFiles(docsSpecs, "*.yaml", SearchOption.AllDirectories));
        if (Directory.Exists(docsExecSpecs)) searchPaths.AddRange(Directory.GetFiles(docsExecSpecs, "*.yaml", SearchOption.AllDirectories));

        List<string> duplicates = new List<string>();

        foreach (string file in searchPaths)
        {
            string relativePath = file.Substring(projectRoot.Length + 1).Replace('\\', '/');
            string content = File.ReadAllText(file);

            foreach (Match match in idPattern.Matches(content))
            {
                string id = match.Groups[1].Value;
                if (seenIds.TryGetValue(id, out string previousFile))
                {
                    duplicates.Add($"Duplicate ID '{id}' found in '{relativePath}' (first seen in '{previousFile}')");
                }
                else
                {
                    seenIds[id] = relativePath;
                }
            }
        }

        Assert.IsEmpty(duplicates, "Duplicate IDs detected across YAML specification files:\n" + string.Join("\n", duplicates));
    }

    [Test]
    public void TestAll15LevelBlueprintsAreValid()
    {
        string projectRoot = GetProjectRoot();

        for (int i = 1; i <= 15; i++)
        {
            string relativePath = $"Assets/Data/Levels/Level_{i:00}_Blueprint.asset";
            string fullPath = Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));

            Assert.IsTrue(File.Exists(fullPath), $"Blueprint asset file missing: {relativePath}");

            string content = File.ReadAllText(fullPath);

            // Camera profile non-null
            Assert.IsFalse(Regex.IsMatch(content, @"^\s*cameraProfile:\s*\{fileID:\s*0\s*\}", RegexOptions.Multiline),
                $"Level_{i:00}_Blueprint has a null cameraProfile!");

            // Lighting profile non-null
            Assert.IsTrue(Regex.IsMatch(content, @"^\s*lightingProfile:\s*", RegexOptions.Multiline),
                $"Level_{i:00}_Blueprint missing lightingProfile field!");
            Assert.IsFalse(Regex.IsMatch(content, @"^\s*lightingProfile:\s*\{fileID:\s*0\s*\}", RegexOptions.Multiline),
                $"Level_{i:00}_Blueprint has a null lightingProfile!");

            // Exactly 1 PlayerStart (type 7)
            int playerStartCount = Regex.Matches(content, @"^\s*type:\s*7\s*$", RegexOptions.Multiline).Count;
            Assert.AreEqual(1, playerStartCount, $"Level_{i:00}_Blueprint must have exactly 1 PlayerStart (type 7), found {playerStartCount}.");

            // At least 1 exit / goal (type 6 or 13)
            int exitCount = Regex.Matches(content, @"^\s*type:\s*(?:6|13)\s*$", RegexOptions.Multiline).Count;
            Assert.GreaterOrEqual(exitCount, 1, $"Level_{i:00}_Blueprint must have at least 1 Exit or Goal (type 6 or 13).");

            // ModuleType validity (0 to 46)
            MatchCollection typeMatches = Regex.Matches(content, @"^\s*type:\s*([0-9]+)\s*$", RegexOptions.Multiline);
            foreach (Match match in typeMatches)
            {
                int typeVal = int.Parse(match.Groups[1].Value);
                Assert.IsTrue(typeVal >= 0 && typeVal <= 46, $"Level_{i:00}_Blueprint contains invalid ModuleType enum value: {typeVal}");
            }

            // Record duration (1..30)
            Match durationMatch = Regex.Match(content, @"^\s*maxRecordSeconds:\s*([0-9.]+)", RegexOptions.Multiline);
            Assert.IsTrue(durationMatch.Success, $"Level_{i:00}_Blueprint missing maxRecordSeconds!");
            float seconds = float.Parse(durationMatch.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
            Assert.IsTrue(seconds >= 1f && seconds <= 30f, $"Level_{i:00}_Blueprint maxRecordSeconds out of range: {seconds}");
        }
    }

    [Test]
    public void TestExecutableSpecValidatorPassesCleanly()
    {
        string projectRoot = GetProjectRoot();
        Type validatorType = Type.GetType("ExecutableSpecValidator, Assembly-CSharp-Editor") ?? Type.GetType("ExecutableSpecValidator");
        if (validatorType != null)
        {
            var method = validatorType.GetMethod("ValidateProject", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            method?.Invoke(null, null);
        }

        string reportPath = Path.Combine(projectRoot, "Reports", "generated", "executable_specs_report.json");
        Assert.IsTrue(File.Exists(reportPath), "Executable spec validator report was not generated.");

        string json = File.ReadAllText(reportPath);
        Assert.IsFalse(json.Contains("\"fatal\":") && !json.Contains("\"fatal\": 0"), "Validator reported fatal errors!");
        Assert.IsFalse(json.Contains("\"errors\":") && !json.Contains("\"errors\": 0"), "Validator reported errors!");

        Assert.IsTrue(json.Contains("\"status\": \"approved\"") || json.Contains("\"status\": \"approved_with_warnings\""),
            $"Validator status was not approved. Json: {json}");
    }
}
