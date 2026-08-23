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
            SelfInitialize();
        }

        void OnEnable()
        {
            // Restore static ref after domain reload
            if (Instance == null)
                Instance = this;
            SelfInitialize();
        }

        /// <summary>
        /// Se prepara solo a partir de su propio UIDocument.
        ///
        /// Hacía falta porque GameplayUIBootstrap solo llama a <see cref="Setup"/>
        /// cuando **crea** el ModalManager, y las escenas ya traen uno serializado.
        /// En ese caso el de la escena se quedaba con <c>_root</c> y
        /// <c>_modalTemplate</c> a null para siempre, y ShowModal se salía por el
        /// early-out: los botones de Reiniciar y Salir no abrían ningún diálogo.
        /// </summary>
        void SelfInitialize()
        {
            if (_root != null && _modalTemplate != null) return;

            if (_modalTemplate == null)
                _modalTemplate = Resources.Load<VisualTreeAsset>("UI/EchoesModal");

            if (_root == null)
            {
                var doc = GetComponent<UIDocument>();
                if (doc != null)
                {
                    if (doc.panelSettings == null)
                        doc.panelSettings = global::UIBootstrap.PanelSettings;
                    _root = doc.rootVisualElement;
                }
            }

            StretchToFullScreen(_root);
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        public void Setup(VisualElement uiRoot, VisualTreeAsset template)
        {
            _root = uiRoot;
            if (template != null) _modalTemplate = template;
            if (_modalTemplate == null)
                _modalTemplate = Resources.Load<VisualTreeAsset>("UI/EchoesModal");

            StretchToFullScreen(_root);
        }

        static void StretchToFullScreen(VisualElement element)
        {
            if (element == null) return;
            element.style.position = Position.Absolute;
            element.style.left = 0;
            element.style.top = 0;
            element.style.right = 0;
            element.style.bottom = 0;
            element.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
            element.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
        }

        /// <summary>
        /// Engancha los estilos del modal. <c>EchoesModal.uxml</c> no declara ninguna
        /// hoja (&lt;Style src&gt;) y el tema del panel, <c>EchoesTheme.tss</c>, solo
        /// importa <c>unity-theme://default</c>: ni EchoesTheme.uss ni
        /// Components/EchoModal.uss llegaban nunca al modal. Se clonaba **sin
        /// estilos**, así que <c>.echo-modal</c> no tenía ni posición absoluta ni
        /// pantalla completa y <c>.echo-modal__content</c> se quedaba sin sus 520px:
        /// el diálogo salía como un pegote sin tamaño en la esquina. De ahí que
        /// "Reiniciar" y "Salir" parecieran no hacer nada.
        /// </summary>
        void AttachModalStyles(VisualElement modal)
        {
            if (modal == null) return;

            var ssTheme = Resources.Load<StyleSheet>("UI/EchoesTheme");
            if (ssTheme != null && !modal.styleSheets.Contains(ssTheme))
                modal.styleSheets.Add(ssTheme);

            var ssModal = Resources.Load<StyleSheet>("UI/Components/EchoModal");
            if (ssModal != null && !modal.styleSheets.Contains(ssModal))
                modal.styleSheets.Add(ssModal);
        }

        /// <summary>
        /// Geometría mínima puesta en línea. Es la red de seguridad: aunque las
        /// hojas de estilo faltasen, el modal cubre la pantalla, queda centrado y
        /// es visible y pulsable. <c>.echo-modal</c> arranca en <c>opacity: 0</c> y
        /// solo sube por una transición CSS; si esa transición no corre, el diálogo
        /// existe pero es invisible.
        /// </summary>
        static void ApplyModalFallbackLayout(VisualElement modal)
        {
            if (modal == null) return;

            StretchToFullScreen(modal);
            modal.style.justifyContent = Justify.Center;
            modal.style.alignItems = Align.Center;
            modal.style.opacity = 1f;
            modal.pickingMode = PickingMode.Position;

            var content = modal.Q("modalContent");
            if (content != null)
            {
                content.style.minWidth = 420;
                content.style.minHeight = 180;
                content.style.flexDirection = FlexDirection.Column;
                content.style.justifyContent = Justify.SpaceBetween;
            }
        }

        public void ShowModal(string title, string message, Action onConfirm = null, Action onCancel = null)
        {
            SelfInitialize();

            if (_modalTemplate == null || _root == null)
            {
                // Sin UI no se puede pedir confirmación: no se ejecuta la acción.
                // Antes se invocaba onConfirm a ciegas, así que un fallo de UI
                // reiniciaba el nivel o salía al menú sin preguntar.
                Debug.LogWarning("[ModalManager] Sin plantilla o sin root: no se muestra el diálogo y no se ejecuta la acción.");
                onCancel?.Invoke();
                return;
            }

            // Cierra modal abierto previo (sin disparar su callback de cancelar)
            HideModal();

            StretchToFullScreen(_root);

            _currentModal = _modalTemplate.CloneTree();
            AttachModalStyles(_currentModal);
            _root.Add(_currentModal);
            _currentModal.BringToFront();

            // El TemplateContainer del CloneTree no tiene clases ni tamaño; hay que
            // estirarlo o el .echo-modal de dentro se queda sin caja donde crecer.
            StretchToFullScreen(_currentModal);
            ApplyModalFallbackLayout(_currentModal.Q("echoModal") ?? _currentModal);

            _confirmBtn = _currentModal.Q<Button>("modalConfirmBtn");
            _cancelBtn = _currentModal.Q<Button>("modalCancelBtn");

            // Sin guardas, un UXML que no traiga estas etiquetas lanza
            // NullReference aquí y el modal queda a medio montar: sin callbacks
            // enganchados y sin forma de cerrarlo.
            var titleLabel = _currentModal.Q<Label>("modalTitle");
            if (titleLabel != null) titleLabel.text = title;
            var messageLabel = _currentModal.Q<Label>("modalMessage");
            if (messageLabel != null) messageLabel.text = message;

            _onConfirm = onConfirm;
            _onCancel = onCancel;

            if (_confirmBtn != null) _confirmBtn.clicked += HandleConfirm;
            if (_cancelBtn != null) _cancelBtn.clicked += HandleCancel;

            // Focus trap track en FocusManager
            FocusManager.Instance?.TrapFocus(_currentModal);

            _modalOpen = true;
            _currentModal.RemoveFromClassList("hidden");
            var modalEl = _currentModal.Q("echoModal") ?? _currentModal;
            modalEl.RemoveFromClassList("hidden");
            modalEl.AddToClassList("echo-modal--entering");
            // --visible existe en el USS y nadie lo ponía: el modal dependía solo
            // de la transición de opacidad para llegar a verse.
            modalEl.AddToClassList("echo-modal--visible");

            _confirmBtn?.Focus();

            // Assign ESC callback global
            StartCoroutine(OnEscHeartbeat());
        }

        /// <summary>
        /// Cierra el modal sin ejecutar ningún callback.
        ///
        /// La versión anterior invocaba <c>_onCancel</c> aquí dentro, y
        /// <see cref="HandleConfirm"/> empieza llamando a HideModal: aceptar
        /// disparaba **también** el callback de cancelar. Ahora cada camino invoca
        /// lo suyo y solo lo suyo.
        /// </summary>
        public void HideModal()
        {
            if (_currentModal == null) return;

            if (_confirmBtn != null) _confirmBtn.clicked -= HandleConfirm;
            if (_cancelBtn != null) _cancelBtn.clicked -= HandleCancel;
            _confirmBtn = null;
            _cancelBtn = null;

            _currentModal.RemoveFromHierarchy();
            _currentModal = null;
            _modalOpen = false;

            FocusManager.Instance?.ReleaseFocusTrap();
        }

        /// <summary>Cierra el modal descartándolo: ejecuta el callback de cancelar.</summary>
        public void DismissModal()
        {
            HandleCancel();
        }

        void HandleConfirm()
        {
            Action confirm = _onConfirm;
            _onConfirm = _onCancel = null;
            HideModal();
            confirm?.Invoke();
        }

        void HandleCancel()
        {
            Action cancel = _onCancel;
            _onConfirm = _onCancel = null;
            HideModal();
            cancel?.Invoke();
        }

        /// <summary>
        /// ESC cierra el modal. Antes esto se sondeaba cada 0.2 s con
        /// <c>Input.GetKeyDown</c>, que solo es cierto durante el frame de la
        /// pulsación: el 90% de las veces la tecla caía entre dos sondeos y no
        /// pasaba nada. Ahora se comprueba en cada frame.
        /// </summary>
        IEnumerator OnEscHeartbeat()
        {
            while (_modalOpen)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    HandleCancel();
                    yield break;
                }
                yield return null;
            }
        }

        public bool IsModalOpen => _modalOpen;
    }
}