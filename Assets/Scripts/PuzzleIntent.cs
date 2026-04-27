using UnityEngine;

/// <summary>
/// Metadata de diseño: declara la intención del puzzle para validación y feedback.
/// Se añade automáticamente por el builder a cada nivel.
/// </summary>
public class PuzzleIntent : MonoBehaviour
{
    [Header("Design Requirements")]
    public int requiredActions = 3;
    public bool requiresMovement = true;
    public bool requiresTiming = true;
    public bool isMultiStep = false;
    public int buttonCount = 2;

    [Header("Echo Behavior")]
    public float minimumEchoDistance = 4f;   // distancia mínima que el eco debe recorrer
    public int echoWaypointCount = 0;        // cuántos waypoints debe visitar

    [Header("Runtime Read")]
    [SerializeField] string designNote = "";

    /// <summary>Distancia mínima esperada que el eco recorre.</summary>
    public float MinimumEchoDistance => minimumEchoDistance;

    /// <summary>Nota de diseño legible para debug.</summary>
    public string DesignNote => designNote;

    void OnValidate()
    {
        requiredActions = Mathf.Max(requiredActions, buttonCount + 1);
        minimumEchoDistance = Mathf.Max(minimumEchoDistance, 2f);
    }
}
