using UnityEngine;

/// <summary>
/// Referencias de escala: el jugador se mantiene; el nivel se amplía con LevelGeometryScale.
/// </summary>
public static class EchoesWorldMetrics
{
    public const float PlayerHeight = 2.2f;
    public const float PlayerRadius = 0.36f;
    public const float PlayerCenterY = 1.1f;

    /// <summary>Modelo FBX bajo PlayerScaler (tamaño original del proyecto).</summary>
    public const float CharacterModelScale = 0.35f;

    /// <summary>Multiplicador de plataformas, mecánicas y distancias del nivel.</summary>
    public const float LevelGeometryScale = 2f;

    public const float HazardMinHeight = 6.5f;
    public const float HazardThicknessScale = 1.6f;
    public const float PressurePlateSize = 2.8f;
    public const float MinDoorHeight = 10f;
    public const float MinBarrierHeight = 6f;
    public const float PlayerJumpHeight = 2.35f;
}
