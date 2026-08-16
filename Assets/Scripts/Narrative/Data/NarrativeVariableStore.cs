using UnityEngine;

namespace Echoes.Narrative.Data
{
    public enum VariableType
    {
        Int,
        Float,
        String,
        Bool
    }

    [System.Serializable]
    public class VariableDef
    {
        [SerializeField] string _name = "";
        [SerializeField] VariableType _type = VariableType.Int;
        [SerializeField] string _defaultValue = "0";
        [SerializeField, TextArea] string _description = "";

        public string Name => _name;
        public VariableType Type => _type;
        public string DefaultValue => _defaultValue;
        public string Description => _description;

        public float DefaultAsFloat
        {
            get
            {
                if (float.TryParse(_defaultValue, out var f)) return f;
                return 0f;
            }
        }
    }

    [CreateAssetMenu(fileName = "NarrativeVariableStore", menuName = "Echoes/Narrative/VariableStore", order = 3)]
    public class NarrativeVariableStore : ScriptableObject
    {
        [SerializeField] VariableDef[] _variables = System.Array.Empty<VariableDef>();

        public VariableDef[] Variables => _variables;

        public VariableDef FindVariable(string name)
        {
            if (_variables == null) return null;
            for (int i = 0; i < _variables.Length; i++)
            {
                if (_variables[i] != null && _variables[i].Name == name)
                    return _variables[i];
            }
            return null;
        }

        public float GetDefault(string name)
        {
            var def = FindVariable(name);
            return def != null ? def.DefaultAsFloat : 0f;
        }
    }
}
