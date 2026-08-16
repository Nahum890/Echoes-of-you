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

            if (_currentTarget != null && !_currentTarget.IsConsumed)
            {
                string promptText = _currentTarget.PromptText;
                bool primary = _currentTarget.IsLyraArtifact;
                InteractionPromptController.Instance?.ShowPrompt("[E]", promptText, primary);
            }
            else
            {
                InteractionPromptController.Instance?.HidePrompt();
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
            var trigger = obj.Trigger;
            if (trigger != null && trigger.CanInteract())
            {
                trigger.OnInteract();
                return;
            }

            RequestInspection(obj);
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
                AidenStage.Conviction => "VN/Sprites/aiden/Aiden_Perturbada",
                AidenStage.Guilt => "VN/Sprites/aiden/Aiden_Pensativa",
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
    }
}
