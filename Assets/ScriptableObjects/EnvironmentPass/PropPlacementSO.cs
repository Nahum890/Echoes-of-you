using System.Collections.Generic;
using UnityEngine;

namespace Echoes.EnvironmentPass
{
    [CreateAssetMenu(menuName = "Echoes/Environment Pass/Prop Placement", fileName = "PropPlacement_")]
    public class PropPlacementSO : ScriptableObject
    {
        [Header("Identity")]
        public string prefabName;
        public NarrativeTag narrativeTag = NarrativeTag.None;

        [Header("Transform (relative to room center)")]
        public Vector3 localPosition;
        public Vector3 localRotationEuler;
        public Vector3 scale = Vector3.one;

        [Header("Classification")]
        public PropSize size = PropSize.Small;
        public Material materialOverride;

        [Header("Validation")]
        public bool requiredForRoomType;
        public float minClearanceFromPuzzle = 1.5f;
    }
}