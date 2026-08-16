using UnityEngine;

namespace Echoes.Narrative.Data
{
    [System.Serializable]
    public class MemoryEffect
    {
        [Header("Memory Identity")]
        [SerializeField] string _memoryId = "";
        [SerializeField] bool _isLyraArtifact = false;

        [Header("Flags")]
        [SerializeField] string[] _flagsAdded = System.Array.Empty<string>();

        [Header("Variables")]
        [SerializeField] VariableChange[] _variableChanges = System.Array.Empty<VariableChange>();

        [Header("Comprehension")]
        [SerializeField] int _comprehensionDelta = 0;

        public string MemoryId => _memoryId;
        public bool IsLyraArtifact => _isLyraArtifact;
        public string[] FlagsAdded => _flagsAdded;
        public VariableChange[] VariableChanges => _variableChanges;
        public int ComprehensionDelta => _comprehensionDelta;
    }
}
