using Echoes.VN;
using UnityEngine;
using UnityEngine.UIElements;

namespace Echoes.UI
{
    /// <summary>
    /// CreditsController — Escena de créditos con scroll automático.
    /// Botón "Volver" cierra los créditos y vuelve a MainMenu.
    /// Velocidad de scroll: 30px/s.
    /// Si hay un final de bloque persistido (BlockEndingResolver), muestra el
    /// epílogo del final antes de iniciar el scroll de créditos.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class CreditsController : MonoBehaviour
    {
        public static CreditsController Instance { get; private set; }

        UIDocument _doc;
        ScrollView _scrollView;
        Button _btnBack;
        VisualElement _root;
        VisualElement _epiloguePanel;
        Button _epilogueContinue;

        const float SCROLL_SPEED = 30f;
        bool _autoScroll = true;

        void Awake()
        {
            Instance = this;
            _doc = GetComponent<UIDocument>();
        }

        void Start()
        {
            _doc = _doc ?? GetComponent<UIDocument>();
            if (_doc == null || _doc.rootVisualElement == null) return;

            _root = _doc.rootVisualElement;
            _scrollView = _root.Q<ScrollView>("creditsScroll");
            _btnBack = _root.Q<Button>("btnBack");
            _epiloguePanel = _root.Q("epiloguePanel");
            _epilogueContinue = _root.Q<Button>("epilogue-continue");

            if (_btnBack != null)
                _btnBack.clicked += OnBack;

            var ending = BlockEndingResolver.GetPersistedEnding();
            if (ending.HasValue && _epiloguePanel != null)
            {
                ShowEpilogue(ending.Value);
            }
            else if (_scrollView != null)
            {
                ScheduleLayoutRefresh();
            }
        }

        void ShowEpilogue(EndingID ending)
        {
            _autoScroll = false;
            if (_btnBack != null) _btnBack.style.display = DisplayStyle.None;

            var entry = VN_TextTable.GetEpilogue(ending.ToString());
            var voice = _root?.Q<Label>("epilogue-voice");
            var narration = _root?.Q<Label>("epilogue-narration");
            if (voice != null) voice.text = entry != null && !string.IsNullOrEmpty(entry.voice_final) ? entry.voice_final : "...";
            if (narration != null) narration.text = entry != null ? entry.narration : "";

            // Capa larga del epilogo: por que este final, la historia detras y
            // el cierre. Cada label se oculta si viene vacio, para que un final
            // sin texto extendido no deje huecos en el panel.
            SetOrHide(_root?.Q<Label>("epilogue-why"), entry?.why);
            SetOrHide(_root?.Q<Label>("epilogue-reflection"), entry?.reflection);
            SetOrHide(_root?.Q<Label>("epilogue-closing"), entry?.closing);

            VN_EndingFlags.Instance?.SetSalirDelColegio(ending == EndingID.Aceptacion);

            _epiloguePanel.style.display = DisplayStyle.Flex;
            if (_epilogueContinue != null) _epilogueContinue.clicked += OnEpilogueContinue;
        }

        static void SetOrHide(Label label, string text)
        {
            if (label == null) return;
            bool has = !string.IsNullOrEmpty(text);
            label.text = has ? text : "";
            label.style.display = has ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void OnEpilogueContinue()
        {
            if (_epiloguePanel != null) _epiloguePanel.style.display = DisplayStyle.None;
            if (_btnBack != null) _btnBack.style.display = DisplayStyle.Flex;
            _autoScroll = true;
            if (_scrollView != null)
                ScheduleLayoutRefresh();
        }

        void ScheduleLayoutRefresh()
        {
            for (int i = 1; i <= 6; i++)
                _scrollView.schedule.Execute(ForceContentRelayout).ExecuteLater(i * 250);
        }

        void ForceContentRelayout()
        {
            if (_scrollView == null) return;

            var container = _scrollView.contentContainer;
            var previous = container.style.display;
            container.style.display = DisplayStyle.None;
            container.style.display = previous;
        }

        void Update()
        {
            if (_scrollView == null || !_autoScroll) return;

            float currentScroll = _scrollView.scrollOffset.y;
            _scrollView.scrollOffset = new Vector2(0, currentScroll + SCROLL_SPEED * Time.unscaledDeltaTime);
        }

        void OnBack()
        {
            if (NavigationManager.Instance != null)
                NavigationManager.Instance.Pop();
            else if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadScene("MainMenu");
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        public VisualElement Root => _root;
    }
}