using UnityEngine;
using UnityEngine.Events;
using Echoes.Narrative;
using Echoes.Narrative.Data;

namespace Echoes.Interaction
{
    public class InteractableObject : MonoBehaviour
    {
        [Header("Categoría Contextual (SPEC interacción contextual)")]
        [Tooltip("A=Gameplay · B=Narrative · C=Ambient · D=Decoration (nunca lleva componente)")]
        [SerializeField] InteractableCategory _category = InteractableCategory.Narrative;
        [Tooltip("Oculta el prompt si no hay línea de visión directa al objeto.")]
        [SerializeField] bool _requireLineOfSight = true;

        [Header("Acción al Interactuar (Gameplay / Ambient)")]
        [SerializeField] UnityEvent _onInteracted = new UnityEvent();

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
        public InteractableCategory Category => _category;
        public bool RequireLineOfSight => _requireLineOfSight;
        public UnityEvent OnInteracted => _onInteracted;

        /// <summary>
        /// Configuración programática (usada por spawners en runtime como
        /// LevelEnvironmentBootstrap). Evita depender de reflexión en campos privados.
        /// </summary>
        public void SetContext(string displayName, string commentKey, bool isLyraArtifact, float triggerRadius, InteractableCategory category)
        {
            this.displayName = displayName;
            this.commentKey = commentKey;
            this.isLyraArtifact = isLyraArtifact;
            this.triggerRadius = triggerRadius;
            _category = category;
            EnsureTrigger();
        }

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
        public string PromptText
        {
            get
            {
                if (_narrativeData != null && !string.IsNullOrEmpty(_narrativeData.PromptText))
                    return _narrativeData.PromptText;
                return _category switch
                {
                    InteractableCategory.Gameplay => "Usar",
                    InteractableCategory.Ambient => "Observar",
                    _ => "Examinar"
                };
            }
        }
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
            if (_onInteracted == null)
                _onInteracted = new UnityEvent();
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
            // Trigger de proximidad SEPARADO: no convertimos los colliders físicos del
            // prop (normalmente en hijos o en el mismo GO) en triggers, para que los
            // objetos sigan sólidos y el prompt solo aparezca al acercarse de verdad.
            var sphere = GetComponent<SphereCollider>();
            if (sphere == null)
                sphere = gameObject.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = Mathf.Max(0.5f, triggerRadius * 0.5f);
        }
    }
}
