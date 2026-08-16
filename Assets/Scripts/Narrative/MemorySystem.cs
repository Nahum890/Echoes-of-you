using System.Collections.Generic;
using UnityEngine;
using Echoes.VN;
using Echoes.Narrative.Data;

namespace Echoes.Narrative
{
    public class MemorySystem : MonoBehaviour
    {
        public static MemorySystem Instance { get; private set; }

        readonly HashSet<string> _inspected = new();

        public int InspectedCount => _inspected.Count;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void RegisterMemory(string interactableId, MemoryEffect effect)
        {
            if (string.IsNullOrEmpty(interactableId))
                return;

            _inspected.Add(interactableId);

            if (effect == null)
                return;

            var flags = VN_EndingFlags.Instance;
            if (flags != null)
            {
                if (effect.FlagsAdded != null)
                {
                    for (int i = 0; i < effect.FlagsAdded.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(effect.FlagsAdded[i]))
                            flags.SetFlag(effect.FlagsAdded[i], true);
                    }
                }

                if (effect.IsLyraArtifact)
                    flags.BumpLyraArtifactSeen();

                if (effect.ComprehensionDelta > 0)
                {
                    for (int i = 0; i < effect.ComprehensionDelta; i++)
                        flags.BumpLyraArtifactSeen();
                }
            }

            var ctrl = NarrativeStateController.Instance;
            if (ctrl != null && effect.VariableChanges != null)
                ctrl.ApplyVariableChanges(effect.VariableChanges);

            if (ctrl != null)
                ctrl.SetVariable("objects_inspected_count", _inspected.Count);

            Debug.Log($"[MemorySystem] Registered memory '{interactableId}'. Total inspected: {_inspected.Count}");
        }

        public bool HasBeenInspected(string interactableId)
        {
            return !string.IsNullOrEmpty(interactableId) && _inspected.Contains(interactableId);
        }

        public List<string> GetInspectedList()
        {
            return new List<string>(_inspected);
        }

        public void LoadInspected(List<string> ids)
        {
            if (ids == null) return;
            _inspected.Clear();
            for (int i = 0; i < ids.Count; i++)
            {
                if (!string.IsNullOrEmpty(ids[i]))
                    _inspected.Add(ids[i]);
            }
        }

        public void ClearAll()
        {
            _inspected.Clear();
        }
    }
}
