using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Echoes.EnvironmentPass
{
    public static class EnvironmentPassValidator
    {
        const string PROPS_CONTAINER = "--- PROPS ---";
        const string MECH_CONTAINER = "--- MECHANICS ---";
        const float MIN_CLEARANCE = 1.5f;

        public static ValidationReport ValidateActiveScene()
        {
            var report = new ValidationReport { sceneName = SceneManager.GetActiveScene().name };

            GameObject propsRoot = GameObject.Find(PROPS_CONTAINER);
            if (propsRoot == null)
            {
                report.criticalErrors.Add("--- PROPS --- container not found. Run PlaceAll first.");
                return report;
            }

            var exclusionZones = GatherPuzzleExclusionZones();

            foreach (Transform roomTr in propsRoot.transform)
            {
                var roomReport = ValidateRoom(roomTr, exclusionZones);
                report.roomReports.Add(roomReport);
                report.totalViolations += roomReport.violations.Count;
                report.totalWarnings += roomReport.warnings.Count;
            }

            CheckColorDistribution(propsRoot, report);
            report.totalWarnings += report.warnings.Count;

            report.passed = report.totalViolations == 0;
            LogReport(report);
            return report;
        }

        private static RoomValidationReport ValidateRoom(Transform roomTr, List<Vector3> exclusionZones)
        {
            var report = new RoomValidationReport { roomId = roomTr.name };
            var propPositions = new List<(string name, Vector3 pos, bool isDecal)>();

            int small = 0, medium = 0, dominant = 0;

            foreach (Transform child in roomTr)
            {
                string n = child.name.ToLower();
                if (n.Contains("light")) continue;

                if (n.Contains("estanteria") || n.Contains("pizarra") || n.Contains("mesa") ||
                    n.Contains("pupitre") || n.Contains("locker"))
                    dominant++;
                else if (n.Contains("silla") || n.Contains("bancomadera") || n.Contains("escritorio") ||
                         n.Contains("carritoconserje"))
                    medium++;
                else
                    small++;

                propPositions.Add((child.name, child.position, n.StartsWith("dec_") || n.Contains("decal")));
            }

            bool isMinimalRoom = roomTr.name.ToLower().Contains("entrada") || roomTr.name.ToLower().Contains("salida") || roomTr.name.ToLower().Contains("ausente");
            if (!isMinimalRoom)
            {
                if (dominant < 1 && medium < 1) report.warnings.Add($"Room has low architectural furniture ({dominant} dominant, {medium} medium)");
            }

            for (int i = 0; i < propPositions.Count; i++)
            for (int j = i + 1; j < propPositions.Count; j++)
            {
                if (propPositions[i].isDecal || propPositions[j].isDecal) continue;

                float dist = Vector2.Distance(
                    new Vector2(propPositions[i].pos.x, propPositions[i].pos.z),
                    new Vector2(propPositions[j].pos.x, propPositions[j].pos.z));
                if (dist < 0.25f)
                    report.violations.Add($"Overlap: {propPositions[i].name} <-> {propPositions[j].name} ({dist:F2}m)");
            }

            foreach (var (pname, ppos, isDecal) in propPositions)
            {
                if (isDecal) continue;
                foreach (var ez in exclusionZones)
                {
                    float dist = Vector2.Distance(new Vector2(ppos.x, ppos.z), new Vector2(ez.x, ez.z));
                    if (dist < MIN_CLEARANCE)
                        report.violations.Add($"{pname} at {dist:F2}m from puzzle object (min {MIN_CLEARANCE}m)");
                }
            }

            foreach (var (pname, ppos, _) in propPositions)
            {
                string pn = pname.ToLower();
                bool isWallProp = pn.Contains("cartelera") || pn.Contains("reloj") || pn.Contains("extintor") || 
                                  pn.Contains("cuadro") || pn.Contains("pizarra") || pn.Contains("light") || pn.Contains("decal");

                if (!isWallProp)
                {
                    if (Physics.Raycast(ppos + Vector3.up * 1f, Vector3.down, out RaycastHit hit, 2.5f,
                                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    {
                        if (Mathf.Abs(ppos.y - hit.point.y) > 0.35f)
                            report.warnings.Add($"{pname} floating {Mathf.Abs(ppos.y - hit.point.y):F2}m above floor");
                    }
                }
            }

            return report;
        }

        private static void CheckColorDistribution(GameObject propsRoot, ValidationReport report)
        {
            var renderers = propsRoot.GetComponentsInChildren<Renderer>();
            int total = renderers.Length;
            int amber = 0, inst = 0;

            foreach (var r in renderers)
            {
                if (r.sharedMaterial == null) continue;
                string mName = r.sharedMaterial.name;
                if (mName.Contains("Memory") || mName.Contains("Amber")) amber++;
                else if (mName.Contains("Teal") || mName.Contains("Mustard") || mName.Contains("Sage") || mName.Contains("Rose")) inst++;
            }

            if (total > 0)
            {
                float amberPct = amber * 100f / total;
                if (amberPct > 20f) report.warnings.Add($"memory-amber at {amberPct:F1}% (>20% dilutes narrative meaning)");
            }
        }

        private static List<Vector3> GatherPuzzleExclusionZones()
        {
            var result = new List<Vector3>();
            var allRoots = new[] { "--- MECHANICS ---", "--- PUZZLE ---", "--- PUZZLES ---", "--- INTERACTION ---" };

            foreach (var rootName in allRoots)
            {
                GameObject root = GameObject.Find(rootName);
                if (root == null) continue;

                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                {
                    if (child == root.transform) continue;
                    if (IsPuzzleObject(child)) result.Add(child.position);
                }
            }

            // Also check root level interactive objects
            foreach (var plate in Object.FindObjectsByType<PressurePlate>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                result.Add(plate.transform.position);
            foreach (var door in Object.FindObjectsByType<DoorController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                result.Add(door.transform.position);
            foreach (var exit in Object.FindObjectsByType<LevelExit>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                result.Add(exit.transform.position);

            return result.Distinct().ToList();
        }

        private static bool IsPuzzleObject(Transform t)
        {
            string n = t.name;
            if (n.StartsWith("PressurePlate") || n.StartsWith("Door") || n.StartsWith("Bridge") ||
                n.StartsWith("ResonancePad") || n.StartsWith("LevelExit") || n.StartsWith("EscapeRoute") ||
                n.StartsWith("Platform") || n.StartsWith("GravityZone") || n.StartsWith("ChaseHazard"))
                return true;
            if (t.GetComponent("PressurePlate") != null) return true;
            if (t.GetComponent("ResonanceZoneTrigger") != null) return true;
            if (t.GetComponent("LevelExit") != null) return true;
            if (t.GetComponent("DoorController") != null) return true;
            return false;
        }

        private static void LogReport(ValidationReport report)
        {
            Debug.Log($"═══ VALIDATION: {report.sceneName} ═══");
            Debug.Log($"Rooms: {report.roomReports.Count} | Violations: {report.totalViolations} | Warnings: {report.totalWarnings}");
            Debug.Log($"Result: {(report.passed ? "PASSED" : "FAILED")}");

            foreach (var rr in report.roomReports)
            {
                if (rr.violations.Count > 0 || rr.warnings.Count > 0)
                {
                    Debug.Log($"  {rr.roomId}:");
                    foreach (var v in rr.violations) Debug.LogError($"    {v}");
                    foreach (var w in rr.warnings) Debug.LogWarning($"    {w}");
                }
            }
            foreach (var ce in report.criticalErrors) Debug.LogError($"  CRITICAL: {ce}");
            Debug.Log("════════════════════════════════");
        }
    }

    public class ValidationReport
    {
        public string sceneName;
        public bool passed;
        public int totalViolations;
        public int totalWarnings;
        public List<string> criticalErrors = new();
        public List<string> warnings = new();
        public List<RoomValidationReport> roomReports = new();
    }

    public class RoomValidationReport
    {
        public string roomId;
        public List<string> violations = new();
        public List<string> warnings = new();
    }
}