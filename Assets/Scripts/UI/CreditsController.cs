using UnityEngine;
using UnityEngine.UIElements;

namespace Echoes.UI
{
    /// <summary>
    /// CreditsController — Escena de créditos con scroll automático.
    /// Botón "Volver" cierra los créditos y vuelve a MainMenu.
    /// Velocidad de scroll: 30px/s.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class CreditsController : MonoBehaviour
    {
        public static CreditsController Instance { get; private set; }

        UIDocument _doc;
        ScrollView _scrollView;
        Button _btnBack;
        VisualElement _root;

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

            if (_btnBack != null)
                _btnBack.clicked += OnBack;
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