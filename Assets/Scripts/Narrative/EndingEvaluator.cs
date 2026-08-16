using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Echoes.VN;

namespace Echoes.Narrative
{
    public struct EndingPreview
    {
        public EndingID currentEnding;
        public int comprehensionScore;
        public int lyraArtifactsSeen;
        public List<string> missingFlagsForAceptacion;
        public float progressTowardsAceptacion;
    }

    public static class EndingEvaluator
    {
        static readonly string[] AceptacionHardFlags =
        {
            "allow_other_version",
            "touch_lyra_object",
            "let_go_pain",
            "imperfect_take",
            "follow_echo",
            "break_pattern_n15",
            "release_control"
        };

        public static EndingID Evaluate(IReadOnlyDictionary<string, bool> flags, int lyraArtifactsSeen)
        {
            return VN_EndingResolver.Resolve(flags, lyraArtifactsSeen);
        }

        public static EndingID EvaluateFromRuntime()
        {
            return VN_EndingResolver.ResolveFromRuntime();
        }

        public static EndingPreview GetEndingPreview()
        {
            var flags = VN_EndingFlags.Instance;
            var preview = new EndingPreview
            {
                currentEnding = EvaluateFromRuntime(),
                comprehensionScore = flags != null ? flags.ComprehensionScore : 0,
                lyraArtifactsSeen = flags != null ? flags.LyraArtifactsSeen : 0,
                missingFlagsForAceptacion = new List<string>(),
                progressTowardsAceptacion = 0f
            };

            if (flags == null)
                return preview;

            int totalRequired = AceptacionHardFlags.Length;
            int present = 0;

            for (int i = 0; i < AceptacionHardFlags.Length; i++)
            {
                if (flags.GetFlag(AceptacionHardFlags[i]))
                    present++;
                else
                    preview.missingFlagsForAceptacion.Add(AceptacionHardFlags[i]);
            }

            bool scoreMet = preview.comprehensionScore >= 14;
            if (scoreMet)
                present++;

            preview.progressTowardsAceptacion = (float)present / (totalRequired + 1);

            return preview;
        }

        public static float GetProgressTowards(EndingID endingId)
        {
            var flags = VN_EndingFlags.Instance;
            if (flags == null) return 0f;

            int score = flags.ComprehensionScore;

            return endingId switch
            {
                EndingID.Aislamiento => 1f - Mathf.Clamp01(score / 5f),
                EndingID.Ruminacion => Mathf.Clamp01((score - 4) / 3f) * (1f - Mathf.Clamp01((score - 7) / 1f)),
                EndingID.Negociacion => Mathf.Clamp01((score - 7) / 3f) * (1f - Mathf.Clamp01((score - 11) / 1f)),
                EndingID.Desesperacion => Mathf.Clamp01((score - 11) / 1f),
                EndingID.Aceptacion => GetEndingPreview().progressTowardsAceptacion,
                _ => 0f
            };
        }

        public static string FormatPreview(EndingPreview preview)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Ending actual: {preview.currentEnding}");
            sb.AppendLine($"Comprehension: {preview.comprehensionScore}");
            sb.AppendLine($"Lyra artifacts: {preview.lyraArtifactsSeen}");
            sb.AppendLine($"Progreso Aceptacion: {preview.progressTowardsAceptacion:P0}");

            if (preview.missingFlagsForAceptacion.Count > 0)
            {
                sb.AppendLine("Flags faltantes para Aceptacion:");
                for (int i = 0; i < preview.missingFlagsForAceptacion.Count; i++)
                    sb.AppendLine($"  - {preview.missingFlagsForAceptacion[i]}");
            }

            return sb.ToString();
        }
    }
}
