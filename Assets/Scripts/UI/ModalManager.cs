using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Echoes.UI
{
    /// <summary>
    /// ModalManager — abre, cierra y gestiona diálogos de confirmación EchoModal.
    /// 
    /// Uso:
    ///   ModalManager.Instance.ShowModal(
    ///     "Reiniciar Capítulo",
    ///     "Esto anulará tu progreso en el aula actual.",
    ///     onConfirm: () => ReloadLevel(),
    ///     onCancel: () => {}
    ///   );
    ///   
    ///   ModalManager.Instance.HideModal();
    ///   
    ///   ModalManager.Instance.RegisterModal(echo-about root, confirmBtn, cancelBtn)
    ///   permite a las pantallas con otro diseño de modal registrar su propia instancia.
    /// </summary>
    public class ModalManager : MonoBehaviour
    {
        public static ModalManager Instance { get; private set; }

        [Header("Pro soporte")]
        [SerializeField] VisualTreeAsset _modalTemplate; // EchoModal.uxml

        VisualElement _root;
        VisualElement _currentModal;
        Button _confirmBtn;
        Button _cancelBtn;
        Action _onConfirm;
        Action _onCancel;
        bool _modalOpen;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        void OnEnable()
        {
            // Restore static ref after domain reload
            if (Instance == null)
                Instance = this;
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        public void Setup(VisualElement uiRoot, VisualTreeAsset template)
        {
            _root = uiRoot;
            _modalTemplate = template;
        }

        public void ShowModal(string title, string message, Action onConfirm = null, Action onCancel = null)
        {
            if (_modalTemplate == null || _root == null)
            {
                onConfirm?.Invoke();
                return;
            }

            // Cierra modal abierto previo
            HideModal();

            _currentModal = _modalTemplate.CloneTree();
            _root.Add(_currentModal);
            _currentModal.BringToFront();

            _confirmBtn = _currentModal.Q<Button>("modalConfirmBtn");
            _cancelBtn = _currentModal.Q<Button>("modalCancelBtn");

            _currentModal.Q<Label>("modalTitle").text = title;
            _currentModal.Q<Label>("modalMessage").text = message;

            _onConfirm = onConfirm;
            _onCancel = onCancel;

            if (_confirmBtn != null) _confirmBtn.clicked += HandleConfirm;
            if (_cancelBtn != null) _cancelBtn.clicked += HandleCancel;

            // Focus trap track en FocusManager
            FocusManager.Instance?.TrapFocus(_currentModal);

            _modalOpen = true;
            _currentModal.RemoveFromClassList("hidden");
            _currentModal.AddToClassList("echo-modal--entering");

            // Assign ESC callback global
            StartCoroutine(OnEscHeartbeat());
        }

        public void HideModal()
        {
            if (_currentModal == null) return;
            _currentModal.RemoveFromHierarchy();
            _currentModal = null;
            _modalOpen = false;

            if (_onCancel != null) { _onCancel(); _onCancel = null; }

            FocusManager.Instance?.ReleaseFocusTrap();
        }

        void HandleConfirm()
        {
            HideModal();
            _onConfirm?.Invoke();
            _onConfirm = _onCancel = null;
        }

        void HandleCancel()
        {
            HideModal();
            _onCancel?.Invoke();
            _onConfirm = _onCancel = null;
        }

        IEnumerator OnEscHeartbeat()
        {
            while (_modalOpen)
            {
                if (Input.GetKeyDown(KeyCode.Escape)) HandleCancel();
                yield return new WaitForSecondsRealtime(0.2f);
            }
        }

        public bool IsModalOpen => _modalOpen;
    }
}