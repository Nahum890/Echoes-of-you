using System.Collections.Generic;
using Echoes.UI;
using Echoes.VN;
using UnityEngine;

namespace Echoes.Interaction
{
    public class InteractionSystem : MonoBehaviour
    {
        public static InteractionSystem Instance { get; private set; }

        readonly List<InteractableObject> _nearby = new();
        readonly Dictionary<string, float> _lastSeenAt = new();

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
            var nearest = PickNearest();
            if (nearest == null) return;
            if (!Input.GetKeyDown(KeyCode.E)) return;
            if (nearest.RequireEchoActive && !IsEchoActive()) return;
            if (_lastSeenAt.TryGetValue(nearest.CommentKey, out var t) && Time.time - t < nearest.Cooldown) return;
            RequestInspection(nearest);
            _lastSeenAt[nearest.CommentKey] = Time.time;
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
            if (o != null && !_nearby.Contains(o)) _nearby.Add(o);
        }

        public void Unregister(InteractableObject o)
        {
            if (o != null) _nearby.Remove(o);
        }

        InteractableObject PickNearest()
        {
            if (_nearby.Count == 0) return null;
            var player = FindAnyObjectByType<PlayerController>();
            if (player == null) return null;
            Vector3 p = player.transform.position;
            float bestSqr = float.MaxValue;
            InteractableObject best = null;
            for (int i = 0; i < _nearby.Count; i++)
            {
                var o = _nearby[i];
                if (o == null) continue;
                float sqr = (o.transform.position - p).sqrMagnitude;
                if (sqr <= o.TriggerRadius * o.TriggerRadius && sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = o;
                }
            }
            return best;
        }

        public void RequestInspection(InteractableObject obj)
        {
            var stage = AidenStageResolver.ResolveForCurrentLevel();
            var entry = VN_TextTable.Get(obj.CommentKey, stage);
            if (entry == null)
            {
                Debug.LogWarning($"[InteractionSystem] No entry for {obj.CommentKey} / {stage}");
                return;
            }
            var hud = FindAnyObjectByType<GameHUD>();
            if (hud != null) hud.ShowInspection(obj.DisplayName, entry.text);
            if (obj.IsLyraArtifact && VN_EndingFlags.Instance != null)
                VN_EndingFlags.Instance.BumpLyraArtifactSeen();
        }
    }
}
