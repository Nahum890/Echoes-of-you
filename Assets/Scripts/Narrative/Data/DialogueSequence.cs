using UnityEngine;

namespace Echoes.Narrative.Data
{
    [CreateAssetMenu(fileName = "DialogueSequence", menuName = "Echoes/Narrative/DialogueSequence", order = 1)]
    public class DialogueSequence : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] string _dialogueId = "";
        [SerializeField] string _levelId = "";

        [Header("Trigger")]
        [SerializeField] string _triggerCondition = "";
        [SerializeField] bool _oneTimeOnly = false;

        [Header("Nodes")]
        [SerializeField] DialogueNode[] _nodes = System.Array.Empty<DialogueNode>();

        [Header("On Complete")]
        [SerializeField] NarrativeAction[] _onCompleteActions = System.Array.Empty<NarrativeAction>();

        public string DialogueId => _dialogueId;
        public string LevelId => _levelId;
        public string TriggerCondition => _triggerCondition;
        public bool OneTimeOnly => _oneTimeOnly;
        public DialogueNode[] Nodes => _nodes;
        public NarrativeAction[] OnCompleteActions => _onCompleteActions;

        public DialogueNode FindNode(string nodeId)
        {
            if (_nodes == null) return null;
            for (int i = 0; i < _nodes.Length; i++)
            {
                if (_nodes[i] != null && _nodes[i].NodeId == nodeId)
                    return _nodes[i];
            }
            return null;
        }
    }
}
