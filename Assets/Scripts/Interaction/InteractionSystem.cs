using System.Collections.Generic;
using Echoes.UI;
using Echoes.VN;
using Echoes.Narrative;
using UnityEngine;

namespace Echoes.Interaction
{
    public class InteractionSystem : MonoBehaviour
    {
        public static InteractionSystem Instance { get; private set; }

        [Header("Raycast Settings")]
        [SerializeField] float raycastDistance = 8f;
        [SerializeField] float raycastRadius = 0.5f;
        [SerializeField] LayerMask interactableMask = ~0;

        readonly List<InteractableObject> _nearby = new();
        readonly Dictionary<string, float> _lastSeenAt = new();
        InteractableObject _currentTarget;
        InteractableObject _lastShownTarget;

        public InteractableObject CurrentTarget => _currentTarget;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            if (VN_OverlayController.Instance != null && VN_OverlayController.Instance.IsOpen)
            {
                _currentTarget = null;
                InteractionPromptController.Instance?.HidePrompt();
                return;
            }

            _currentTarget = PickNearest();

            bool hasTarget = _currentTarget != null && !_currentTarget.IsConsumed && IsRelevant(_currentTarget);
            if (hasTarget)
            {
                string promptText = _currentTarget.PromptText;
                bool primary = _currentTarget.IsLyraArtifact && _currentTarget.Category == InteractableCategory.Narrative;
                InteractionPromptController.Instance?.ShowPrompt("[E]", promptText, primary);

                // Anti-spam: el sonido de prompt suena solo al cambiar de objetivo.
                if (_lastShownTarget != _currentTarget)
                {
                    PlayPromptSound();
                    _lastShownTarget = _currentTarget;
                }
            }
            else
            {
                InteractionPromptController.Instance?.HidePrompt();
                _lastShownTarget = null;
                return;
            }

            bool interactPressed = Input.GetKeyDown(KeyCode.E);
            var inputMap = Echoes.InputActionMap.Instance;
            if (inputMap != null)
                interactPressed = inputMap.PlaybackPressed;

            if (!interactPressed) return;
            if (_currentTarget.RequireEchoActive && !IsEchoActive()) return;

            string cooldownKey = _currentTarget.InteractableId;
            if (string.IsNullOrEmpty(cooldownKey))
                cooldownKey = _currentTarget.CommentKey;

            if (_lastSeenAt.TryGetValue(cooldownKey, out var t) &&
                Time.time - t < _currentTarget.Cooldown) return;

            HandleInteraction(_currentTarget);
            _lastSeenAt[cooldownKey] = Time.time;
        }

        InteractableObject PickNearest()
        {
            var player = FindAnyObjectByType<PlayerController>();
            if (player == null) return null;

            Vector3 p = player.transform.position;
            float bestSqr = float.MaxValue;
            InteractableObject best = null;

            for (int i = _nearby.Count - 1; i >= 0; i--)
            {
                var o = _nearby[i];
                if (o == null || !o.gameObject.activeInHierarchy || !o.enabled || o.IsConsumed)
                {
                    _nearby.RemoveAt(i);
                    continue;
                }
                float sqr = (o.transform.position - p).sqrMagnitude;
                float range = Mathf.Max(o.TriggerRadius, 8f);
                range *= range;
                if (sqr <= range && sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = o;
                }
            }

            if (best != null)
                return best;

            return RaycastForInteractable(player);
        }

        InteractableObject RaycastForInteractable(PlayerController player)
        {
            var cam = Camera.main;
            Vector3 origin;
            Vector3 direction;

            if (cam != null)
            {
                origin = cam.transform.position;
                direction = cam.transform.forward;
            }
            else
            {
                origin = player.transform.position + Vector3.up * 1.5f;
                direction = player.transform.forward;
            }

            if (Physics.SphereCast(origin, raycastRadius, direction, out var hit, raycastDistance, interactableMask))
            {
                var interactable = hit.collider.GetComponentInParent<InteractableObject>();
                if (interactable != null && interactable.enabled && interactable.gameObject.activeInHierarchy && !interactable.IsConsumed)
                    return interactable;
            }

            var allInteractables = FindObjectsByType<InteractableObject>(FindObjectsInactive.Exclude);
            Vector3 p = player.transform.position;
            float bestSqr = float.MaxValue;
            InteractableObject autoBest = null;
            for (int i = 0; i < allInteractables.Length; i++)
            {
                var o = allInteractables[i];
                if (o == null || !o.gameObject.activeInHierarchy || o.IsConsumed) continue;
                float sqr = (o.transform.position - p).sqrMagnitude;
                float range = Mathf.Max(o.TriggerRadius, 8f);
                range *= range;
                if (sqr <= range && sqr < bestSqr)
                {
                    bestSqr = sqr;
                    autoBest = o;
                }
            }
            return autoBest;
        }

