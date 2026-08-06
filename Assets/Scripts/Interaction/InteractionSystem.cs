using System.Collections.Generic;
using Echoes.UI;
using Echoes.VN;
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

        readonly Dictionary<string, float> _lastSeenAt = new();
        InteractableObject _currentTarget;

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
            _currentTarget = RaycastForInteractable();

            if (_currentTarget != null)
            {
                InteractionPromptController.Instance?.ShowPrompt("[E]", "Interactuar", true);
            }
            else
            {
                InteractionPromptController.Instance?.HidePrompt();
                return;
            }

            if (!Input.GetKeyDown(KeyCode.E)) return;
            if (_currentTarget.RequireEchoActive && !IsEchoActive()) return;
            if (_lastSeenAt.TryGetValue(_currentTarget.CommentKey, out var t) &&
                Time.time - t < _currentTarget.Cooldown) return;
            RequestInspection(_currentTarget);
            _lastSeenAt[_currentTarget.CommentKey] = Time.time;
        }

        InteractableObject RaycastForInteractable()
        {
            var player = FindAnyObjectByType<PlayerController>();
            if (player == null) return null;

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
                if (interactable != null && interactable.enabled && interactable.gameObject.activeInHierarchy)
                    return interactable;
            }

            var allInteractables = FindObjectsByType<InteractableObject>(FindObjectsInactive.Exclude);
            Vector3 p = player.transform.position;
            float bestSqr = float.MaxValue;
            InteractableObject best = null;
            for (int i = 0; i < allInteractables.Length; i++)
            {
                var o = allInteractables[i];
                if (o == null || !o.gameObject.activeInHierarchy) continue;
                float sqr = (o.transform.position - p).sqrMagnitude;
                float range = Mathf.Max(o.TriggerRadius, 6f);
                range *= range;
                if (sqr <= range && sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = o;
                }
            }
            return best;
        }

        static bool IsEchoActive()
        {
            var rec = EchoRecorder.Instance;
            if (rec != null && rec.IsRecording) return true;
            var pb = FindAnyObjectByType<EchoPlayback>();
            return pb != null && pb.IsPlaying;
        }

        public void Register(InteractableObject o) { }

        public void Unregister(InteractableObject o) { }

        public void RequestInspection(InteractableObject obj)
        {
            var stage = AidenStageResolver.ResolveForCurrentLevel();
            var entry = VN_TextTable.Get(obj.CommentKey, stage);
            if (entry == null)
            {
                Debug.LogWarning($"[InteractionSystem] No entry for {obj.CommentKey} / {stage}");
                return;
            }
            var vnCtrl = FindAnyObjectByType<VN_DialogueController>();
            if (vnCtrl != null)
            {
                var line = new VN_DialogueController.DialogueLine
                {
                    characterName = obj.DisplayName,
                    text = entry.text,
                    spritePath = "VN/Sprites/aiden/Aiden_Pensativa",
                    position = VN_DialogueController.DialogueLine.SpritePosition.Left,
                    voiceClipPath = ""
                };
                vnCtrl.Enqueue(line);
                vnCtrl.Play();
            }
            else
            {
                var hud = FindAnyObjectByType<GameHUD>();
                if (hud != null) hud.ShowInspection(obj.DisplayName, entry.text);
            }
            if (obj.IsLyraArtifact && VN_EndingFlags.Instance != null)
                VN_EndingFlags.Instance.BumpLyraArtifactSeen();
        }
    }
}
