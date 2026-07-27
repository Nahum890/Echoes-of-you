using UnityEngine;

/// <summary>
/// Marks a GameObject as an architectural building block for the Echoes level kit.
/// Stores the material-token name so the RoomComposer and PrefabBatchBuilder can
/// re-assign materials at generation time.
/// </summary>
public class ArchitecturePiece : MonoBehaviour
{
    [Tooltip("EchoesMaterialLibrary token, e.g. FloorMat, WallTealMat, ArchMat")]
    public string materialToken = "ArchMat";

    [Tooltip("Apply KenneyTiling UV scaling to all renderers on Start")]
    public bool useKenneyTiling = true;

    [Header("Metadata")]
    public string pieceId;
    public Vector3 snapOffset = Vector3.zero;
}
