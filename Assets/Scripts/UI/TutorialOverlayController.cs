using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Echoes.UI
{
    public class TutorialOverlayController : MonoBehaviour
    {
        public static TutorialOverlayController Instance { get; private set; }

        [SerializeField] UIDocument _doc;
        VisualElement _root;
        VisualElement _container;
        VisualElement _tutorialImage;
        Button _btnClose;
        Button _btnContinue;
        Label _promptLabel;

        bool _isOpen;
        bool _uiReady;
        Action _onClosedCallback;

        public bool IsOpen => _isOpen;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (_doc == null)
                _doc = GetComponent<UIDocument>();

            if (_doc == null)
                _doc = gameObject.AddComponent<UIDocument>();

            if (_doc.panelSettings == null)
                _doc.panelSettings = global::UIBootstrap.PanelSettings;

            if (_doc.visualTreeAsset == null)
            {
                var vta = Resources.Load<VisualTreeAsset>("UI/Tutorial/TutorialOverlayUI");
#if UNITY_EDITOR
                if (vta == null)
                    vta = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/Tutorial/TutorialOverlayUI.uxml");
#endif
                if (vta != null)
                    _doc.visualTreeAsset = vta;
            }

            _doc.sortingOrder = 550; // Above VN and HUD
        }

        void OnEnable()
        {
            StartCoroutine(InitializeUiWhenAttached());
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        IEnumerator InitializeUiWhenAttached()
        {
            _uiReady = false;
            const int maxFramesToWait = 120;
            for (int f = 0; f < maxFramesToWait; f++)
            {
                _container = _doc != null ? _doc.rootVisualElement : null;
                if (_container != null && _container.panel != null)
                    break;
                yield return null;
            }

            if (_container == null || _container.panel == null)
                yield break;

            BuildUi();
            Hide();
            _uiReady = true;
        }

        void BuildUi()
        {
            if (_container != null)
            {
                _container.style.position = Position.Absolute;
                _container.style.left = 0;
                _container.style.top = 0;
                _container.style.right = 0;
                _container.style.bottom = 0;
                _container.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                _container.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                _container.style.flexGrow = 1;
            }

            _root = _container.Q<VisualElement>("tutorial-overlay-root");
            if (_root == null && _doc != null && _doc.visualTreeAsset != null)
            {
                _doc.visualTreeAsset.CloneTree(_container);
                _root = _container.Q<VisualElement>("tutorial-overlay-root");
            }
            if (_root == null)
                _root = _container;

            if (_root != null)
            {
                _root.style.position = Position.Absolute;
                _root.style.left = 0;
                _root.style.top = 0;
                _root.style.right = 0;
                _root.style.bottom = 0;
                _root.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                _root.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                _root.style.flexGrow = 1;
            }

            _tutorialImage = _root.Q<VisualElement>("tutorial-image");
            _btnClose = _root.Q<Button>("btn-close-tutorial");
            _btnContinue = _root.Q<Button>("btn-continue-tutorial");
            _promptLabel = _root.Q<Label>("tutorial-prompt-text");

            if (_btnClose != null)
                _btnClose.clicked += CloseTutorial;

            if (_btnContinue != null)
                _btnContinue.clicked += CloseTutorial;

            // Load high-resolution tutorial texture/sprite
            LoadTutorialImage();
        }

        void LoadTutorialImage()
        {
            if (_tutorialImage == null) return;

            var sprite = Resources.Load<Sprite>("UI/Tutorial/Tutorial_Level01");
            if (sprite != null)
            {
                _tutorialImage.style.backgroundImage = new StyleBackground(sprite);
                _tutorialImage.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                return;
            }

            var tex = Resources.Load<Texture2D>("UI/Tutorial/Tutorial_Level01");
#if UNITY_EDITOR
            if (tex == null)
                tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/UI/Tutorial/Tutorial_Level01.png") ??
                      UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/UI/Tutorial/Tutorial_Level01.jpg");
#endif
            if (tex != null)
            {
                _tutorialImage.style.backgroundImage = new StyleBackground(tex);
                _tutorialImage.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            }
        }

        void Update()
        {
            if (!_isOpen) return;

            if (Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.KeypadEnter) ||
                Input.GetKeyDown(KeyCode.E) ||
                Input.GetKeyDown(KeyCode.Escape))
            {
                CloseTutorial();
            }
        }

        public void Show(Action onClosed = null)
        {
            _onClosedCallback = onClosed;
            _isOpen = true;

            LockPlayerMovement(true);

            if (_container != null)
            {
                _container.style.position = Position.Absolute;
                _container.style.left = 0;
                _container.style.top = 0;
                _container.style.right = 0;
                _container.style.bottom = 0;
                _container.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                _container.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                _container.style.flexGrow = 1;
                _container.style.display = DisplayStyle.Flex;
            }

            if (_root != null)
            {
                _root.RemoveFromClassList("tutorial-hidden");
                _root.style.position = Position.Absolute;
                _root.style.left = 0;
                _root.style.top = 0;
                _root.style.right = 0;
                _root.style.bottom = 0;
                _root.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                _root.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                _root.style.flexGrow = 1;
                _root.style.display = DisplayStyle.Flex;
            }

            LoadTutorialImage();
        }

        public void CloseTutorial()
        {
            if (!_isOpen) return;
            _isOpen = false;

            Hide();
            LockPlayerMovement(false);

            var callback = _onClosedCallback;
            _onClosedCallback = null;
            callback?.Invoke();
        }

        void Hide()
        {
            if (_root != null)
            {
                _root.AddToClassList("tutorial-hidden");
                _root.style.display = DisplayStyle.None;
            }
        }

        void LockPlayerMovement(bool locked)
        {
            var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var p in players)
            {
                if (p != null) p.SetInputLocked(locked);
            }
        }

        public static IEnumerator ShowAndWait()
        {
            var tc = Instance;
            if (tc == null)
            {
                var go = new GameObject("TutorialOverlayController");
                tc = go.AddComponent<TutorialOverlayController>();
            }

            while (!tc._uiReady)
                yield return null;

            bool done = false;
            tc.Show(() => done = true);

            while (!done)
                yield return null;
        }
    }
}