using UnityEngine;
using Echoes.Narrative;
using Echoes.Narrative.Data;

namespace Echoes.Interaction
{
    public class InteractableObject : MonoBehaviour
    {
        [Header("Narrative Data (optional — overrides legacy fields)")]
        [SerializeField] InteractableData _narrativeData;

        [Header("Legacy Fields (used when NarrativeData is null)")]
        [SerializeField] string commentKey = "interaction.default";
        [SerializeField] bool isLyraArtifact = false;
        [SerializeField] bool requireEchoActive = false;
        [SerializeField] float cooldown = 3.0f;
        [SerializeField] float triggerRadius = 4.0f;
        [SerializeField] string displayName = "Objeto";

        DialogueTrigger _triggerCache;
        bool _triggerCacheDone;

        public InteractableData NarrativeData => _narrativeData;

        public DialogueTrigger Trigger
        {
            get
            {
                if (!_triggerCacheDone)
                {
                    _triggerCache = GetComponent<DialogueTrigger>();
                    _triggerCacheDone = true;
                }
                return _triggerCache;
            }
        }

        public string CommentKey => _narrativeData != null ? _narrativeData.CommentKey : commentKey;
        public bool IsLyraArtifact => _narrativeData != null ? _narrativeData.IsLyraArtifact : isLyraArtifact;
        public bool RequireEchoActive => _narrativeData != null ? _narrativeData.RequireEchoActive : requireEchoActive;
        public float Cooldown => _narrativeData != null ? _narrativeData.Cooldown : cooldown;
        public float TriggerRadius => _narrativeData != null ? _narrativeData.TriggerRadius : triggerRadius;
        public string DisplayName => _narrativeData != null ? _narrativeData.DisplayName : displayName;
        public string InteractableId => _narrativeData != null ? _narrativeData.InteractableId : "";
        public InteractionType InteractionType => _narrativeData != null ? _narrativeData.InteractionType : InteractionType.Inspect;
        public string PromptText => _narrativeData != null ? _narrativeData.PromptText : "Examinar";
        public bool OneTimeOnly => _narrativeData != null && _narrativeData.OneTimeOnly;

        public bool IsConsumed
        {
            get
            {
                if (Trigger != null) return Trigger.IsConsumed;
                if (OneTimeOnly && MemorySystem.Instance != null && !string.IsNullOrEmpty(InteractableId))
                    return MemorySystem.Instance.HasBeenInspected(InteractableId);
                return false;
            }
        }

        void Reset()
        {
            EnsureTrigger();
        }

        void Awake()
        {
            EnsureTrigger();
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (InteractionSystem.Instance == null) return;
            InteractionSystem.Instance.Register(this);
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (InteractionSystem.Instance == null) return;
            InteractionSystem.Instance.Unregister(this);
        }

        void EnsureTrigger()
        {
            var col = GetComponent<Collider>();
            if (col == null) col = gameObject.AddComponent<SphereCollider>();
            if (col is SphereCollider s) s.radius = triggerRadius * 0.5f;
            col.isTrigger = true;
        }
    }
}
