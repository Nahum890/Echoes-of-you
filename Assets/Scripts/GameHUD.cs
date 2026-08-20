using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

namespace Echoes.UI
{
    /// <summary>
    /// GameHUD — Fase 4: Mínimo esencial.
    /// Opacidad base 0.0. Fade 0.0→0.85 en 0.10s SOLO grabando (RULE-UI-004).
    /// Elementos visibles:
    ///   - Grabando: REC● + barra 12s + slots
    ///   - Reproduciendo: ECO▶ + slots
    ///   - Objetivo contextual, Toast, Key Prompt, Chalkboard, Footer states
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class GameHUD : MonoBehaviour
    {
        UIDocument _doc;
        VisualElement _root;

        // Elements
        VisualElement _recordPanel;
        VisualElement _recordDot;
        VisualElement _recordBarFill;
        VisualElement _echoSlots;
        VisualElement _hudContainer;
        VisualElement _stabilityFill;
        VisualElement _recallFill;
        Label _objectiveText;
        Label _toastText;
        Label _echoStateLabel;
        Label _keyPromptLabel;
        VisualElement _chalkboardBox;
        Label _chalkboardTitle;
        Label _chalkboardText;
        VisualElement _recordingStatus;
        VisualElement _playbackStatus;

        // State
        bool _recording;
        float _recordNorm;
        int _echoCurrent, _echoMax;
        float _targetOpacity;
        float _currentOpacity;

        void OnEnable()
        {
            _doc = GetComponent<UIDocument>();
            if (_doc == null || _doc.rootVisualElement == null) return;
            _root = _doc.rootVisualElement;

            ApplySavedUIScale();
            QueryElements();
            _root.style.opacity = 0f; // Base opacity 0.0
            _currentOpacity = 0f;
        }

        void QueryElements()
        {
            _recordPanel      = _root.Q("hud-record-panel");
            _recordDot        = _root.Q("hud-record-dot");
            _recordBarFill    = _root.Q("hud-record-bar-fill");
            _echoSlots        = _root.Q("hud-echo-slots");
            _hudContainer     = _root.Q("hud-container");
            _stabilityFill    = _root.Q("hud-stability-fill");
            _recallFill       = _root.Q("hud-recall-fill");
            _objectiveText    = _root.Q<Label>("hud-objective-text");
            _toastText        = _root.Q<Label>("hud-toast-text");
            _echoStateLabel   = _root.Q<Label>("hud-echo-state-label");
            _keyPromptLabel   = _root.Q<Label>("key-prompt-label");
            _chalkboardBox    = _root.Q("chalkboard-box");
            _chalkboardTitle  = _root.Q<Label>("chalkboard-title");
            _chalkboardText   = _root.Q<Label>("chalkboard-text");
            _recordingStatus  = _root.Q("hud-recording-status");
            _playbackStatus   = _root.Q("hud-playback-status");
        }

        void Update()
        {
            if (_root == null) return;

            // Fade logic
            if (Mathf.Abs(_currentOpacity - _targetOpacity) > 0.01f)
            {
                _currentOpacity = Mathf.MoveTowards(_currentOpacity, _targetOpacity, Time.unscaledDeltaTime / 0.10f);
                _root.style.opacity = _currentOpacity;
            }

            // Pulse record dot
            if (_recording && _recordDot != null)
            {
                float pulse = Mathf.Sin(Time.unscaledTime * 4f) * 0.3f + 0.7f;
                _recordDot.style.opacity = pulse;
            }

            // Update record bar
            if (_recording && _recordBarFill != null)
                _recordBarFill.style.width = Length.Percent(_recordNorm * 100f);

            // Footer states
            UpdateFooterStates();
        }

        void UpdateFooterStates()
        {
            if (_recordingStatus != null)
            {
                if (_recording)
                    _recordingStatus.RemoveFromClassList("footer-item--inactive");
                else
                    _recordingStatus.AddToClassList("footer-item--inactive");
            }

            if (_playbackStatus != null)
            {
                if (_echoCurrent > 0 && !_recording)
                    _playbackStatus.RemoveFromClassList("footer-item--inactive");
                else
                    _playbackStatus.AddToClassList("footer-item--inactive");
            }
        }

        // ===== Public API =====

        public void SetRecording(bool recording, float normalizedTime01)
        {
            _recording = recording;
            _recordNorm = Mathf.Clamp01(normalizedTime01);
            _targetOpacity = recording ? 0.85f : 0f; // RULE-UI-004

            // Show/hide record panel
            if (_recordPanel != null)
            {
                if (recording)
                    _recordPanel.RemoveFromClassList("hidden");
                else
                    _recordPanel.AddToClassList("hidden");
            }

            // Record dot pulse handled in Update
        }

        public void SetEchoCount(int current, int max)
        {
            _echoCurrent = Mathf.Max(0, current);
            _echoMax = Mathf.Max(0, max);
            RebuildEchoSlots();
        }

        public void SetEchoState(string state)
        {
            if (_echoStateLabel != null)
            {
                _echoStateLabel.text = state ?? "";
                _echoStateLabel.RemoveFromClassList("hidden");
            }
        }

        public void SetObjective(string objective)
        {
            if (_objectiveText != null)
            {
                _objectiveText.text = objective ?? "";
                _objectiveText.RemoveFromClassList("hidden");
            }
        }

        public void SetPrompt(string prompt)
        {
            if (_keyPromptLabel != null)
            {
                _keyPromptLabel.text = prompt ?? "";
                _keyPromptLabel.RemoveFromClassList("hidden");
            }
        }

        public void SetPrompt(string prompt, float duration)
        {
            SetPrompt(prompt); // Duration ignored - prompt is sticky until ClearPrompt()
        }

        public void ClearPrompt()
        {
            if (_keyPromptLabel != null)
                _keyPromptLabel.AddToClassList("hidden");
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
            if (_chalkboardBox != null)
            {
                _chalkboardTitle.text = title ?? "";
                _chalkboardText.text = text ?? "";
                _chalkboardBox.RemoveFromClassList("hidden");
            }
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

        void RebuildEchoSlots()
        {
            if (_echoSlots == null) return;
            _echoSlots.Clear();
            for (int i = 0; i < _echoMax; i++)
            {
                var slot = new VisualElement();
                slot.AddToClassList("hud-echo-slot");
                if (i < _echoCurrent) slot.AddToClassList("hud-echo-slot--filled");
                _echoSlots.Add(slot);
            }
        }

        public void SetVisible(bool visible)
        {
            // Called by PauseMenu
            if (_root != null)
                _root.style.opacity = visible ? _currentOpacity : 0f;
        }

        public void ApplySavedUIScale()
        {
            if (_root == null) return;
            string scale = GameSettings.UIScaleName;
            _root.RemoveFromClassList("scale-large");
            _root.RemoveFromClassList("scale-xl");
            if (scale == "Large" || scale == "Grande") _root.AddToClassList("scale-large");
            else if (scale == "Extra Large" || scale == "Extra Grande") _root.AddToClassList("scale-xl");
        }
    }
}