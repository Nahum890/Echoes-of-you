using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Echoes.UI
{
    /// <summary>
    /// Navigation HUD – shows level info, echo counter, interaction prompt,
    /// and the ECHO ARCHIVE Recording Overlay (Broken O gauge, buffer countdown, glitch jitter).
    /// It is created once (DontDestroyOnLoad) and updated by other systems.
    /// </summary>
    public class NavigationHUD : MonoBehaviour
    {
        public static NavigationHUD Instance { get; private set; }

        // Exploration UI references
        private VisualElement _root;
        private VisualElement _explorationGroup;
        private Label _locationLabel;
        private Label _echoCountLabel;
        private Button _promptButton;
        private Label _promptKeyLabel;
        private Label _promptActionLabel;

        // Recording Overlay references
        private VisualElement _recordingOverlay;
        private VisualElement _recLiveDot;
        private Label _recFragmentLabel;
        private Label _recBufferValue;
        private VisualElement _recProgressArc;
        private VisualElement _recCenterContent;
        private Label _recMainLabel;

        [Header("Echo settings")]
        [SerializeField] private int maxEchoes = 3;

        private string _currentLocation = "Desconocido";
        private int _currentLevelIndex = 0;
        private int _currentEchoCount = 0;

        // Recording state
        private bool _isRecording;
        private float _recNormalizedTime;
        private float _recRemainingNormalized = 1f;
        private float _nextGlitchTime;
        private Vector2 _currentGlitchOffset;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            RefreshVisualElements();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RefreshVisualElements();
            UpdateLocationForScene(scene.name);
        }

        public void RefreshVisualElements()
        {
            var doc = GetComponent<UIDocument>();
            if (doc == null) return;
            _root = doc.rootVisualElement;
            if (_root == null) return;

            // Exploration
            _explorationGroup   = _root.Q("exploration-group");
            _locationLabel      = _root.Q<Label>("location-label");
            _echoCountLabel     = _root.Q<Label>("echo-count-label");
            _promptButton       = _root.Q<Button>("prompt-button");
            _promptKeyLabel     = _root.Q<Label>("prompt-key");
            _promptActionLabel  = _root.Q<Label>("prompt-action");

            // Recording
            _recordingOverlay   = _root.Q("recording-overlay");
            _recLiveDot         = _root.Q("rec-live-dot");
            _recFragmentLabel   = _root.Q<Label>("rec-fragment-label");
            _recBufferValue     = _root.Q<Label>("rec-buffer-value");
            _recProgressArc     = _root.Q("rec-progress-arc");
            _recCenterContent   = _root.Q("rec-center-content");
            _recMainLabel       = _root.Q<Label>("rec-main-label");

            // Hook Vector Painter on Progress Arc
            if (_recProgressArc != null)
            {
                _recProgressArc.generateVisualContent -= OnGenerateProgressArc;
                _recProgressArc.generateVisualContent += OnGenerateProgressArc;
            }

            // Re-apply state
            HideInteractionPrompt();
            ApplyLocationText();
            ApplyEchoCountText();

            SetRecordingState(_isRecording, _recNormalizedTime, _recRemainingNormalized);

            string currentScene = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(currentScene))
                UpdateLocationForScene(currentScene);
        }

        private void Update()
        {
            if (!_isRecording) return;

            // Live dot pulsing
            if (_recLiveDot != null)
            {
                float pulse = Mathf.Sin(Time.unscaledTime * 6f) * 0.35f + 0.65f;
                _recLiveDot.style.opacity = pulse;
            }

            // Micro-Glitch Jitter on REC center
            if (Time.unscaledTime >= _nextGlitchTime)
            {
                _nextGlitchTime = Time.unscaledTime + Random.Range(0.08f, 0.25f);
                if (Random.value < 0.4f)
                {
                    _currentGlitchOffset = new Vector2(Random.Range(-2f, 2f), Random.Range(-1.5f, 1.5f));
                }
                else
                {
                    _currentGlitchOffset = Vector2.zero;
                }

                if (_recCenterContent != null)
                {
                    _recCenterContent.style.translate = new StyleTranslate(new Translate(_currentGlitchOffset.x, _currentGlitchOffset.y));
                }
            }

            // Repaint vector arc
            _recProgressArc?.MarkDirtyRepaint();
        }

        private void OnGenerateProgressArc(MeshGenerationContext ctx)
        {
            if (!_isRecording) return;

            var p = ctx.painter2D;
            Vector2 center = new Vector2(150f, 150f);
            float radius = 136f;

            // 1. Ghost background arc
            p.strokeColor = new Color(0.39f, 0.83f, 0.98f, 0.12f);
            p.lineWidth = 2.5f;
            p.lineCap = LineCap.Round;
            p.BeginPath();
            p.Arc(center + new Vector2(12f, 6f), radius, -90f, 270f);
            p.Stroke();

            // 2. Active cyan buffer countdown arc
            float sweepAngle = Mathf.Clamp(_recRemainingNormalized * 360f, 0f, 360f);
            if (sweepAngle > 0.5f)
            {
                // Glow outer stroke
                p.strokeColor = new Color(0.39f, 0.83f, 0.98f, 0.35f);
                p.lineWidth = 6f;
                p.lineCap = LineCap.Round;
                p.BeginPath();
                p.Arc(center, radius, -90f, -90f + sweepAngle);
                p.Stroke();

                // Bright core stroke
                p.strokeColor = new Color(0.72f, 0.94f, 1f, 0.95f);
                p.lineWidth = 3f;
                p.lineCap = LineCap.Round;
                p.BeginPath();
                p.Arc(center, radius, -90f, -90f + sweepAngle);
                p.Stroke();
            }
        }

        // -----------------------------------------------------------------
        // Recording State API
        // -----------------------------------------------------------------
        public void SetRecordingState(bool recording, float normalizedTime01, float remainingNormalized01)
        {
            _isRecording = recording;
            _recNormalizedTime = Mathf.Clamp01(normalizedTime01);
            _recRemainingNormalized = Mathf.Clamp01(remainingNormalized01);

            if (_recordingOverlay != null)
            {
                if (recording)
                    _recordingOverlay.RemoveFromClassList("hidden");
                else
                    _recordingOverlay.AddToClassList("hidden");
            }

            if (_explorationGroup != null)
            {
                _explorationGroup.style.opacity = recording ? 0.2f : 1.0f;
            }

            if (recording)
            {
                if (_recBufferValue != null)
                {
                    int pct = Mathf.Clamp(Mathf.RoundToInt(_recRemainingNormalized * 100f), 0, 100);
                    _recBufferValue.text = $"{pct}%";
                }

                if (_recFragmentLabel != null)
                {
                    _recFragmentLabel.text = $"FRAGMENT_ID: {_currentLevelIndex + 1:D2}";
                }

                _recProgressArc?.MarkDirtyRepaint();
            }
        }

        // -----------------------------------------------------------------
        // Public API – called from other systems (InteractionSystem, EchoRecorder…)
        // -----------------------------------------------------------------
        public void UpdateLocation(string location)
        {
            _currentLocation = location ?? "Desconocido";
            ApplyLocationText();
        }

        public void UpdateLocationForScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return;

            if (sceneName == "MainMenu" || sceneName == "CreditsScene" || sceneName == "VN_Dialogue_Test")
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);

            if (sceneName.StartsWith("Level_"))
            {
                _currentLevelIndex = GameProgress.GetSceneIndex(sceneName);
                if (_currentLevelIndex < 0) _currentLevelIndex = 0;

                string displayName = GameProgress.GetLevelDisplayName(sceneName);
                UpdateLocation(displayName);
            }
            else
            {
                UpdateLocation(sceneName);
            }
        }

        public void UpdateEchoCount(int current, int max = -1)
        {
            _currentEchoCount = Mathf.Max(0, current);
            if (max > 0) maxEchoes = max;
            ApplyEchoCountText();
        }

        private void ApplyLocationText()
        {
            if (_locationLabel != null)
                _locationLabel.text = $"Archivo: {_currentLocation}";
        }

        private void ApplyEchoCountText()
        {
            if (_echoCountLabel != null)
                _echoCountLabel.text = $"Ecos {_currentEchoCount}/{maxEchoes}";
        }

        /// <summary>
        /// Shows the central interaction prompt (key + action).
        /// </summary>
        public void ShowInteractionPrompt(string key, string action, bool primary = false)
        {
            if (_promptButton == null)
            {
                RefreshVisualElements();
                if (_promptButton == null) return;
            }

            if (_promptKeyLabel != null)    _promptKeyLabel.text    = key ?? "[ E ]";
            if (_promptActionLabel != null) _promptActionLabel.text = action ?? "";

            if (primary)
                _promptButton.AddToClassList("prompt-button--primary");
            else
                _promptButton.RemoveFromClassList("prompt-button--primary");

            _promptButton.RemoveFromClassList("hidden");
        }

        public void HideInteractionPrompt()
        {
            if (_promptButton != null)
                _promptButton.AddToClassList("hidden");
        }

        // ---------------------------------------------------------------
        // Visibility helper
        // ---------------------------------------------------------------
        public void SetVisible(bool visible)
        {
            if (_root == null)
            {
                var doc = GetComponent<UIDocument>();
                if (doc != null) _root = doc.rootVisualElement;
            }

            if (_root != null)
            {
                _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        public void Hide() => SetVisible(false);
        public void Show() => SetVisible(true);
    }
}
