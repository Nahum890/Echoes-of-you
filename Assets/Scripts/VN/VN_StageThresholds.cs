using System;

namespace Echoes.VN
{
    public enum AidenStage { Conviction, Guilt, Realization, Acceptance }

    public static class VN_StageThresholds
    {
        public static readonly int[] ByLevel = new int[16]
        {
            0, 0, 0, 0, 0, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12
        };
        public static int ForLevel(int levelIndex) => ByLevel[Math.Min(levelIndex, 15)];

        public static AidenStage StageByLevel(int levelIndex)
        {
            if (levelIndex < 5) return AidenStage.Conviction;
            if (levelIndex < 9) return AidenStage.Guilt;
            if (levelIndex < 13) return AidenStage.Realization;
            return AidenStage.Acceptance;
        }

        public static AidenStage StageByScore(int score)
        {
            if (score < 4) return AidenStage.Conviction;
            if (score < 8) return AidenStage.Guilt;
            if (score < 12) return AidenStage.Realization;
            return AidenStage.Acceptance;
        }
    }
}
