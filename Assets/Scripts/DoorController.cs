using System;
using UnityEngine;

/// <summary>
/// When all linked plates are pressed, the door opens and emits a one-shot
/// puzzle solved feedback event. If plates are released again, the door closes.
/// </summary>
public class DoorController : MonoBehaviour, IResettableLevelObject
{
    public PressurePlate[] plates;
    public bool latchOpen = true;

    Collider _col;
    Renderer[] _renderers;
    bool _isOpen;

    public bool IsOpen => _isOpen;

    public event Action<bool> DoorStateChanged;

    void Start()
    {
        _col = GetComponent<Collider>();
        _renderers = GetComponentsInChildren<Renderer>();

        if (plates != null)
        {
            foreach (PressurePlate plate in plates)
            {
                if (plate != null)
                    plate.PressedChanged += OnPlateChanged;
            }
        }

        UpdateState();
    }

    void OnDestroy()
    {
        if (plates == null)
            return;

        foreach (PressurePlate plate in plates)
        {
            if (plate != null)
                plate.PressedChanged -= OnPlateChanged;
        }
    }

    void OnPlateChanged(bool _)
    {
        UpdateState();
    }

    void UpdateState()
    {
        bool shouldOpen = true;
        if (plates == null || plates.Length == 0)
        {
            shouldOpen = false;
        }
        else
        {
            foreach (PressurePlate plate in plates)
            {
                if (plate != null && !plate.IsPressed)
                {
                    shouldOpen = false;
                    break;
                }
            }
        }

        if (latchOpen && _isOpen)
            shouldOpen = true;

        if (_isOpen != shouldOpen)
        {
            _isOpen = shouldOpen;
            DoorStateChanged?.Invoke(_isOpen);

            GameFeelController.Instance?.PlayDoorMove(transform.position);

            if (_isOpen)
            {
                if (TutorialHUD.Instance != null)
                    TutorialHUD.Instance.ShowMessage("Puerta abierta", "", 1.5f);
            }
        }

        if (_col != null)
            _col.enabled = !_isOpen;

        if (_renderers == null)
            return;

        foreach (Renderer renderer in _renderers)
        {
            if (renderer != null)
                renderer.enabled = !_isOpen;
        }
    }

    public void ResetLevelState()
    {
        _isOpen = false;
        UpdateState();
    }
}
