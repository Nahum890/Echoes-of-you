using UnityEngine;

namespace Echoes.Narrative.Data
{
    [CreateAssetMenu(fileName = "InteractableData", menuName = "Echoes/Narrative/InteractableData", order = 0)]
    public class InteractableData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] string _interactableId = "";
        [SerializeField] string _levelId = "";
        [SerializeField] InteractionType _interactionType = InteractionType.Inspect;

        [Header("Prompt")]
        [SerializeField] string _promptText = "Examinar";

        [Header("Dialogue / Inspect")]
        [SerializeField] string _commentKey = "interaction.default";
        [SerializeField] string _dialogueId = "";
        [SerializeField] string _displayName = "Objeto";

        [Header("Effects")]
        [SerializeField] MemoryEffect _memoryEffect;
        [SerializeField] DialogueChoice _choiceEffect;
        [SerializeField] bool _oneTimeOnly = false;
        [SerializeField] VisualStateChange _visualStateAfter;

        [Header("Legacy Compat")]
        [SerializeField] bool _isLyraArtifact = false;
        [SerializeField] bool _requireEchoActive = false;
        [SerializeField] float _cooldown = 3.0f;
        [SerializeField] float _triggerRadius = 2.5f;

        public string InteractableId => _interactableId;
        public string LevelId => _levelId;
        public InteractionType InteractionType => _interactionType;
        public string PromptText => _promptText;
        public string CommentKey => _commentKey;
        public string DialogueId => _dialogueId;
        public string DisplayName => _displayName;
        public MemoryEffect MemoryEffect => _memoryEffect;
        public DialogueChoice ChoiceEffect => _choiceEffect;
        public bool OneTimeOnly => _oneTimeOnly;
        public VisualStateChange VisualStateAfter => _visualStateAfter;
        public bool IsLyraArtifact => _isLyraArtifact;
        public bool RequireEchoActive => _requireEchoActive;
        public float Cooldown => _cooldown;
        public float TriggerRadius => _triggerRadius;
    }
}
