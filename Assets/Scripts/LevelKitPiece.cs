using UnityEngine;

/// <summary>
/// Metadata used by the production builder and validator for playable megakit pieces.
/// Walkable pieces must own a non-trigger collider sized to their footprint.
/// </summary>
public class LevelKitPiece : MonoBehaviour
{
    public string pieceId;
    public string role;
    public bool walkableSurface;
    public bool cameraOccluder;
    public bool requiresGameplayCollider = true;
    public Vector3 footprintSize = Vector3.one;
    public Vector3 clearanceSize = Vector3.one;

    public void Configure(
        string id,
        string pieceRole,
        bool walkable,
        bool occludesCamera,
        Vector3 footprint,
        Vector3 clearance)
    {
        pieceId = id;
        role = pieceRole;
        walkableSurface = walkable;
        cameraOccluder = occludesCamera;
        requiresGameplayCollider = walkable;
        footprintSize = footprint;
        clearanceSize = clearance;
    }
}
