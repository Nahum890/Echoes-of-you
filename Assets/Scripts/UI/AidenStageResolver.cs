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

            // Si el juego corre como bloque de 6 niveles (lo normal hoy: ver
            // GameProgress.BlockLevelCount), se usan las tablas escaladas al
            // bloque. Con las de 15 niveles, Aiden se quedaba en Guilt y el
            // jugador no veia nunca los tonos de Realization ni Acceptance.
            bool isBlock = GameProgress.TotalLevels <= VN_StageThresholds.ByLevelBlock.Length - 1;

            int threshold = isBlock ? VN_StageThresholds.ForLevelBlock(N) : VN_StageThresholds.ForLevel(N);
            AidenStage byLevel = isBlock ? VN_StageThresholds.StageByLevelBlock(N) : VN_StageThresholds.StageByLevel(N);
            AidenStage byScore = VN_StageThresholds.StageByScore(score);

            // Sigue mandando el score: quien no explora se queda atras aunque
            // avance de nivel.
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
