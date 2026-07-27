#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Deterministic preflight for the machine-readable project contract.
/// It deliberately reports implementation gaps instead of silently repairing them.
/// </summary>
public static class ExecutableSpecValidator
{
    private const string SpecsRoot = "Docs/ExecutableSpecs";
    private const string ReportPath = "Reports/generated/executable_specs_report.json";

    [Serializable]
    private sealed class Report
    {
        public string report_id;
        public string generated_at;
        public string subject;
        public string validator_version;
        public string status;
        public Summary summary = new Summary();
        public List<Check> checks = new List<Check>();
        public List<string> next_actions = new List<string>();
    }

    [Serializable]
    private sealed class Summary
    {
        public int fatal;
        public int errors;
        public int warnings;
        public int passed;
    }

    [Serializable]
    private sealed class Check
    {
        public string rule_id;
        public string status;
        public string severity;
        public string message;
        public string[] evidence;
        public string remediation;
    }

    [MenuItem("Echoes of You/Specs/Validate Executable Specs")]
    public static void ValidateProject()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        Report report = new Report
        {
            report_id = "RPT-SPECS-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
            generated_at = DateTime.UtcNow.ToString("o"),
            subject = "Echoes of You executable specification system",
            validator_version = "1.0"
        };

        CheckRequiredFiles(projectRoot, report);
        CheckDuplicateIds(projectRoot, report);
        CheckReferencedAssets(projectRoot, report);
        CheckSourceContracts(projectRoot, report);
        CheckBlueprints(projectRoot, report);

        if (report.summary.fatal > 0)
        {
            report.status = "blocked";
            report.next_actions.Add("Resolve every fatal check before autonomous level generation.");
        }
        else if (report.summary.errors > 0)
        {
            report.status = "needs_review";
            report.next_actions.Add("Resolve executable specification errors before approval.");
        }
        else if (report.summary.warnings > 0)
        {
            report.status = "approved_with_warnings";
        }
        else
        {
            report.status = "approved";
        }

        string reportAbsolutePath = Path.Combine(projectRoot, ReportPath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(reportAbsolutePath));
        File.WriteAllText(reportAbsolutePath, JsonUtility.ToJson(report, true));
        AssetDatabase.Refresh();

