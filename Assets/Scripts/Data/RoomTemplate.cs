using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serialisable snapshot of a composed room layout.
/// Created by RoomComposer.ExportRoomTemplate().
/// </summary>
[CreateAssetMenu(fileName = "NewRoomTemplate", menuName = "Echoes of You/Room Template", order = 5)]
public class RoomTemplate : ScriptableObject
{
    [Serializable]
    public class Placement
    {
        public string prefabPath;       // e.g. "Assets/Prefabs/Architecture/Arch_Wall.prefab"
        public string prefabGuid;
        public Vector3 localPosition;
        public Quaternion rotation = Quaternion.identity;
        public Vector3 localScale = Vector3.one;
        public string materialToken;    // ArchitecturePiece.materialToken or ""
        public string propName;         // NarrativeProp.propName or ""
        public bool isNarrativeProp;
    }

    [Header("Room Identity")]
    public string roomName;
    public string sceneName;

    [Header("Layout")]
    public List<Placement> placements = new List<Placement>();

    [Header("Profiles")]
    public CameraProfile cameraProfile;
    public string lightingProfileName;

    [Header("Echo Settings")]
    public bool echoMode = true;
    public int maxEchoes = 3;

    [Header("Metadata")]
    public string createdDate;
    public string guid;
}
