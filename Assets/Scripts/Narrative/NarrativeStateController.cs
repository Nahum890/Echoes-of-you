using System.Collections.Generic;
using UnityEngine;
using Echoes.Narrative.Data;

namespace Echoes.Narrative
{
    public class NarrativeStateController : MonoBehaviour
    {
        public static NarrativeStateController Instance { get; private set; }

        [SerializeField] NarrativeVariableStore _variableStore;

        readonly Dictionary<string, float> _variables = new();
        bool _inNarrativeMode;
        InteractionType _currentModeType;
        bool _movementWasLocked;

        public bool InNarrativeMode => _inNarrativeMode;
        public InteractionType CurrentModeType => _currentModeType;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDefaults();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void InitializeDefaults()
        {
            _variables.Clear();
            if (_variableStore != null)
            {
                var defs = _variableStore.Variables;
                for (int i = 0; i < defs.Length; i++)
                {
                    if (defs[i] != null)
                        _variables[defs[i].Name] = defs[i].DefaultAsFloat;
                }
            }
            _variables.TryAdd("comprehension_score", 0f);
            _variables.TryAdd("lyra_artifact_seen_count", 0f);
            _variables.TryAdd("objects_inspected_count", 0f);
        }

        public bool HasVariable(string name) => _variables.ContainsKey(name);

        public float GetVariable(string name)
        {
            return _variables.TryGetValue(name, out var v) ? v : 0f;
        }

        public void SetVariable(string name, float value)
        {
            _variables[name] = value;
        }

        public void ApplyVariableChange(VariableChange change)
        {
            if (change == null || string.IsNullOrEmpty(change.VariableName))
                return;
            float current = GetVariable(change.VariableName);
            _variables[change.VariableName] = change.Apply(current);
        }

        public void ApplyVariableChanges(VariableChange[] changes)
        {
            if (changes == null) return;
            for (int i = 0; i < changes.Length; i++)
            {
                if (changes[i] != null)
                    ApplyVariableChange(changes[i]);
            }
        }

        public IReadOnlyDictionary<string, float> AllVariables => _variables;

        public void LoadVariables(Dictionary<string, float> loaded)
        {
            if (loaded == null) return;
            foreach (var kv in loaded)
                _variables[kv.Key] = kv.Value;
        }

        public void EnterNarrativeMode(InteractionType type)
        {
            _inNarrativeMode = true;
            _currentModeType = type;

            bool shouldLock = type == InteractionType.Dialogue || type == InteractionType.Choice;
            if (!shouldLock) return;

            var player = FindAnyObjectByType<PlayerController>();
            if (player != null && !player.IsInputLocked)
            {
                _movementWasLocked = false;
                player.SetInputLocked(true);
            }
            else
            {
                _movementWasLocked = true;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ExitNarrativeMode()
        {
            if (!_inNarrativeMode)
                return;

            _inNarrativeMode = false;
            _currentModeType = InteractionType.Inspect;

            bool shouldUnlock = _currentModeType == InteractionType.Dialogue || _currentModeType == InteractionType.Choice;
            if (!_movementWasLocked && shouldUnlock)
            {
                var player = FindAnyObjectByType<PlayerController>();
                if (player != null)
                    player.SetInputLocked(false);
            }

            _movementWasLocked = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
