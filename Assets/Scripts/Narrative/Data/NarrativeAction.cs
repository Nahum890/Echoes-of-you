using UnityEngine;

namespace Echoes.Narrative.Data
{
    [System.Serializable]
    public class NarrativeAction
    {
        [SerializeField] NarrativeActionType _type;
        [SerializeField] string _target = "";
        [SerializeField] string _value = "";

        public NarrativeActionType Type => _type;
        public string Target => _target;
        public string Value => _value;
    }

    public enum NarrativeActionType
    {
        SetFlag,
        ClearFlag,
        SetVariable,
        LoadScene,
        ShowMemory,
        PlaySound,
        SetVisualState
    }
}
