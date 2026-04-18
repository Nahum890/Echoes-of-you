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
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        if (GetComponent<Rigidbody>() == null)
        {
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void SetPressed(bool value)
    {
        if (_pressed == value)
            return;

        _pressed = value;
        PressedChanged?.Invoke(_pressed);
    }

    void Evaluate()
    {
        SetPressed(_overlapCount > 0);
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsActor(other))
        {
            _overlapCount++;
            Evaluate();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsActor(other))
        {
            _overlapCount = Mathf.Max(0, _overlapCount - 1);
            Evaluate();
        }
    }

    bool IsActor(Collider c)
    {
        return c.CompareTag(playerTag) || c.CompareTag(echoTag);
    }
}
