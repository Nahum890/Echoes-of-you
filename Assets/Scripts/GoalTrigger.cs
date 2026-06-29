using System;
using UnityEngine;

public class GoalTrigger : MonoBehaviour, IResettableLevelObject
{
    [SerializeField] string displayName = "Memoria";
    [SerializeField] PressurePlate pressurePlate;
    [SerializeField] DoorController doorController;
    [SerializeField] PuzzleSignal puzzleSignal;
    [SerializeField] bool usePlatePressedState = true;
    [SerializeField] bool useDoorOpenState;
    [SerializeField] bool usePuzzleSignalState;
    [SerializeField] bool accumulateOnce = true;

    bool _satisfied;

    public string DisplayName => displayName;
    public bool IsSatisfied => _satisfied;

    public event Action<GoalTrigger, bool> SatisfactionChanged;

    void Awake()
    {
        if (pressurePlate == null)
            pressurePlate = GetComponent<PressurePlate>();
        if (doorController == null)
            doorController = GetComponent<DoorController>();
        if (puzzleSignal == null)
            puzzleSignal = GetComponent<PuzzleSignal>();
    }

    void OnEnable()
    {
        if (pressurePlate != null)
            pressurePlate.PressedChanged += OnPlateChanged;
        if (doorController != null)
            doorController.DoorStateChanged += OnDoorChanged;
        if (puzzleSignal != null)
            puzzleSignal.SignalChanged += OnPuzzleSignalChanged;
    }

    void Start()
    {
        RefreshFromSources();
    }

    void OnDisable()
    {
        if (pressurePlate != null)
            pressurePlate.PressedChanged -= OnPlateChanged;
        if (doorController != null)
            doorController.DoorStateChanged -= OnDoorChanged;
        if (puzzleSignal != null)
            puzzleSignal.SignalChanged -= OnPuzzleSignalChanged;
    }

    public void MarkSatisfied()
    {
        SetSatisfied(true);
    }

    public void ResetLevelState()
    {
        _satisfied = false;
        SatisfactionChanged?.Invoke(this, false);
        RefreshFromSources();
    }

    void OnPlateChanged(bool pressed)
    {
        if (usePlatePressedState)
            SetSatisfied(pressed);
    }

    void OnDoorChanged(bool open)
    {
        if (useDoorOpenState)
            SetSatisfied(open);
    }

    void OnPuzzleSignalChanged(PuzzleSignal signal, bool satisfied)
    {
        if (usePuzzleSignalState)
            SetSatisfied(satisfied);
    }

    void RefreshFromSources()
    {
        if (usePuzzleSignalState && puzzleSignal != null)
        {
            SetSatisfied(puzzleSignal.IsSatisfied);
            return;
        }

        if (useDoorOpenState && doorController != null)
        {
            SetSatisfied(doorController.IsOpen);
            return;
        }

        if (usePlatePressedState && pressurePlate != null)
            SetSatisfied(pressurePlate.IsPressed);
    }

    void SetSatisfied(bool value)
    {
        if (accumulateOnce && _satisfied && !value)
            return;

        if (_satisfied == value)
            return;

        _satisfied = value;
        SatisfactionChanged?.Invoke(this, _satisfied);
    }
}
