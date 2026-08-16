using UnityEngine;

namespace Echoes.Narrative.Data
{
    [System.Serializable]
    public class DialogueChoice
    {
        [Header("Identity")]
        [SerializeField] string _choiceId = "";
        [SerializeField] string _displayText = "";
        [SerializeField] ChoiceColor _color = ChoiceColor.Cyan;

        [Header("Conditions")]
        [SerializeField] string[] _conditions = System.Array.Empty<string>();

        [Header("Effects")]
        [SerializeField] NarrativeAction[] _effects = System.Array.Empty<NarrativeAction>();
        [SerializeField] string[] _flagsAdded = System.Array.Empty<string>();
        [SerializeField] string[] _flagsRemoved = System.Array.Empty<string>();
        [SerializeField] VariableChange[] _variableChanges = System.Array.Empty<VariableChange>();

        [Header("Flow")]
        [SerializeField] string _nextNode = "";
        [SerializeField] int _comprehensionDelta = 0;

        public string ChoiceId => _choiceId;
        public string DisplayText => _displayText;
        public ChoiceColor Color => _color;
        public string[] Conditions => _conditions;
        public NarrativeAction[] Effects => _effects;
        public string[] FlagsAdded => _flagsAdded;
        public string[] FlagsRemoved => _flagsRemoved;
        public VariableChange[] VariableChanges => _variableChanges;
        public string NextNode => _nextNode;
        public int ComprehensionDelta => _comprehensionDelta;
    }
}
