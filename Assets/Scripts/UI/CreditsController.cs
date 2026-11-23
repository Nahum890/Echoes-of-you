using Echoes.VN;
using UnityEngine;
using UnityEngine.UIElements;

namespace Echoes.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class CreditsController : MonoBehaviour
    {
        static EndingID? _pendingEnding;

        public static CreditsController Instance { get; private set; }
        public VisualElement Root { get; private set; }

        VisualElement _creditsPage;
        VisualElement _epiloguePage;
        Button _back;
        Button _continue;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetEntry() => _pendingEnding = null;

        public static void Open(EndingID? ending = null)
        {
            _pendingEnding = ending;
            LoadingScreenController.TransitionToScene("CreditsScene");
        }

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            Root = GetComponent<UIDocument>().rootVisualElement;
            EndingID? ending = _pendingEnding;
            _pendingEnding = null;
            if (Root == null) return;

            _creditsPage = Root.Q("creditsPage");
            _epiloguePage = Root.Q("epiloguePanel");
            _back = Root.Q<Button>("btnBack");
            _continue = Root.Q<Button>("epilogue-continue");

            if (_back != null) _back.clicked += OnBack;
            if (_continue != null) _continue.clicked += ShowCredits;
            Root.RegisterCallback<NavigationCancelEvent>(OnCancel);
            Root.RegisterCallback<KeyDownEvent>(OnKeyDown);

            if (ending.HasValue && _epiloguePage != null)
                ShowEpilogue(ending.Value);
            else
                ShowCredits();
        }

        void OnDestroy()
        {
            if (_back != null) _back.clicked -= OnBack;
            if (_continue != null) _continue.clicked -= ShowCredits;
            Root?.UnregisterCallback<NavigationCancelEvent>(OnCancel);
            Root?.UnregisterCallback<KeyDownEvent>(OnKeyDown);
            if (Instance == this) Instance = null;
        }

        void ShowEpilogue(EndingID ending)
        {
            var entry = VN_TextTable.GetEpilogue(ending.ToString());
            SetOrHide("epilogue-voice", entry?.voice_final);
            SetOrHide("epilogue-narration", entry?.narration);
            SetOrHide("epilogue-why", entry?.why);
            SetOrHide("epilogue-reflection", entry?.reflection);
            SetOrHide("epilogue-closing", entry?.closing);

            VN_EndingFlags.Instance?.SetSalirDelColegio(ending == EndingID.Aceptacion);
            SetPage(showEpilogue: true);
        }

        void SetOrHide(string name, string text)
        {
            var label = Root.Q<Label>(name);
            if (label == null) return;
            label.text = text ?? string.Empty;
            label.style.display = string.IsNullOrEmpty(text) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        void ShowCredits() => SetPage(showEpilogue: false);

        void SetPage(bool showEpilogue)
        {
            if (_creditsPage != null)
                _creditsPage.style.display = showEpilogue ? DisplayStyle.None : DisplayStyle.Flex;
            if (_epiloguePage != null)
                _epiloguePage.style.display = showEpilogue ? DisplayStyle.Flex : DisplayStyle.None;

            var focus = showEpilogue ? _continue : _back;
            focus?.schedule.Execute(() => focus.Focus());
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Escape)
            {
                Back();
                evt.StopPropagation();
            }
        }

        void OnCancel(NavigationCancelEvent evt)
        {
            Back();
            evt.StopPropagation();
        }

        void Back()
        {
            if (_epiloguePage != null && _epiloguePage.style.display.value == DisplayStyle.Flex)
                ShowCredits();
            else
                OnBack();
        }

        void OnBack() => LoadingScreenController.TransitionToScene("MainMenu");
    }
}
