using Echoes.VN;
using UnityEngine;

namespace Echoes.UI
{
    public static class AidenStageResolver
    {
        public static AidenStage ResolveForCurrentLevel()
        {
            int N = LevelRuntimeController.Instance != null ? ResolveLevelIndex() : 1;
            int score = VN_EndingFlags.Instance != null ? VN_EndingFlags.Instance.ComprehensionScore : 0;
            int threshold = VN_StageThresholds.ForLevel(N);
            AidenStage byLevel = VN_StageThresholds.StageByLevel(N);
            AidenStage byScore = VN_StageThresholds.StageByScore(score);
            return score >= threshold ? byLevel : byScore;
        }

        static int ResolveLevelIndex()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            int idx = GameProgress.GetSceneIndex(scene);
            if (idx >= 0) return idx + 1;
            if (int.TryParse(scene.Replace("Level_", "").Replace("N", ""), out var manual)) return manual;
            return 1;
        }
    }
}
