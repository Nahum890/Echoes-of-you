using System.Collections.Generic;
using UnityEngine;

namespace Echoes.VN
{
    public class VN_EndingFlags : MonoBehaviour
    {
        public static VN_EndingFlags Instance { get; private set; }

        readonly Dictionary<string, bool> _flags = new();
        int _comprehensionScore;
        int _lyraArtifactsSeen;

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

        public IReadOnlyDictionary<string, bool> Flags => _flags;

        public int ComprehensionScore => _comprehensionScore;
        public int LyraArtifactsSeen => _lyraArtifactsSeen;

        // Openness flags inject +1 comprehension. Pattern-holding flags inject +0.
        static readonly HashSet<string> OpennessFlags = new()
        {
            "allow_to_see", "pattern_seen", "allow_other_version", "trust_echo",
            "admit_silence", "touched_locker", "memory_is_real", "single_take",
            "release_control", "touch_lyra_object", "self_coordination", "carry_two",
            "let_go_pain", "imperfect_take", "follow_echo", "break_pattern_n15"
        };

        public void SetFlag(string key, bool value = true)
        {
            if (string.IsNullOrEmpty(key)) return;
            bool wasOpenness = OpennessFlags.Contains(key);
            bool previouslySet = _flags.TryGetValue(key, out var prev) && prev;
            _flags[key] = value;

            if (value && wasOpenness && !previouslySet)
            {
                _comprehensionScore += 1;
                Debug.Log($"[VN_EndingFlags] Openness '{key}' set - comprehension={_comprehensionScore}");
            }
            else if (value && !previouslySet)
            {
                Debug.Log($"[VN_EndingFlags] Pattern '{key}' set (no comprehension)");
            }
        }

        public bool GetFlag(string key)
        {
            return !string.IsNullOrEmpty(key) && _flags.TryGetValue(key, out var v) && v;
        }

        public void BumpLyraArtifactSeen()
        {
            _lyraArtifactsSeen++;
            _comprehensionScore += 1;
            Debug.Log($"[VN_EndingFlags] Lyra artifact inspected ({_lyraArtifactsSeen}). comprehension={_comprehensionScore}");
        }

        public bool GetSalirDelColegio() => GetFlag("salir_del_colegio");
        public void SetSalirDelColegio(bool v) => SetFlag("salir_del_colegio", v);

        public void ClearAll()
        {
            _flags.Clear();
            _comprehensionScore = 0;
            _lyraArtifactsSeen = 0;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
