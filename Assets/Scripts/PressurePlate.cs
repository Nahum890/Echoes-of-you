using System;
using UnityEngine;

/// <summary>
/// Placa de presión: detecta Player y Echo (por etiqueta). Añade Rigidbody cinemático si falta (requerido con CharacterController).
/// </summary>
[RequireComponent(typeof(Collider))]
public class PressurePlate : MonoBehaviour
{
    [SerializeField] string playerTag = "Player";
    [SerializeField] string echoTag = "Echo";

    int _overlapCount;
    bool _pressed;

    public bool IsPressed => _pressed;

    public event Action<bool> PressedChanged;

    void Awake()
    {
        var col = GetComponent<BoxCollider>();
        col.isTrigger = true;
    }

    void SetPressed(bool value)
    {
        if (_pressed == value)
            return;

        _pressed = value;
        PressedChanged?.Invoke(_pressed);
    }

    void FixedUpdate()
    {
        var box = GetComponent<BoxCollider>();
        Vector3 center = transform.TransformPoint(box.center);
        Vector3 halfExtents = Vector3.Scale(box.size, transform.lossyScale) * 0.5f;

        // Inflate detection slightly upward
        halfExtents.y += 0.2f;
        center.y += 0.1f;

        Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation);
        
        bool foundActor = false;
        foreach (var c in hits)
        {
            if (c.CompareTag(playerTag) || c.CompareTag(echoTag))
            {
                foundActor = true;
                break;
            }
        }

        SetPressed(foundActor);
    }
}
