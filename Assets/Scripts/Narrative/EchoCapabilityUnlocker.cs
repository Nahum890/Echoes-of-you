using UnityEngine;
using Echoes.VN;

namespace Echoes.Narrative
{
    /// <summary>
    /// Bridges a GoalTrigger satisfaction event to a persistent VN_EndingFlags
    /// capability flag, so narrative/level progression can unlock new echo
    /// modes across scenes. Used by N03 rama-derecha reward to set
    /// "unlock_future_echo" for N04's EchoPlaybackMode.Future.
    /// </summary>
    [RequireComponent(typeof(GoalTrigger))]
    public class EchoCapabilityUnlocker : MonoBehaviour
    {
        [SerializeField] string capabilityFlag = "unlock_future_echo";
        [SerializeField] bool setValue = true;
        [SerializeField] bool saveImmediately = true;

        GoalTrigger _trigger;
        bool _applied;

        void Awake()
        {
            _trigger = GetComponent<GoalTrigger>();
        }

        void OnEnable()
        {
            if (_trigger != null)
                _trigger.SatisfactionChanged += OnSatisfied;
        }

        void OnDisable()
        {
            if (_trigger != null)
                _trigger.SatisfactionChanged -= OnSatisfied;
        }

        void OnSatisfied(GoalTrigger trigger, bool satisfied)
        {
            if (!satisfied || _applied)
                return;

            var flags = VN_EndingFlags.Instance;
            if (flags == null)
            {
                Debug.LogWarning("[EchoCapabilityUnlocker] VN_EndingFlags.Instance es null; flag no establecido.");
                return;
            }

            flags.SetFlag(capabilityFlag, setValue);
            _applied = true;

            if (saveImmediately)
                NarrativeSaveBridge.Save();

            Debug.Log($"[EchoCapabilityUnlocker] Capacidad '{capabilityFlag}' = {setValue} persistida en flags narrativos.");
        }
    }
}
