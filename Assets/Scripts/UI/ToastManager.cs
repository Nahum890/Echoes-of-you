using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Echoes.UI
{
    /// <summary>
    /// ToastManager — cola de notificaciones fugaz. 
    /// Instancia toasts desde EchoToast.uxml en el root del HUD,
    /// auto-dismiss después de duración.
    /// 1 toast a la vez (cola FIFO).
    /// 
    /// Uso:
    ///   ToastManager.Instance.Show("Guardado completado.", ToastType.Info, 1.5f);
    ///   ToastManager.Instance.Show("Error al grabar eco.", ToastType.Error, 2.0f);
    /// </summary>
    public enum ToastType { Info, Success, Warning, Error }

    public class ToastManager : MonoBehaviour
    {
        public static ToastManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] UIDocument _uiDocument;
        [SerializeField] VisualTreeAsset _toastTemplate;  // EchoToast.uxml

VisualElement _root;
    VisualElement _currentToast;
    Coroutine _toastDismissCoroutine;
        Coroutine _autoDismissCoroutine;

        Queue<(string message, ToastType type, float duration)> _queue = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        public void Setup(VisualElement hudRoot, VisualTreeAsset template)
        {
            _root = hudRoot;
            _toastTemplate = template;
        }

        public void Show(string message, ToastType type = ToastType.Info, float duration = 1.5f)
        {
            _queue.Enqueue((message, type, Mathf.Max(0.1f, duration)));
            if (_currentToast == null || !_currentToast.visible)
                ShowNext();
        }

        void ShowNext()
        {
            if (_queue.Count == 0) return;

            var (msg, type, dur) = _queue.Dequeue();
            if (_root == null) return;

            _currentToast?.RemoveFromHierarchy();
            if (_toastTemplate == null) return;

            _currentToast = _toastTemplate.CloneTree();
            _currentToast.RemoveFromClassList("hidden");
            _root.Add(_currentToast);
            _currentToast.BringToFront();

            var text = _currentToast.Q<Label>("toastText");
            if (text != null) text.text = msg;

            Color col = type switch
            {
                ToastType.Success => new Color(0.83f, 0.93f, 0.58f, 1f), // #D4EE94
                ToastType.Warning => new Color(1, 0.75f, 0, 1),          // #FFBF00 canon
                ToastType.Error   => new Color(0.71f, 0.22f, 0.22f, 1),  // #B23A3A
                _                 => new Color(0.13f, 0.74f, 0.91f, 1),  // #4FC3E8
            };
var borderEl = _currentToast.Q("echoToast");
        if (borderEl != null)
        {
            borderEl.style.borderLeftColor = new StyleColor(col);
            borderEl.style.borderRightColor = new StyleColor(col);
            borderEl.style.borderTopColor = new StyleColor(col);
            borderEl.style.borderBottomColor = new StyleColor(col);
        }

            if (_toastDismissCoroutine != null) StopCoroutine(_toastDismissCoroutine);
            _toastDismissCoroutine = StartCoroutine(DismissToast(dur));
        }

        IEnumerator DismissToast(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (_currentToast != null)
            {
                _currentToast.RemoveFromHierarchy();
                _currentToast = null;
            }
            ProcessNext();
        }

        void ProcessNext()
        {
            if (_queue.Count > 0)
                ShowNext();
        }

        public void Clear()
        {
            _queue.Clear();
            if (_toastDismissCoroutine != null) StopCoroutine(_toastDismissCoroutine);
            _currentToast?.RemoveFromHierarchy();
            _currentToast = null;
        }
    }
}