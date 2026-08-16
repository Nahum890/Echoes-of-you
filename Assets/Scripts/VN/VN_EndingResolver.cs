using System.Collections.Generic;
using UnityEngine;

namespace Echoes.VN
{
    public enum EndingID { Aislamiento, Ruminacion, Negociacion, Desesperacion, Aceptacion }

    public static class VN_EndingResolver
    {
        static readonly HashSet<string> OpennessFlags = new()
        {
            "allow_to_see", "pattern_seen", "allow_other_version", "trust_echo",
            "admit_silence", "touched_locker", "memory_is_real", "single_take",
            "release_control", "touch_lyra_object", "self_coordination", "carry_two",
            "let_go_pain", "imperfect_take", "follow_echo", "break_pattern_n15"
        };

        public static int ComputeComprehension(IReadOnlyDictionary<string, bool> flags, int lyraArtifactsSeen)
        {
            int score = lyraArtifactsSeen;
            if (flags == null) return score;
            foreach (var kv in flags)
            {
                if (kv.Value && OpennessFlags.Contains(kv.Key)) score += 1;
            }
            return score;
        }

        public static EndingID Resolve(IReadOnlyDictionary<string, bool> flags, int lyraArtifactsSeen = 0)
        {
            int score = ComputeComprehension(flags, lyraArtifactsSeen);
            bool Get(string k) => flags != null && flags.TryGetValue(k, out var v) && v;

            bool hasAllHardAceptacion =
                Get("allow_other_version") &&
                Get("touch_lyra_object") &&
                Get("let_go_pain") &&
                Get("imperfect_take") &&
                Get("follow_echo") &&
                Get("break_pattern_n15") &&
                Get("release_control") &&
                score >= 14;

            if (hasAllHardAceptacion) return EndingID.Aceptacion;

            bool desesperacion =
                score >= 12 && !Get("let_go_pain");
            if (desesperacion) return EndingID.Desesperacion;

            if (score >= 8 && score < 14) return EndingID.Negociacion;
            if (score >= 5 && score < 8) return EndingID.Ruminacion;
            return EndingID.Aislamiento;
        }

        public static EndingID ResolveFromRuntime()
        {
            var f = VN_EndingFlags.Instance;
            if (f == null) return EndingID.Aislamiento;
            return Resolve(f.Flags, f.LyraArtifactsSeen);
        }

        public static string EndingScene(EndingID id) => id switch
        {
            EndingID.Aislamiento => "Epilogue_Aislamiento",
            EndingID.Ruminacion => "Epilogue_Ruminacion",
            EndingID.Negociacion => "Epilogue_Negociacion",
            EndingID.Desesperacion => "Epilogue_Desperacion",
            EndingID.Aceptacion => "Epilogue_Aceptacion",
            _ => "MainMenu"
        };
    }
}
