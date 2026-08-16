using UnityEngine;
using UnityEngine.UIElements;

namespace Echoes.UI
{
    /// <summary>
    /// LevelCompleteController — level completion screen.
    /// Shows "MEMORIA ANCLADA" with stats and action buttons.
    /// Caller provides stats via Show(echos, breaks, time).
    /// Wire buttons externally: btn-continue → next scene,
    /// btn-restart → replay, btn-copybook → journal/menu.
    /// Host on a UI GameObject with UIDocument referencing LevelCompleteUI.uxml.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class LevelCompleteController : MonoBehaviour
    {
        public static LevelCompleteController Instance { get; private set; }

        UIDocument _doc;
        VisualElement _root;
        Label _statEchoes;
        Label _statBreaks;
        Label _statTime;
        Button _btnContinue;
        Button _btnRestart;
        Button _btnCopybook;

        public Button BtnContinue => _btnContinue;
        public Button BtnRestart => _btnRestart;
        public Button BtnCopybook => _btnCopybook;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void OnEnable()
        {
            _doc = GetComponent<UIDocument>();
            if (_doc == null) return;
            _root = _doc.rootVisualElement;
            if (_root == null) return;

            _statEchoes  = _root.Q<Label>("stat-echoes");
            _statBreaks  = _root.Q<Label>("stat-breaks");
            _statTime    = _root.Q<Label>("stat-time");
            _btnContinue = _root.Q<Button>("btn-continue");
            _btnRestart  = _root.Q<Button>("btn-restart");
            _btnCopybook = _root.Q<Button>("btn-copybook");

            Hide();
        }

        /// <summary>True while the completion screen is visible.</summary>
        public bool IsVisible { get; private set; }

        /// <summary>Show the completion screen with the given stats.</summary>
        public void Show(int echoes, int breaks, float seconds)
        {
            if (_root == null) return;

            if (_statEchoes != null) _statEchoes.text = echoes.ToString();
            if (_statBreaks != null) _statBreaks.text = breaks.ToString();
            if (_statTime != null) _statTime.text = FormatTime(seconds);

            _root.RemoveFromClassList("complete-hidden");
            IsVisible = true;
        }

        /// <summary>Hide the completion screen.</summary>
        public void Hide()
        {
            _root?.AddToClassList("complete-hidden");
            IsVisible = false;
        }

        static string FormatTime(float seconds)
        {
            int m = Mathf.FloorToInt(seconds / 60f);
            int s = Mathf.FloorToInt(seconds % 60f);
            return $"{m:D2}:{s:D2}";
        }
    }
}
