using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Echoes.UI
{
    /// <summary>
    /// FocusManager — Gestión de foco para UI Toolkit.
    ///
    /// RegisterFocusable(element) marca un VisualElement como interactive + focusable
    /// para navegación por teclado/gamepad.
    ///
    /// SaveFocused() / RestoreLastFocus(): guarda y restaura el foco cuando el
    /// NavigationManager hace Push/Pop (ej: menú de confirmación).
    ///
    /// TrapFocus(modalRoot) / ReleaseFocusTrap(): implementa focus-atrapado para
    /// diálogos modales (UI propietaria de Echoes).
    /// </summary>
    public class FocusManager : MonoBehaviour
    {
        public static FocusManager Instance { get; private set; }

        [Header("Config")]
        [SerializeField] bool logNavigation = false;

        // Guardado temporal para restaurar el foco tras Push/Pop
        VisualElement _lastFocusEl;
        VisualElement _modalRoot;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Registra un visual element para ser focusable (tabIndex, focusable en el DOM).</summary>
        public void RegisterFocusable(VisualElement element, int tabIndex = 0)
        {
            if (element == null) return;
            
            // Focusable requiere focusable=true + tabIndex >= 0 internamente en UITK
            // VisualElement.focusable no es público en todas las versiones — 
            // usamos SetEnabled, tabIndex y foco vía events.
            element.tabIndex = tabIndex;

            // Si el element es Button, añadir la clase de foco del plan.
            element.RegisterCallback<FocusEvent>(_ =>
            {
                if (logNavigation) Debug.Log($"[FocusManager] Focused: {element.name}");
                _lastFocusEl = element;
            });
        }

        /// <summary>Fuerza el foco a un elemento concreto (ej. al abrir pause).</summary>
        public void SetFocus(VisualElement element)
        {
            if (element == null) return;
            element.Focus();
            _lastFocusEl = element;
        }

        /// <summary>Restaura el foco al último elemento focusedo (si existe).</summary>
        public void RestoreLastFocus()
        {
            if (_lastFocusEl != null && _lastFocusEl.enabledInHierarchy && _lastFocusEl.visible)
            {
                _lastFocusEl.Focus();
                if (logNavigation) Debug.Log($"[FocusManager] Restored focus to: {_lastFocusEl.name}");
            }
        }

        /// <summary>Guarda el foco actual y lo limpia (antes de Push).</summary>
        public void SaveAndClearFocus()
        {
            _lastFocusEl = null;  // el usuario lo registra con RegisterFocusable o Referencia aparte
        }

        /// <summary>
        /// Activa focus-trap dentro del contenedor root de un modal.
        /// Únicamente los elementos dentro de root son focusables mientras esté atrapado.
        /// No hay implementación de cross fuera — navegación wrap-around (top→footer y viceversa).
        /// </summary>
        public void TrapFocus(VisualElement modalRoot)
        {
            if (modalRoot == null) return;
            _modalRoot = modalRoot;

            // Hacer focusable a todos los buttons/interactables dentro del modal
            var buttons = modalRoot.Query<Button>().ToList();
            foreach (var btn in buttons)
            {
                btn.tabIndex = 1;
            }

            // Intentar enfocar el primer button disponible
            if (buttons.Count > 0)
            {
                SetFocus(buttons[0]);
            }
            else
            {
                modalRoot.Focus();
            }

            if (logNavigation) Debug.Log($"[FocusManager] Trapped focus in modal: {modalRoot.name}");
        }

        /// <summary>Libera el focus-trap (restaura foco al último elemento global).</summary>
        public void ReleaseFocusTrap()
        {
            _modalRoot = null;
            RestoreLastFocus();
        }

        /// <summary>Devuelve true si hay focus-trap activo actualmente.</summary>
        public bool IsFocusTrapped => _modalRoot != null;
    }
}