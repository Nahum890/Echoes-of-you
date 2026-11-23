using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

namespace Echoes.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class GameHUD : MonoBehaviour
    {
        public static GameHUD Instance { get; private set; }

        VisualElement _root;
        VisualElement _recordPanel;
        VisualElement _recordProgress;
        VisualElement _objective;
        VisualElement _tapeArtwork;
        VisualElement _recordFrame;
        VisualElement _echoSlots;
        readonly Vector2[] _tapeSamples = new Vector2[32];
        int _sampleCursor;
        float _remaining = 1f;
        VisualElement _interaction;
        VisualElement _chalkboardBox;
        Label _recordMode;
        Label _recordTime;
        Label _recordHelp;
        Label _echoCount;
        Label _locationLabel;
        Label _objectiveText;
        Label _toastText;
        Label _echoStateLabel;
        Label _keyPromptLabel;
        Label _interactionKey;
        Label _interactionAction;
        Label _resonanceState;
        Label _chalkboardTitle;
        Label _chalkboardText;

        bool _recording;
        int _echoCurrent = -1;
        int _echoMax = -1;
        float _recordMaxSeconds;
        bool _locationLocked;
        float _targetOpacity = 1f;
        float _currentOpacity = 1f;

        void OnEnable()
        {
            Instance = this;
            _root = GetComponent<UIDocument>().rootVisualElement;
            if (_root == null) return;

            _recordPanel = _root.Q("hud-record-panel");
            _recordProgress = _root.Q("hud-record-progress");
            _objective = _root.Q("hud-objective");
            _tapeArtwork = _root.Q("hud-tape-artwork");
            _recordFrame = _root.Q("hud-record-frame");
            _echoSlots = _root.Q("hud-echo-slots");
            if (_tapeArtwork != null)
                _tapeArtwork.generateVisualContent += DrawTape;
            _interaction = _root.Q("hud-interaction");
            _chalkboardBox = _root.Q("chalkboard-box");
            _recordMode = _root.Q<Label>("hud-record-mode");
            _recordTime = _root.Q<Label>("hud-record-time");
            _recordHelp = _root.Q<Label>("hud-record-help");
            _echoCount = _root.Q<Label>("hud-echo-count");
            _locationLabel = _root.Q<Label>("hud-location");
            _objectiveText = _root.Q<Label>("hud-objective-text");
            _toastText = _root.Q<Label>("hud-toast-text");
            _echoStateLabel = _root.Q<Label>("hud-echo-state-label");
            _keyPromptLabel = _root.Q<Label>("key-prompt-label");
            _interactionKey = _root.Q<Label>("hud-interaction-key");
            _interactionAction = _root.Q<Label>("hud-interaction-action");
            _resonanceState = _root.Q<Label>("hud-resonance-state");
            _chalkboardTitle = _root.Q<Label>("chalkboard-title");
            _chalkboardText = _root.Q<Label>("chalkboard-text");

            _recording = false;
            _recordFrame?.AddToClassList("hidden");
            _recordProgress?.AddToClassList("hidden");
            _echoCurrent = _echoMax = -1;
            _targetOpacity = _currentOpacity = 1f;
            _root.style.opacity = 1f;
            _root.pickingMode = PickingMode.Ignore;
            ApplySavedUIScale();
            NavigationHUD.Instance?.Hide();

            if (!_locationLocked)
                SetLocation(GetSceneDisplayName(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name), false);
        }

        void OnDisable()
        {
            if (_tapeArtwork != null)
                _tapeArtwork.generateVisualContent -= DrawTape;
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (_root == null) return;

            bool dialogue = Echoes.VN.VN_DialogueController.Instance?.IsActive == true
                || Echoes.VN.VN_OverlayController.Instance?.IsOpen == true
                || TutorialOverlayController.Instance?.IsOpen == true;
            float target = dialogue ? 0f : _targetOpacity;
            if (_currentOpacity == target) return;
            _currentOpacity = EchoesSettings.ReduceMotion
                ? target
                : Mathf.MoveTowards(_currentOpacity, target, Time.unscaledDeltaTime / 0.10f);
            _root.style.opacity = _currentOpacity;
        }

        public void SetRecording(bool recording, float normalizedTime01)
        {
            if (_recording != recording)
            {
                _recording = recording;
                _recordPanel?.EnableInClassList("hud-recorder--recording", recording);
                _recordProgress?.EnableInClassList("hidden", !recording);
                _recordFrame?.EnableInClassList("hidden", !recording);
                _tapeArtwork?.MarkDirtyRepaint();
                if (recording)
                {
                    System.Array.Clear(_tapeSamples, 0, _tapeSamples.Length);
                    _sampleCursor = 0;
                }
                if (_recordMode != null) _recordMode.text = recording ? "REC" : "CINTA DE ECO";
                if (_recordHelp != null) _recordHelp.text = recording ? "Suelta R · Crear eco" : "Mantén R · Grabar";
            }

            if (!recording) return;

            float remaining = 1f - Mathf.Clamp01(normalizedTime01);
            SetStability(remaining);
            if (_recordTime != null)
            {
                string time = $"{remaining * _recordMaxSeconds:0.0} s";
                if (_recordTime.text != time) _recordTime.text = time;
            }
            _recordPanel?.EnableInClassList("hud-recorder--low", remaining <= 0.2f);
        }

        public void SetRecordDuration(float maxSeconds) => _recordMaxSeconds = Mathf.Max(0f, maxSeconds);

        public void SetEchoCount(int current, int max)
        {
            if (_echoCurrent == current && _echoMax == max) return;

            _echoCurrent = current;
            _echoMax = max;
            _echoStateLabel?.EnableInClassList("hidden", current == 0 || string.IsNullOrEmpty(_echoStateLabel.text));
            _recordPanel?.EnableInClassList("hidden", max <= 0);
            if (_echoCount != null) _echoCount.text = $"Ecos {current}/{max}";
            _echoSlots?.Clear();
            for (int i = 0; i < max && i < 8; i++)
            {
                var slot = new VisualElement { pickingMode = PickingMode.Ignore };
                slot.AddToClassList(i < current ? "hud-splice-filled" : "hud-splice");
                _echoSlots?.Add(slot);
            }
        }

        public void SetEchoState(string state)
        {
            if (_echoStateLabel == null || _echoStateLabel.text == state) return;
            _echoStateLabel.text = state ?? string.Empty;
            _echoStateLabel.EnableInClassList("hidden", string.IsNullOrEmpty(state) || _echoCurrent == 0);
        }

        public void SetResonance(string state)
        {
            if (_resonanceState == null) return;
            _resonanceState.text = state ?? string.Empty;
            _resonanceState.EnableInClassList("hidden", string.IsNullOrEmpty(state));
        }

        public void SetInteraction(string key, string action, bool primary = false)
        {
            if (_interaction == null) return;

            bool hasAction = !string.IsNullOrEmpty(action);
            _interaction.EnableInClassList("hidden", !hasAction);
            if (!hasAction) return;

            bool hasKey = !string.IsNullOrEmpty(key);
            _interactionKey.EnableInClassList("hidden", !hasKey);
            _interactionKey.text = key ?? string.Empty;
            _interactionAction.text = action;
            _interaction.EnableInClassList("hud-interaction--primary", primary);
        }

        /// <summary>
        /// Fija la ubicación del HUD. Con <paramref name="lockLocation"/> el texto
        /// deja de autocompletarse con el nombre de escena.
        /// </summary>
        public void SetLocation(string location, bool lockLocation = true)
        {
            if (_locationLabel == null) return;
            _locationLabel.text = string.IsNullOrEmpty(location) ? "Escuela" : location;
            _locationLocked = lockLocation && !string.IsNullOrEmpty(location);
        }

        public void SetObjective(string objective)
        {
            if (_objectiveText == null) return;
            bool has = !string.IsNullOrEmpty(objective);
            _objectiveText.text = has ? objective : "";
            _objective?.EnableInClassList("hidden", !has);
        }

        public void SetStability(float normalized01)
        {
            _remaining = Mathf.Clamp01(normalized01);
            _tapeArtwork?.MarkDirtyRepaint();

            if (!_recording || _recordFrame == null) return;
            float reveal = EchoesSettings.ReduceMotion ? 1f : Mathf.Clamp01((1f - _remaining) * _recordMaxSeconds / 0.28f);
            float size = Mathf.SmoothStep(0.94f, 1f, reveal);
            _recordFrame.style.scale = new Scale(new Vector3(size, size, 1f));
            _recordFrame.style.opacity = Mathf.Lerp(0.55f, 1f, reveal);
            _recordFrame.EnableInClassList("hud-record-frame--low", _remaining <= 0.2f);
        }

        public void AddTapeSample(Vector2 movement)
        {
            if (!_recording) return;
            _tapeSamples[_sampleCursor] = movement;
            _sampleCursor = (_sampleCursor + 1) % _tapeSamples.Length;
        }

        void DrawTape(MeshGenerationContext context)
        {
            float width = _tapeArtwork.contentRect.width;
        float height = _tapeArtwork.contentRect.height;
            if (width < 110f || height < 30f) return;

            Painter2D pen = context.painter2D;
            var metal = new Color32(190, 165, 112, 255);
            var aqua = new Color32(141, 225, 202, 255);
            var iron = new Color32(40, 48, 42, 255);
            var red = new Color32(245, 166, 131, 255);
            float radius = height * 0.28f;
            float centerY = height * 0.48f;
            float side = radius + 11f;
            var left = new Vector2(side, centerY);
            var right = new Vector2(width - side, centerY);
            Circle(pen, left, radius, metal, iron);
            Circle(pen, right, radius, metal, iron);
            float degrees = _recording && !EchoesSettings.ReduceMotion ? (1f - _remaining) * 1440f : 15f;
            for (int i = 0; i < 3; i++)
            {
                float angle = (degrees + i * 120f) * Mathf.Deg2Rad;
                Vector2 arm = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Line(pen, left + arm * (radius * 0.38f), left + arm * (radius * 0.77f), metal, 2f);
                Line(pen, right - arm * (radius * 0.38f), right - arm * (radius * 0.77f), metal, 2f);
            }

            float start = side + radius + 7f;
            float length = width - start * 2f;
            Line(pen, new(start, centerY), new(start + length, centerY), metal, 1f);

            if (!_recording) return;
            pen.BeginPath();
            pen.strokeColor = _remaining > 0.2f ? aqua : red;
            pen.lineWidth = 2f;
            for (int i = 0; i < _tapeSamples.Length; i++)
            {
                Vector2 sample = _tapeSamples[(_sampleCursor + i) % _tapeSamples.Length];
                Vector2 point = new(start + length * i / (_tapeSamples.Length - 1),
                    centerY - Mathf.Clamp(sample.x * 0.75f + sample.y, -1f, 1f) * (height * 0.32f));
                if (i == 0) pen.MoveTo(point);
                else pen.LineTo(point);
            }
            pen.Stroke();
            Line(pen, new(start, height - 6f), new(start + length * _remaining, height - 6f), _remaining > 0.2f ? aqua : red, 3f);
        }

        static void Circle(Painter2D pen, Vector2 center, float radius, Color outline, Color fill)
        {
            pen.BeginPath();
            pen.fillColor = fill;
            pen.strokeColor = outline;
            pen.lineWidth = 1.5f;
            pen.Arc(center, radius, 0f, 360f);
            pen.ClosePath();
            pen.Fill();
            pen.Stroke();
        }

        static void Line(Painter2D pen, Vector2 from, Vector2 to, Color color, float width)
        {
            pen.BeginPath();
            pen.strokeColor = color;
            pen.lineWidth = width;
            pen.MoveTo(from);
            pen.LineTo(to);
            pen.Stroke();
        }

        public void SetPrompt(string prompt)
        {
            if (_promptCoroutine != null)
            {
                StopCoroutine(_promptCoroutine);
                _promptCoroutine = null;
            }
            if (_keyPromptLabel == null) return;
            bool has = !string.IsNullOrEmpty(prompt);
            _keyPromptLabel.text = has ? prompt : "";
            if (has) _keyPromptLabel.RemoveFromClassList("hidden");
            else _keyPromptLabel.AddToClassList("hidden");
        }

        public void SetPrompt(string prompt, float duration)
        {
            SetPrompt(prompt);
            if (!string.IsNullOrEmpty(prompt) && duration > 0f)
                _promptCoroutine = StartCoroutine(HidePromptAfter(duration));
        }

        Coroutine _promptCoroutine;

        IEnumerator HidePromptAfter(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            _promptCoroutine = null;
            ClearPrompt();
        }

        public void ClearPrompt()
        {
            SetPrompt(null);
        }

        public void ShowToast(string message, Color color, float duration = 1.5f)
        {
            if (_toastText == null) return;
            _toastText.text = message ?? "";
            _toastText.style.color = new StyleColor(color);
            _toastText.RemoveFromClassList("hidden");
            if (_toastCoroutine != null) StopCoroutine(_toastCoroutine);
            _toastCoroutine = StartCoroutine(HideToastAfter(duration));
        }

        Coroutine _toastCoroutine;
        IEnumerator HideToastAfter(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (_toastText != null) _toastText.AddToClassList("hidden");
        }

        public void ShowChalkboard(string title, string text)
        {
            if (_chalkboardBox == null) return;
            if (_chalkboardTitle != null) _chalkboardTitle.text = title ?? "";
            if (_chalkboardText != null) _chalkboardText.text = text ?? "";
            _chalkboardBox.RemoveFromClassList("hidden");
        }

        public void HideChalkboard()
        {
            _chalkboardBox?.AddToClassList("hidden");
        }

        // ===== Inspección (Voz Interna de Aiden — pop-ups ≤42 chars, 1ª persona) =====
        // RULE-INSPECT-007: auto-dismiss a 2.5s (±1.0s tolerancia). Usa el
        // canal Chalkboard HUD, no un componente nuevo (RULE-INSPECT-009).
        public void ShowInspection(string title, string text)
        {
            ShowChalkboard(title, text);
            if (_inspectionCoroutine != null) StopCoroutine(_inspectionCoroutine);
            _inspectionCoroutine = StartCoroutine(HideInspectionAfter(2.5f));
        }

        Coroutine _inspectionCoroutine;
        IEnumerator HideInspectionAfter(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            HideChalkboard();
        }

        public void SetVisible(bool visible)
        {
            _targetOpacity = visible ? 1f : 0f;
        }

        public void ApplySavedUIScale()
        {
            if (_root == null) return;
            _root.EnableInClassList("hud-high-contrast", EchoesSettings.HighContrast);
            string scale = GameSettings.UIScaleName;
            _root.RemoveFromClassList("scale-large");
            _root.RemoveFromClassList("scale-xl");
            if (scale == "Large" || scale == "Grande") _root.AddToClassList("scale-large");
            else if (scale == "Extra Large" || scale == "Extra Grande") _root.AddToClassList("scale-xl");
        }

        static string GetSceneDisplayName(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return "Escuela";
            if (sceneName.StartsWith("Level_"))
            {
                string displayName = GameProgress.GetLevelDisplayName(sceneName);
                if (!string.IsNullOrEmpty(displayName)) return displayName;
            }
            return sceneName;
        }
    }
}
