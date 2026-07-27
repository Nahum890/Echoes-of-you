using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Asset conteniendo datos de trayectoria grabados para ecos ambientales, impuestos o de inversión.
/// </summary>
[CreateAssetMenu(fileName = "NewEchoRecording", menuName = "Echoes of You/Echo Recording Data", order = 2)]
public class EchoRecordingData : ScriptableObject
{
    public float duration;
    public List<RecordFrame> frames = new List<RecordFrame>();
    public AudioClip audioClip;
}
