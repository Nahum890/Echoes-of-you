using System.Collections.Generic;
using UnityEngine;

namespace Echoes.VN
{
    public class VN_EndingFlags : MonoBehaviour
    {
        public static VN_EndingFlags Instance { get; private set; }

        readonly Dictionary<string, bool> _flags = new();
        int _memoryBonus;
        int _lyraArtifactsSeen;
        bool _loadedFromDisk;

        public bool LoadedFromDisk => _loadedFromDisk;

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

        public void LoadFromDisk()
        {
            if (_loadedFromDisk) return;
            _loadedFromDisk = true;
            var data = Echoes.Narrative.NarrativeSaveBridge.Load();
            if (data != null)
                Echoes.Narrative.NarrativeSaveBridge.ApplyToRuntime(data);
        }

        public void SaveToDisk()
        {
            Echoes.Narrative.NarrativeSaveBridge.Save();
        }

        void OnApplicationQuit()
        {
            SaveToDisk();
        }

        public IReadOnlyDictionary<string, bool> Flags => _flags;

        public int ComprehensionScore
        {
            get
            {
                int score = _lyraArtifactsSeen + _memoryBonus;
                foreach (var flag in _flags)
                    if (flag.Value && (BlockEndingResolver.CountsForBlock(flag.Key) || OpennessFlags.Contains(flag.Key)))
                        score++;
                return score;
            }
        }
        public int MemoryBonus => _memoryBonus;
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
            _flags[key] = value;
        }

        public bool GetFlag(string key)
        {
            return !string.IsNullOrEmpty(key) && _flags.TryGetValue(key, out var v) && v;
        }

        public void BumpLyraArtifactSeen()
        {
            _lyraArtifactsSeen++;
        }

        public void AddMemoryBonus(int amount) => _memoryBonus += Mathf.Max(0, amount);
        public void RestoreMemoryCounts(int artifacts, int bonus)
        {
            _lyraArtifactsSeen = Mathf.Max(0, artifacts);
            _memoryBonus = Mathf.Max(0, bonus);
        }

        public bool GetSalirDelColegio() => GetFlag("salir_del_colegio");
        public void SetSalirDelColegio(bool v) => SetFlag("salir_del_colegio", v);

        public void ClearAll()
        {
            _flags.Clear();
            _memoryBonus = 0;
            _lyraArtifactsSeen = 0;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
