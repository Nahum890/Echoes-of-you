using UnityEngine;

namespace Echoes.Interaction
{
    public class InteractableObject : MonoBehaviour
    {
        [SerializeField] string commentKey = "interaction.default";
        [SerializeField] bool isLyraArtifact = false;
        [SerializeField] bool requireEchoActive = false;
        [SerializeField] float cooldown = 3.0f;
        [SerializeField] float triggerRadius = 2.5f;
        [SerializeField] string displayName = "Objeto";

        public string CommentKey => commentKey;
        public bool IsLyraArtifact => isLyraArtifact;
        public bool RequireEchoActive => requireEchoActive;
        public float Cooldown => cooldown;
        public float TriggerRadius => triggerRadius;
        public string DisplayName => displayName;

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
