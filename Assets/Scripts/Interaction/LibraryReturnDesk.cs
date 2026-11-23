using Echoes.UI;
using UnityEngine;

namespace Echoes.Interaction
{
    [RequireComponent(typeof(InteractableObject), typeof(PuzzleSignal))]
    public class LibraryReturnDesk : MonoBehaviour
    {
        [SerializeField] PressurePlate echoPlate;

        void OnEnable() => GetComponent<InteractableObject>().OnInteracted.AddListener(Stamp);
        void OnDisable() => GetComponent<InteractableObject>().OnInteracted.RemoveListener(Stamp);

        void Stamp()
        {
            var reader = FindAnyObjectByType<PlayerController>();
            if (reader == null || Vector3.Distance(reader.transform.position, transform.position) > 2.6f)
            {
                GameHUD.Instance?.ShowToast("Acercate al mostrador para usar el sello.", Color.white, 2f);
                return;
            }
            var signal = GetComponent<PuzzleSignal>();
            if (signal.IsSatisfied)
            {
                GameHUD.Instance?.ShowToast("Devolución registrada. El puente está fijo.", Color.white, 2f);
                return;
            }
            if (echoPlate == null || !echoPlate.IsPressed)
            {
                GameHUD.Instance?.ShowToast("El sello apenas deja media fecha. Del otro lado del mostrador suena un clic.", Color.white, 3f);
                return;
            }
            signal.MarkSatisfied();
            GameHUD.Instance?.ShowToast("La fecha salió entera. La pasarela se asienta.", Color.white, 2f);
        }
    }
}
