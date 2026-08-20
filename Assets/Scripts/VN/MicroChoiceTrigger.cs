using UnityEngine;

namespace Echoes.VN
{
    /// <summary>
    /// Dispara un micro-choice VN (isMicro=true) cuando el jugador entra al trigger.
    /// Se usa en N03 en el punto de bifurcación para registrar la confianza en el
    /// eco (trust_first_take / redo_silently) durante el puzzle, mientras el gate
    /// de fin de nivel muestra la decisión base (left_corridor / right_corridor).
    /// </summary>
    public class MicroChoiceTrigger : MonoBehaviour, IResettableLevelObject
    {
        [Tooltip("Nivel cuyo nodo micro se muestra (registry.GetNode(levelIndex, isMicro=true)).")]
        [SerializeField] int levelIndex = 3;
        [Tooltip("Si es true, el trigger solo se dispara una vez por visita.")]
        [SerializeField] bool fireOnce = true;

        bool _fired;

        void OnTriggerEnter(Collider other)
        {
            if (_fired || !other.CompareTag("Player")) return;

            var gate = VN_ChoiceGateController.Instance;
            if (gate == null || gate.IsShowing) return;

            if (fireOnce) _fired = true;
            gate.Show(levelIndex, true, _ => { });
        }

        public void ResetLevelState()
        {
            _fired = false;
        }
    }
}