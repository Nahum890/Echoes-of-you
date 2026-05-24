using UnityEngine;

/// <summary>
/// Guarda el spawn original del jugador para escalar el nivel sin desalinear la posición.
/// </summary>
public class LevelSpawnMarker : MonoBehaviour
{
    public Vector3 OriginalPosition;
    public bool PositionScaled;
}
