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

        // ---------------------------------------------------------------
        // Variante para el bloque de 6 niveles, por el mismo motivo que existe
        // BlockEndingResolver: las tablas de arriba estan calibradas para 15
        // niveles y en un bloque de 6 dejan el arco a medias.
        //
        // Con ByLevel/StageByLevel, un jugador que termina el bloque pasa por
        // Conviction (N1-N4) y Guilt (N5-N6) y NUNCA llega a Realization ni a
        // Acceptance. Como cada prop tiene los cuatro tonos escritos, eso hacia
        // inalcanzable la mitad del texto del juego — y precisamente la mitad
        // donde Aiden entiende algo.
        // ---------------------------------------------------------------

        /// <summary>Umbrales escalados al bloque: piden un punto mas de
        /// comprension por nivel, en vez de los saltos de la tabla de 15.</summary>
        public static readonly int[] ByLevelBlock = new int[7] { 0, 0, 1, 2, 3, 4, 5 };

        public static int ForLevelBlock(int levelIndex) => ByLevelBlock[Math.Clamp(levelIndex, 0, 6)];

        /// <summary>El arco de 15 niveles comprimido en 6: dos niveles por
        /// etapa hasta el final del bloque.</summary>
        public static AidenStage StageByLevelBlock(int levelIndex)
        {
            if (levelIndex < 3) return AidenStage.Conviction;   // N1-N2
            if (levelIndex < 5) return AidenStage.Guilt;        // N3-N4
            if (levelIndex < 6) return AidenStage.Realization;  // N5
            return AidenStage.Acceptance;                       // N6
        }
    }
}
