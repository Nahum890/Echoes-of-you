using UnityEngine;

namespace Echoes.Narrative.Data
{
    [System.Serializable]
    public class VariableChange
    {
        [SerializeField] string _variableName = "";
        [SerializeField] VariableOperation _operation = VariableOperation.Set;
        [SerializeField] float _value = 0f;

        public string VariableName => _variableName;
        public VariableOperation Operation => _operation;
        public float Value => _value;

        public float Apply(float current)
        {
            return _operation switch
            {
                VariableOperation.Set => _value,
                VariableOperation.Add => current + _value,
                VariableOperation.Subtract => current - _value,
                VariableOperation.Multiply => current * _value,
                _ => current
            };
        }
    }

    public enum VariableOperation
    {
        Set,
        Add,
        Subtract,
        Multiply
    }
}
