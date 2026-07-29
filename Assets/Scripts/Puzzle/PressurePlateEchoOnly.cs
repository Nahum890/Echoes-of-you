// PressurePlateEchoOnly.cs
using UnityEngine;

/// <summary>
/// Variant of PressurePlate that only accepts echo actors.
/// This component configures a standard PressurePlate to act as an EchoOnly plate.
/// </summary>
[RequireComponent(typeof(PressurePlate))]
public class PressurePlateEchoOnly : MonoBehaviour
{
    void Awake()
    {
        var plate = GetComponent<PressurePlate>();
        if (plate != null)
        {
            // Accept only Echo and EchoProjection, reject Player.
            plate.ConfigureAcceptedActors(false, true, true);
        }
    }
}