        Debug.Log($"[ExecutableSpecValidator] {report.status}: {report.summary.passed} passed, " +
                  $"{report.summary.fatal} fatal, {report.summary.errors} errors, {report.summary.warnings} warnings. " +
                  $"Report: {ReportPath}");
    }

    private static void CheckRequiredFiles(string projectRoot, Report report)
    {
        string[] required =
        {
            "Docs/ExecutableSpecs/manifest.yaml",
            "Docs/ExecutableSpecs/rules/authority.yaml",
            "Docs/ExecutableSpecs/registry/decisions.yaml",
            "Docs/ExecutableSpecs/registry/documents.yaml",
            "Docs/ExecutableSpecs/schemas/rule.schema.yaml",
            "Docs/ExecutableSpecs/schemas/blueprint.schema.json",
            "Docs/ExecutableSpecs/schemas/ai-task.schema.json",
            "Docs/ExecutableSpecs/schemas/validator-report.schema.json",
            "Docs/ExecutableSpecs/catalogs/scale.yaml",
            "Docs/ExecutableSpecs/catalogs/props.yaml",
            "Docs/ExecutableSpecs/catalogs/room_templates.yaml",
            "Docs/ExecutableSpecs/catalogs/modules.yaml",
            "Docs/ExecutableSpecs/architecture/school_graph.yaml",
            "Docs/ExecutableSpecs/gameplay/echo_states.yaml",
            "Docs/ExecutableSpecs/gameplay/puzzle_archetypes.yaml",
            "Docs/ExecutableSpecs/gameplay/blueprint_spec.yaml",
            "Docs/ExecutableSpecs/levels/level_structure.yaml",
            "Docs/ExecutableSpecs/levels/level_intents.yaml",
            "Docs/ExecutableSpecs/levels/puzzle_specs.yaml",
            "Docs/ExecutableSpecs/visual/camera_profiles.yaml",
            "Docs/ExecutableSpecs/visual/lighting_profiles.yaml",
            "Docs/ExecutableSpecs/visual/materials.yaml",
            "Docs/ExecutableSpecs/validators/static_validator.yaml",
            "Docs/ExecutableSpecs/validators/level_validator.yaml",
            "Docs/ExecutableSpecs/validators/blueprint_validator.yaml",
            "Docs/ExecutableSpecs/validators/documentation_validator.yaml",
            "Docs/ExecutableSpecs/ai/task_schema.yaml",
            "Docs/ExecutableSpecs/ai/report_schema.yaml"
        };

        foreach (string relativePath in required)
        {
            if (File.Exists(Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar))) )
                Pass(report, "DOC-VAL-001", "required_file", relativePath);
            else
                Fail(report, "DOC-VAL-001", "fatal", "Required executable specification file is missing: " + relativePath, relativePath, "Create or restore the required file.");
        }
    }

    private static void CheckDuplicateIds(string projectRoot, Report report)
    {
        string specsPath = Path.Combine(projectRoot, SpecsRoot.Replace('/', Path.DirectorySeparatorChar));
        Dictionary<string, string> firstOccurrence = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Regex idPattern = new Regex(@"^\s*(?:-\s*)?id:\s*([A-Z][A-Z0-9-]+)\s*$", RegexOptions.Multiline);

        foreach (string file in Directory.GetFiles(specsPath, "*.yaml", SearchOption.AllDirectories))
        {
            string relative = ToProjectPath(projectRoot, file);
            string contents = File.ReadAllText(file);
            foreach (Match match in idPattern.Matches(contents))
            {
                string id = match.Groups[1].Value;
                if (firstOccurrence.TryGetValue(id, out string previous))
                {
                    Fail(report, "DOC-VAL-002", "fatal", $"Duplicate executable id '{id}'.", relative + " / " + previous, "Rename one ID and update all references.");
                }
                else
                {
                    firstOccurrence[id] = relative;
                }
            }
        }

        Pass(report, "DOC-VAL-002", "duplicate_ids", "Scanned executable YAML files");
    }

    private static void CheckReferencedAssets(string projectRoot, Report report)
    {
        string specsPath = Path.Combine(projectRoot, SpecsRoot.Replace('/', Path.DirectorySeparatorChar));
        Regex pathPattern = new Regex(@"(?:asset|prefab|shader|code_source|enum_source|factory_source|runtime_authority|source):\s*(Assets/[A-Za-z0-9_./-]+\.(?:prefab|mat|shader|cs|asset|png|jpg|jpeg|wav|mp3))", RegexOptions.IgnoreCase);

        foreach (string file in Directory.GetFiles(specsPath, "*.yaml", SearchOption.AllDirectories))
        {
            string relative = ToProjectPath(projectRoot, file);
            string contents = File.ReadAllText(file);
            foreach (Match match in pathPattern.Matches(contents))
            {
                string assetPath = match.Groups[1].Value;
                string absolute = Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(absolute))
                    Pass(report, "DOC-VAL-006", "asset_reference", assetPath);
                else
                    Fail(report, "DOC-VAL-006", "error", "Executable catalog references missing asset: " + assetPath, relative, "Remove the reference or add the asset.");
            }
        }
    }

    private static void CheckSourceContracts(string projectRoot, Report report)
    {
        string[] sourceFiles = Directory.GetFiles(Path.Combine(projectRoot, "Assets"), "*.cs", SearchOption.AllDirectories);
        bool standardShaderFound = false;

        foreach (string sourceFile in sourceFiles)
        {
            string contents = File.ReadAllText(sourceFile);
            if (contents.Contains("Shader.Find(\"Standard\")") || contents.Contains("Shader.Find(\"Standard\")"))
            {
                standardShaderFound = true;
                Fail(report, "MAT-001", "fatal", "Built-in Standard shader reference found in source.", ToProjectPath(projectRoot, sourceFile), "Replace with an approved URP shader.");
            }
        }

        if (!standardShaderFound)
            Pass(report, "MAT-001", "urp_only", "No Shader.Find(\"Standard\") reference found.");

        string levelBlueprint = Path.Combine(projectRoot, "Assets/Scripts/LevelBlueprint.cs");
        string moduleFactory = Path.Combine(projectRoot, "Assets/Editor/EchoesModuleFactory.cs");
        if (!File.Exists(levelBlueprint))
            Fail(report, "MODULE-001", "fatal", "LevelBlueprint.cs is missing.", "Assets/Scripts/LevelBlueprint.cs", "Restore the active Blueprint class.");
        else
            Pass(report, "MODULE-001", "code_source", "LevelBlueprint.cs exists.");

        if (!File.Exists(moduleFactory))
            Fail(report, "MODULE-002", "fatal", "EchoesModuleFactory.cs is missing.", "Assets/Editor/EchoesModuleFactory.cs", "Restore the active module factory.");
        else
            Pass(report, "MODULE-002", "code_source", "EchoesModuleFactory.cs exists.");
    }

    private static void CheckBlueprints(string projectRoot, Report report)
    {
        for (int index = 1; index <= 15; index++)
        {
            string relative = $"Assets/Data/Levels/Level_{index:00}_Blueprint.asset";
            string absolute = Path.Combine(projectRoot, relative.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(absolute))
            {
                Fail(report, "BLUEPRINT-000", "fatal", "Blueprint asset is missing.", relative, "Restore or create the level Blueprint.");
                continue;
            }

            string contents = File.ReadAllText(absolute);
            Pass(report, "BLUEPRINT-000", "blueprint_exists", relative);

            if (Regex.IsMatch(contents, @"^\s*cameraProfile:\s*\{fileID:\s*0\s*\}", RegexOptions.Multiline))
                Fail(report, "CAM-001", "fatal", "Production Blueprint has a null cameraProfile.", relative, "Create and assign a CameraProfile asset.");
            else
                Pass(report, "CAM-001", "camera_profile", relative);

            if (!Regex.IsMatch(contents, @"^\s*lightingProfile:\s*", RegexOptions.Multiline))
                Fail(report, "LIGHT-001", "error", "Blueprint does not serialize a lightingProfile field.", relative, "Assign a LightingProfile or update the contract after verifying the runtime path.");
            else if (Regex.IsMatch(contents, @"^\s*lightingProfile:\s*\{fileID:\s*0\s*\}", RegexOptions.Multiline))
                Fail(report, "LIGHT-001", "error", "Production Blueprint has a null lightingProfile.", relative, "Create and assign a LightingProfile asset.");
            else
                Pass(report, "LIGHT-001", "lighting_profile", relative);

            int playerStartCount = Regex.Matches(contents, @"^\s*type:\s*7\s*$", RegexOptions.Multiline).Count;
            int exitCount = Regex.Matches(contents, @"^\s*type:\s*(?:6|13)\s*$", RegexOptions.Multiline).Count;
            if (playerStartCount != 1)
                Fail(report, "BLU-001", "fatal", $"Expected exactly one PlayerStart (type 7), found {playerStartCount}.", relative, "Fix PlayerStart module count.");
            else
                Pass(report, "BLU-001", "player_start", relative);

            if (exitCount < 1)
                Fail(report, "BLU-001", "fatal", "Expected at least one LevelExit or LevelGoal module (type 6 or 13).", relative, "Add a valid exit module.");
            else
                Pass(report, "BLU-001", "level_exit", relative);

            Match duration = Regex.Match(contents, @"^\s*maxRecordSeconds:\s*([0-9.]+)", RegexOptions.Multiline);
            if (!duration.Success || !float.TryParse(duration.Groups[1].Value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float seconds) || seconds < 1f || seconds > 30f)
                Fail(report, "ECHO-002", "error", "Blueprint maxRecordSeconds is missing or outside 1..30 seconds.", relative, "Set a canonical per-level record duration.");
            else
                Pass(report, "ECHO-002", "echo_duration", relative);
        }
    }

    private static void Pass(Report report, string ruleId, string message, string evidence)
    {
        report.summary.passed++;
        report.checks.Add(new Check
        {
            rule_id = ruleId,
            status = "passed",
            severity = "advisory",
            message = message,
            evidence = new[] { evidence },
            remediation = string.Empty
        });
    }

    private static void Fail(Report report, string ruleId, string severity, string message, string evidence, string remediation)
    {
        if (severity == "fatal") report.summary.fatal++;
        else if (severity == "error") report.summary.errors++;
        else report.summary.warnings++;

        report.checks.Add(new Check
        {
            rule_id = ruleId,
            status = "failed",
            severity = severity,
            message = message,
            evidence = new[] { evidence },
            remediation = remediation
        });
    }

    private static string ToProjectPath(string projectRoot, string absolutePath)
    {
        return absolutePath.Substring(projectRoot.Length + 1).Replace('\\', '/');
    }
}
#endif
