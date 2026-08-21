using Echoes.UI;
using Echoes.VN;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace Echoes.VN
{
    public class EpilogueController : MonoBehaviour
    {
        [SerializeField] EndingID expectedEnding;
        [SerializeField] UIDocument document;
        [SerializeField] string returnScene = "MainMenu";
        [SerializeField] float autoReturnSeconds = 10f;

        Label _voiceLabel;
        Label _narrationLabel;
        Button _continueButton;

        void OnEnable()
        {
            if (document == null) document = GetComponent<UIDocument>();
            if (document == null) return;
            var root = document.rootVisualElement;
            _voiceLabel = root.Q<Label>("epilogue-voice");
            _narrationLabel = root.Q<Label>("epilogue-narration");
            _continueButton = root.Q<Button>("epilogue-continue");

            var entry = VN_TextTable.GetEpilogue(expectedEnding.ToString());
            if (_voiceLabel != null) _voiceLabel.text = entry != null ? entry.voice_final : "...";
            if (_narrationLabel != null) _narrationLabel.text = entry != null ? entry.narration : "";

            // Misma capa extendida que en CreditsController: este controlador se
            // usa si el epilogo vive en su propia escena en vez de en creditos.
            SetOrHide(root.Q<Label>("epilogue-why"), entry?.why);
            SetOrHide(root.Q<Label>("epilogue-reflection"), entry?.reflection);
            SetOrHide(root.Q<Label>("epilogue-closing"), entry?.closing);

            if (expectedEnding == EndingID.Aceptacion)
                VN_EndingFlags.Instance?.SetSalirDelColegio(true);
            else
                VN_EndingFlags.Instance?.SetSalirDelColegio(false);

            if (_continueButton != null) _continueButton.clicked += OnContinue;
            Invoke(nameof(AutoReturn), autoReturnSeconds);
        }

        static void SetOrHide(Label label, string text)
        {
            if (label == null) return;
            bool has = !string.IsNullOrEmpty(text);
            label.text = has ? text : "";
            label.style.display = has ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void OnContinue()
        {
            CancelInvoke(nameof(AutoReturn));
            SceneManager.LoadScene(returnScene);
        }

        void AutoReturn()
        {
            SceneManager.LoadScene(returnScene);
        }
    }
}
