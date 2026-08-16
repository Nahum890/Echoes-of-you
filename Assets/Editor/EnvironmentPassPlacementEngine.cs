using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Echoes.EnvironmentPass
{
    public static class EnvironmentPassPlacementEngine
    {
        const string PROPS_CONTAINER = "--- PROPS ---";
        const string ENV_CONTAINER = "--- ENVIRONMENT ---";
        const string MECH_CONTAINER = "--- MECHANICS ---";
        const string PROP_TAG = "EnvironmentPassProp";
        const float DEFAULT_CLEARANCE = 1.5f;
        const float RAYCAST_DOWN_DIST = 10f;
        const float AUTO_OFFSET_MAX_RADIUS = 4f;

        private struct PlacedPropInfo
        {
            public Vector3 position;
            public PropSize size;
        }

        private static float GetMinDistance(PropSize sizeA, PropSize sizeB)
        {
            if (sizeA == PropSize.Dominant || sizeB == PropSize.Dominant)
            {
                if (sizeA == PropSize.Dominant && sizeB == PropSize.Dominant) return 1.5f;
                if (sizeA == PropSize.Small || sizeB == PropSize.Small) return 0.8f;
                return 1.2f;
            }
            if (sizeA == PropSize.Medium || sizeB == PropSize.Medium)
            {
                if (sizeA == PropSize.Medium && sizeB == PropSize.Medium) return 1.0f;
                return 0.7f;
            }
            return 0.5f;
        }

        public static PlacementResult PlaceLevel(LevelDataSO levelData, bool dryRun = false)
        {
            var result = new PlacementResult { levelName = levelData.levelName };

            if (!dryRun)
            {
                EditorSceneManager.OpenScene(levelData.scenePath, OpenSceneMode.Single);
                CleanPreviousProps();
            }

            GameObject propsRoot = dryRun ? null : GetOrCreatePropsContainer();
            var exclusionZones = GatherPuzzleExclusionZones();
            if (!dryRun) Debug.Log($"[EnvPass]   {exclusionZones.Count} exclusion zones detected");

            foreach (var roomData in levelData.rooms)
            {
                if (roomData == null) continue;
                var roomResult = PlaceRoom(roomData, propsRoot, exclusionZones, dryRun, levelData.levelNumber);
                result.roomResults.Add(roomResult);
                if (!roomResult.success) result.success = false;
            }

            if (!dryRun && result.success)
            {
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                Debug.Log($"[EnvPass]   ✅ {levelData.levelName} saved ({result.totalProps} props)");
            }

            return result;
        }

        private static RoomPlacementResult PlaceRoom(RoomDataSO roomData, GameObject propsRoot,
                                                      List<Vector3> exclusionZones, bool dryRun, int levelNumber)
        {
            var result = new RoomPlacementResult { roomId = roomData.roomId };

            GameObject zoneContainer = FindZoneContainer(roomData.roomId);
            if (zoneContainer == null)
            {
                result.success = false;
                result.errors.Add($"Zone '{roomData.roomId}' not found in scene");
                return result;
            }

            Bounds validBounds = ComputeValidBounds(zoneContainer);
            GameObject roomContainer = null;
            if (!dryRun)
            {
                roomContainer = new GameObject($"[PROPS] {roomData.roomId}");
                roomContainer.transform.SetParent(propsRoot.transform, false);
            }

            var placedPositions = new List<PlacedPropInfo>();
            var orderedProps = roomData.placements
                .Where(p => p != null)
                .OrderByDescending(p => (int)p.size)
                .ToList();

            foreach (var prop in orderedProps)
            {
                var propResult = TryPlaceProp(prop, zoneContainer, roomContainer, validBounds,
                                             exclusionZones, placedPositions, dryRun);
                result.propResults.Add(propResult);
                if (propResult.success && propResult.position != Vector3.zero)
                {
                    placedPositions.Add(new PlacedPropInfo { position = propResult.position, size = prop.size });
                }
            }

            foreach (var decal in roomData.decals)
            {
                if (decal == null) continue;
                var decalResult = TryPlaceProp(decal, zoneContainer, roomContainer, validBounds,
                                              exclusionZones, placedPositions, dryRun, isDecal: true);
                result.propResults.Add(decalResult);
            }

            if (roomData.validateRequiredProps && !dryRun)
                ValidateRequiredProps(roomData, result);

            if (!dryRun)
            {
                SpawnLocalRoomLight(roomContainer, validBounds, levelNumber);
            }

            return result;
        }

        private static PropPlacementResult TryPlaceProp(PropPlacementSO prop, GameObject zoneContainer,
                                                        GameObject roomContainer, Bounds validBounds,
                                                        List<Vector3> exclusionZones, List<PlacedPropInfo> placedPositions,
                                                        bool dryRun, bool isDecal = false)
        {
            var result = new PropPlacementResult { prefabName = prop.prefabName };

            string folder = isDecal ? "Assets/Prefabs/Decals/" : "Assets/Prefabs/Props/";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(folder + prop.prefabName + ".prefab");
            if (prefab == null)
            {
                result.success = false;
                result.error = $"Prefab not found: {folder}{prop.prefabName}.prefab";
                return result;
            }

            Vector3 worldPos = zoneContainer.transform.TransformPoint(prop.localPosition);

            if (Physics.Raycast(worldPos + Vector3.up * 5f, Vector3.down, out RaycastHit hit,
                                RAYCAST_DOWN_DIST, LayerMask.GetMask("Default")))
            {
                worldPos.y = hit.point.y + (isDecal ? 0.01f : 0f);
            }

            if (!isDecal)
            {
                if (!ValidateClearance(worldPos, prop.minClearanceFromPuzzle, exclusionZones,
                                       placedPositions, prop.size, out string error))
                {
                    if (TryAutoOffset(ref worldPos, prop.minClearanceFromPuzzle, exclusionZones,
                                      placedPositions, prop.size, validBounds))
                    {
                        result.warning = $"Auto-offset applied: {error}";
                    }
                    else
                    {
                        result.success = false;
                        result.error = error;
                        return result;
                    }
                }
            }

            if (!validBounds.Contains(worldPos))
            {
                worldPos = ClampToBounds(worldPos, validBounds);
                result.warning = "Clamped to zone bounds";
            }

            if (!dryRun)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, roomContainer.transform);
                instance.transform.position = worldPos;
                instance.transform.rotation = Quaternion.Euler(prop.localRotationEuler);
                instance.transform.localScale = prop.scale;
                instance.name = $"{prop.prefabName}_{roomContainer.name}";

                try { instance.tag = PROP_TAG; } catch (System.Exception) { /* tag may not exist yet */ }

                if (prop.materialOverride != null)
                    ApplySharedMaterial(instance, prop.materialOverride);

                Undo.RegisterCreatedObjectUndo(instance, $"EnvPass Place {prop.prefabName}");
            }

            result.success = true;
            result.position = worldPos;
            result.rotation = prop.localRotationEuler;
            result.scale = prop.scale;
            return result;
        }

        private static bool ValidateClearance(Vector3 pos, float minClearance, List<Vector3> exclusionZones,
                                              List<PlacedPropInfo> placedPositions, PropSize currentSize, out string error)
        {
            error = "";
            Vector2 pos2D = new(pos.x, pos.z);

            foreach (var ez in exclusionZones)
            {
                if (Vector2.Distance(pos2D, new Vector2(ez.x, ez.z)) < minClearance)
                {
                    error = $"Too close to puzzle object at {ez}";
                    return false;
                }
            }

            foreach (var pp in placedPositions)
            {
                float minSeparation = GetMinDistance(currentSize, pp.size);
                if (Vector2.Distance(pos2D, new Vector2(pp.position.x, pp.position.z)) < minSeparation)
                {
                    error = $"Overlap with previously placed prop ({pp.size}) at {pp.position}";
                    return false;
                }
            }

            return true;
        }

        private static bool TryAutoOffset(ref Vector3 pos, float minClearance, List<Vector3> exclusionZones,
                                          List<PlacedPropInfo> placedPositions, PropSize currentSize, Bounds bounds)
        {
            for (float r = 0.5f; r <= AUTO_OFFSET_MAX_RADIUS; r += 0.5f)
            {
                for (int i = 0; i < 16; i++)
                {
                    float ang = i * Mathf.PI * 2f / 16f;
                    Vector3 candidate = pos + new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang)) * r;
                    candidate = ClampToBounds(candidate, bounds);

                    if (ValidateClearance(candidate, minClearance, exclusionZones, placedPositions, currentSize, out _))
                    {
                        pos = candidate;
                        return true;
                    }
                }
            }
            return false;
        }

        private static Vector3 ClampToBounds(Vector3 pos, Bounds bounds)
        {
            return new Vector3(
                Mathf.Clamp(pos.x, bounds.min.x, bounds.max.x),
                pos.y,
                Mathf.Clamp(pos.z, bounds.min.z, bounds.max.z)
            );
        }

        private static Bounds ComputeValidBounds(GameObject zoneContainer)
        {
            Vector3 center = zoneContainer.transform.position;
            float maxDist = 50f;
            var bounds = new Bounds(center, Vector3.zero);

            for (int i = 0; i < 8; i++)
            {
                float ang = i * Mathf.PI / 4f;
                Vector3 dir = new(Mathf.Cos(ang), 0, Mathf.Sin(ang));
                if (Physics.Raycast(center + Vector3.up, dir, out RaycastHit hit, maxDist,
                                    LayerMask.GetMask("Default")))
                    bounds.Encapsulate(hit.point);
                else
                    bounds.Encapsulate(center + dir * maxDist);
            }

            bounds.Expand(new Vector3(0, 5f, 0));
            return bounds;
        }

        private static List<Vector3> GatherPuzzleExclusionZones()
        {
            var result = new List<Vector3>();
            GameObject mechRoot = GameObject.Find(MECH_CONTAINER);
            if (mechRoot == null) return result;

            foreach (Transform child in mechRoot.GetComponentsInChildren<Transform>(true))
            {
                if (child == mechRoot.transform) continue;
                if (IsPuzzleObject(child)) result.Add(child.position);
            }
            return result;
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

        private static GameObject FindZoneContainer(string roomId)
        {
            foreach (string containerName in new[] { ENV_CONTAINER, MECH_CONTAINER })
            {
                GameObject root = GameObject.Find(containerName);
                if (root != null)
                {
                    Transform found = root.transform.Find(roomId);
                    if (found != null) return found.gameObject;
                }
            }
            return null;
        }

        private static GameObject GetOrCreatePropsContainer()
        {
            GameObject existing = GameObject.Find(PROPS_CONTAINER);
            if (existing != null)
            {
                for (int i = existing.transform.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(existing.transform.GetChild(i).gameObject);
                return existing;
            }
            GameObject go = new(PROPS_CONTAINER);
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            return go;
        }

        private static void CleanPreviousProps()
        {
            GameObject existing = GameObject.Find(PROPS_CONTAINER);
            if (existing != null)
                Object.DestroyImmediate(existing);
        }

        private static void ApplySharedMaterial(GameObject go, Material mat)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers) r.sharedMaterial = mat;
        }

        private static void ValidateRequiredProps(RoomDataSO roomData, RoomPlacementResult result)
        {
            var required = roomData.GetRequiredPropsForType();
            foreach (var req in required)
            {
                bool found = roomData.placements.Any(p => p != null && p.prefabName == req);
                if (!found)
                    result.warnings.Add($"Missing required prop for {roomData.roomType}: {req}");
            }

            int small = roomData.placements.Count(p => p != null && p.size == PropSize.Small);
            int med = roomData.placements.Count(p => p != null && p.size == PropSize.Medium);
            int dom = roomData.placements.Count(p => p != null && p.size == PropSize.Dominant);

            if (small < 5) result.warnings.Add($"Small props: {small} (min 5)");
            if (med < 2) result.warnings.Add($"Medium props: {med} (min 2)");
            if (dom < 1) result.warnings.Add($"Dominant props: {dom} (min 1)");
        }

        private static void SpawnLocalRoomLight(GameObject roomContainer, Bounds bounds, int levelNumber)
        {
            GameObject lightGo = new GameObject("RoomLocalLight");
            lightGo.transform.SetParent(roomContainer.transform, false);
            // Position the light at the center of the room, 3 meters above the floor level
            lightGo.transform.position = new Vector3(bounds.center.x, bounds.center.y + 3.0f, bounds.center.z);

            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.shadows = LightShadows.Hard;

            Color lightColor = Color.white;
            float intensity = 1.5f;
            float range = 12f;
            bool shouldFlicker = false;

            if (levelNumber <= 3) // Chapter 1: sick green/yellow fluorescent
            {
                ColorUtility.TryParseHtmlString("#C9D4B0", out lightColor);
                intensity = 1.8f;
                range = 14f;
                if (levelNumber == 1 || levelNumber == 2) shouldFlicker = true;
            }
            else if (levelNumber <= 5) // Chapter 2: mustard yellow
            {
                ColorUtility.TryParseHtmlString("#D8B262", out lightColor);
                intensity = 1.6f;
                range = 12f;
            }
            else if (levelNumber <= 7) // Chapter 3: cold blue
            {
                ColorUtility.TryParseHtmlString("#A4C2E0", out lightColor);
                intensity = 1.4f;
                range = 12f;
            }
            else if (levelNumber == 9) // Chapter 4: patio (sunlight only, dim light)
            {
                ColorUtility.TryParseHtmlString("#E8DAB2", out lightColor);
                intensity = 0.5f;
                range = 8f;
            }
            else if (levelNumber <= 11) // Chapter 4 (part 2) & Chapter 5: amber
            {
                ColorUtility.TryParseHtmlString("#FFBF00", out lightColor);
                intensity = 1.6f;
                range = 14f;
            }
            else if (levelNumber <= 13) // Chapter 5 & Chapter 6: red
            {
                ColorUtility.TryParseHtmlString("#B23A3A", out lightColor);
                intensity = 2.0f;
                range = 10f;
                if (levelNumber == 13) shouldFlicker = true;
            }
            else if (levelNumber == 14) // Chapter 6: void islands
            {
                ColorUtility.TryParseHtmlString("#8A9BB8", out lightColor);
                intensity = 2.5f;
                range = 10f;
            }
            else if (levelNumber == 15) // Level 15: integration
            {
                ColorUtility.TryParseHtmlString("#C9D4B0", out lightColor);
                intensity = 1.8f;
                range = 14f;
            }

            light.color = lightColor;
            light.intensity = intensity;
            light.range = range;

            if (shouldFlicker)
            {
                LightFlicker flicker = lightGo.AddComponent<LightFlicker>();
                flicker.baseIntensity = intensity;
                flicker.minIntensity = 0.2f;
                flicker.maxIntensity = 1.2f;
                flicker.flickerSpeed = 0.08f;
            }
        }
    }

    public class PlacementResult
    {
        public string levelName;
        public bool success = true;
        public int totalProps => roomResults.Sum(r => r.propResults.Count(p => p.success));
        public List<RoomPlacementResult> roomResults = new();
    }

    public class RoomPlacementResult
    {
        public string roomId;
        public bool success = true;
        public List<PropPlacementResult> propResults = new();
        public List<string> errors = new();
        public List<string> warnings = new();
    }

    public class PropPlacementResult
    {
        public string prefabName;
        public bool success;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public string error;
        public string warning;
    }
}