using System.Collections.Generic;
using UnityEngine;

namespace Echoes.VN
{
    /// <summary>
    /// Resolver de final para el bloque de 6 niveles (N01–N06).
    /// El resolver global (VN_EndingResolver) exige flags de N11–N15, inalcanzables
    /// en un bloque de 6 niveles; este resolver mide la apertura del jugador con
    /// los flags reales del bloque y escala los umbrales a sus máximos locales.
    /// </summary>
    public static class BlockEndingResolver
    {
        public const string UnlockFutureEcho = "unlock_future_echo";
        public const string FinaleEndingKey = "Echoes.BlockFinaleEnding";
        public const int BlockLevelCount = 6;

        /// <summary>Flags de apertura alcanzables en N01–N06 (registry + micro-choices).</summary>
        static readonly HashSet<string> BlockOpennessFlags = new()
        {
            "allow_to_see",        // N1  — mirar antes de huir
            "pause_at_doubt",      // N2  — nombrar el patrón
            "left_corridor",       // N3  — permitir otra versión (rama Lyra)
            "trust_first_take",    // N3  — micro: confiar en el eco
            "let_hand_rest",       // N4  — admitir el silencio
            "pattern_recognized",  // N5  — tocar la taquilla
            "speak_below_doubt"    // N6  — la memoria es real
        };

        public static int ComputeComprehension(IReadOnlyDictionary<string, bool> flags, int lyraArtifactsSeen)
        {
            int score = Mathf.Max(0, lyraArtifactsSeen);
            if (flags != null)
            {
                foreach (var kv in flags)
                {
                    if (kv.Value && BlockOpennessFlags.Contains(kv.Key)) score += 1;
                }
                if (Get(flags, UnlockFutureEcho)) score += 1;
            }
            return score;
        }

        static bool Get(IReadOnlyDictionary<string, bool> flags, string key)
        {
            return flags != null && flags.TryGetValue(key, out var v) && v;
        }

        public static EndingID Resolve(IReadOnlyDictionary<string, bool> flags, int lyraArtifactsSeen = 0)
        {
            int score = ComputeComprehension(flags, lyraArtifactsSeen);

            bool hasAceptacion =
                Get(flags, "allow_to_see") &&
                Get(flags, "trust_first_take") &&
                Get(flags, "left_corridor") &&
                Get(flags, "let_hand_rest") &&
                Get(flags, "speak_below_doubt") &&
                Get(flags, UnlockFutureEcho) &&
                score >= 8;

            if (hasAceptacion) return EndingID.Aceptacion;
            if (score >= 7) return EndingID.Desesperacion;
            if (score >= 4) return EndingID.Negociacion;
            if (score >= 2) return EndingID.Ruminacion;
            return EndingID.Aislamiento;
        }

        public static EndingID ResolveFromRuntime()
        {
            var f = VN_EndingFlags.Instance;
            if (f == null) return EndingID.Aislamiento;
            return Resolve(f.Flags, f.LyraArtifactsSeen);
        }

        public static void PersistEnding(EndingID id)
        {
            PlayerPrefs.SetString(FinaleEndingKey, id.ToString());
            PlayerPrefs.Save();
        }

        public static EndingID? GetPersistedEnding()
        {
            string raw = PlayerPrefs.GetString(FinaleEndingKey, "");
            if (string.IsNullOrEmpty(raw)) return null;
            if (System.Enum.TryParse(raw, out EndingID id)) return id;
            return null;
        }

        public static void ClearPersistedEnding()
        {
            PlayerPrefs.DeleteKey(FinaleEndingKey);
            PlayerPrefs.Save();
        }
    }
}