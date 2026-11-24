using Echoes.UI;
using UnityEngine;

namespace Echoes.Interaction
{
    public class EchoRehearsalNote : MonoBehaviour, IResettableLevelObject
    {
        [SerializeField] PressurePlate openingMark;
        [SerializeField] InteractableObject note;
        [SerializeField] PuzzleSignal read;
        bool hasReadPart;

        void OnEnable() => openingMark.PressedChanged += Reveal;
        void OnDisable() => openingMark.PressedChanged -= Reveal;

        void Reveal(bool pressed)
        {
            if (!pressed || note.gameObject.activeSelf) return;
            note.gameObject.SetActive(true);
            GameHUD.Instance?.ShowToast("Junto a la primera marca apareció una hoja con tinta azul.", Color.cyan, 4f);
        }

        public void Read()
        {
            hasReadPart = true;
            InteractionSystem.Instance.RequestInspection(note);
        }

        public void StrikeWood() => Answer(true);
        public void RingBell() => Answer(false);

        void Answer(bool wooden)
        {
            string response;
            if (read.IsSatisfied) response = "La respuesta ya está marcada. Ahora toca entrar en pista.";
            else if (!hasReadPart) response = "Dos instrumentos para una sola entrada. En el atril está mi parte.";
            else if (!wooden) response = "El timbre sigue sonando durante el silencio. Lyra dibujó una nota mucho más corta.";
            else
            {
                read.MarkSatisfied();
                response = "Toc. El golpe se corta en seco. Ahora reconozco mi entrada.";
            }
            GameHUD.Instance?.ShowToast(response, Color.white, 4f);
        }

        public void ResetLevelState()
        {
            hasReadPart = false;
            note.gameObject.SetActive(false);
            read.ResetLevelState();
        }
    }
}
