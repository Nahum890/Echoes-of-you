using System.Collections.Generic;
using UnityEngine;

namespace Echoes.EnvironmentPass
{
    [CreateAssetMenu(menuName = "Echoes/Environment Pass/Narrative Cluster", fileName = "NarrativeCluster_")]
    public class NarrativeClusterSO : ScriptableObject
    {
        public int levelNumber;
        public string clusterName;
        public List<string> requiredPrefabNames = new();
        public List<string> forbiddenMaterials = new();
        public Material requiredMaterial;
        public NarrativeTag tag = NarrativeTag.Lyra;

        public bool Validate(RoomDataSO room, out string error)
        {
            error = "";
            if (requiredPrefabNames == null) return true;

            foreach (var req in requiredPrefabNames)
            {
                if (string.IsNullOrEmpty(req)) continue;
                bool found = room.placements.Exists(p => p != null && p.prefabName == req)
                          || room.decals.Exists(d => d != null && d.prefabName == req);
                if (!found) { error = $"Missing required narrative prop: {req}"; return false; }
            }

            if (requiredMaterial != null)
            {
                bool hasMat = room.placements.Exists(p => p != null && p.materialOverride == requiredMaterial);
                if (!hasMat) { error = $"Missing required narrative material: {requiredMaterial.name}"; return false; }
            }

            if (forbiddenMaterials != null)
            {
                foreach (var forbid in forbiddenMaterials)
                {
                    if (string.IsNullOrEmpty(forbid)) continue;
                    bool hasForbidden = room.placements.Exists(p => p != null && p.materialOverride != null && p.materialOverride.name == forbid);
                    if (hasForbidden) { error = $"Forbidden material found in this level: {forbid}"; return false; }
                }
            }

            return true;
        }
    }
}