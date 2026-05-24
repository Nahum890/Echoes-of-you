using System;
using UnityEngine;

public class PuzzleSignal : MonoBehaviour, IResettableLevelObject
{
    [SerializeField] string displayName = "Memoria";
    [SerializeField] bool accumulateOnce = true;
    [SerializeField] bool satisfiedOnStart;

    bool _satisfied;

    public string DisplayName => displayName;
    public bool IsSatisfied => _satisfied;

    public event Action<PuzzleSignal, bool> SignalChanged;

    public void Configure(string nextDisplayName, bool nextAccumulateOnce = true, bool nextSatisfiedOnStart = false)
    {
        displayName = nextDisplayName;
        accumulateOnce = nextAccumulateOnce;
        satisfiedOnStart = nextSatisfiedOnStart;
    }

    void Start()
    {
        SetSatisfied(satisfiedOnStart);
    }

    public void MarkSatisfied()
    {
        SetSatisfied(true);
    }

    public void SetSatisfied(bool value)
    {
        if (accumulateOnce && _satisfied && !value)
            return;

        if (_satisfied == value)
            return;

        _satisfied = value;
        SignalChanged?.Invoke(this, _satisfied);
    }

    public void ResetLevelState()
    {
        _satisfied = false;
        SignalChanged?.Invoke(this, false);
        if (satisfiedOnStart)
            SetSatisfied(true);
    }
}
