using UnityEngine;

namespace Echoes.Narrative.Data
{
    public enum FlagCategory
    {
        Openness,
        PatternHolding,
        Exit,
        Memory,
        Custom
    }

    [System.Serializable]
    public class FlagDef
    {
        [SerializeField] string _flagName = "";
        [SerializeField] FlagCategory _category = FlagCategory.Custom;
        [SerializeField] int _comprehensionDelta = 0;
        [SerializeField, TextArea] string _description = "";

        public string FlagName => _flagName;
        public FlagCategory Category => _category;
        public int ComprehensionDelta => _comprehensionDelta;
        public string Description => _description;
    }

    [CreateAssetMenu(fileName = "NarrativeFlagCatalog", menuName = "Echoes/Narrative/FlagCatalog", order = 2)]
    public class NarrativeFlagCatalog : ScriptableObject
    {
        [SerializeField] FlagDef[] _flags = System.Array.Empty<FlagDef>();

        public FlagDef[] Flags => _flags;

        public FlagDef FindFlag(string flagName)
        {
            if (_flags == null) return null;
            for (int i = 0; i < _flags.Length; i++)
            {
                if (_flags[i] != null && _flags[i].FlagName == flagName)
                    return _flags[i];
            }
            return null;
        }

        public bool IsValidFlag(string flagName)
        {
            return FindFlag(flagName) != null;
        }
    }
}