        static bool IsEchoActive()
        {
            var rec = EchoRecorder.Instance;
            if (rec != null && rec.IsRecording) return true;
            var pb = FindAnyObjectByType<EchoPlayback>();
            return pb != null && pb.IsPlaying;
        }

        public void Register(InteractableObject o)
        {
            if (o == null) return;
            if (!_nearby.Contains(o))
                _nearby.Add(o);
        }

        public void Unregister(InteractableObject o)
        {
            if (o == null) return;
            _nearby.Remove(o);
        }

        void HandleInteraction(InteractableObject obj)
        {
            switch (obj.Category)
            {
                case InteractableCategory.Ambient:
                    // Categoría C: feedback sin abrir interfaz (sonido + reacción).
                    obj.OnInteracted?.Invoke();
                    if (obj.GetComponent<AmbientReaction>() == null && GameFeelController.Instance != null)
                        GameFeelController.Instance.PlayMechanicTick(obj.transform.position, 0.5f);
                    break;

                case InteractableCategory.Gameplay:
                    // Categoría A: acción de juego (puerta, toggle, pista, custom).
                    obj.OnInteracted?.Invoke();
                    if (obj.GetComponent<GameplayInteraction>() == null && GameFeelController.Instance != null)
                        GameFeelController.Instance.PlayMechanicTick(obj.transform.position, 0.6f);
                    break;

                default:
                    // Categoría B: flujo narrativo existente (VN / inspección).
                    var trigger = obj.Trigger;
                    if (trigger != null && trigger.CanInteract())
                    {
                        trigger.OnInteract();
                        return;
                    }
                    RequestInspection(obj);
                    break;
            }
        }

        public void RequestInspection(InteractableObject obj)
        {
            string title = obj.DisplayName ?? "Objeto";
            string text;

            var stage = AidenStageResolver.ResolveForCurrentLevel();
            var entry = VN_TextTable.Get(obj.CommentKey, stage);
            if (entry != null && !string.IsNullOrEmpty(entry.text))
                text = entry.text;
            else
                text = "Algo en este objeto llama mi atencion.";

            string spritePath = stage switch
            {
                AidenStage.Conviction => "VN/Sprites/aiden/Aiden_preocupada",
                AidenStage.Guilt => "VN/Sprites/aiden/Aiden_pensativa_preocupada",
                AidenStage.Realization => "VN/Sprites/aiden/Aiden_triste",
                _ => "VN/Sprites/aiden/Aiden_Feliz",
            };

            if (obj.IsLyraArtifact && VN_EndingFlags.Instance != null)
            {
                VN_EndingFlags.Instance.BumpLyraArtifactSeen();

                if (MemorySystem.Instance != null && !string.IsNullOrEmpty(obj.InteractableId))
                    MemorySystem.Instance.RegisterMemory(obj.InteractableId, obj.NarrativeData?.MemoryEffect);
            }

            VN_OverlayController.Instance?.Play(title, text, spritePath);
        }

        /// <summary>
        /// Un objeto es "relevante" si no exige línea de visión, o si esta existe.
        /// Evita mostrar el prompt a través de paredes (SPEC: visible).
        /// </summary>
        bool IsRelevant(InteractableObject obj)
        {
            if (!obj.RequireLineOfSight)
                return true;
            return HasLineOfSight(obj);
        }

        bool HasLineOfSight(InteractableObject obj)
        {
            Camera cam = Camera.main;
            if (cam == null)
                return true;

            Vector3 origin = cam.transform.position;
            Vector3 target = obj.transform.position + Vector3.up * 0.5f;
            Vector3 dir = target - origin;
            float dist = dir.magnitude;
            if (dist < 0.01f)
                return true;
            dir /= dist;

            int mask = interactableMask.value;
            mask &= ~(1 << LayerMask.NameToLayer("Player"));
            if (Physics.SphereCast(origin, 0.15f, dir, out RaycastHit hit, dist, mask, QueryTriggerInteraction.Ignore))
            {
                // El propio objeto no se tapa a sí mismo.
                if (hit.collider != null && hit.collider.transform.IsChildOf(obj.transform))
                    return true;
                return hit.distance >= dist - 0.1f;
            }
            return true;
        }

        void PlayPromptSound()
        {
            AudioClip clip = EchoesAudioAssets.Get(EchoesAudioAssets.UiInteractionAvailable);
            if (clip == null)
                return;

            GameObject host = new GameObject("InteractionPromptSFX");
            AudioSource src = host.AddComponent<AudioSource>();
            src.clip = clip;
            src.spatialBlend = 0f;
            src.volume = 0.35f;
            src.Play();
            Destroy(host, clip.length + 0.1f);
        }
    }
}
