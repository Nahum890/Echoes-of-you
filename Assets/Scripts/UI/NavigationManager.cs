using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Echoes.UI
{
    public enum ScreenLayer { Base, Overlay, Modal, Loading }

    /// <summary>
    /// Stack de navegación para UI Toolkit (F0.5).
    /// Gestiona Push/Pop/Replace/Peek de pantallas con clearing por capa.
    ///
    /// - Base: menú principal, nivel-select (fondo permanente).
    /// - Overlay: pausa, settings, confirm modals pequeños.
    /// - Modal: diálogos de confirmación que atrapan foco.
    /// - Loading: pantalla de carga (reemplaza todo).
    ///
    /// Integración con FocusManager: al Push de un Modal se llama TrapFocus;
    /// al Pop se restaura el foco anterior.
    /// </summary>
    public class NavigationManager : MonoBehaviour
    {
        public static NavigationManager Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] bool logNavigation = false;

        readonly Stack<(string screenId, VisualElement root, ScreenLayer layer)> _stack = new();
        readonly Stack<string> _focusHistory = new();

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

        public int StackCount => _stack.Count;

        /// <summary>Añade pantalla al stack. Si es Modal, llama FocusManager.TrapFocus en el root.</summary>
        public void Push(string screenId, VisualElement root, ScreenLayer layer = ScreenLayer.Overlay)
        {
            if (string.IsNullOrEmpty(screenId) || root == null)
            {
                Debug.LogError("[NavigationManager] Push requires non-null screenId and root.");
                return;
            }

            // Si ya está en la cima, ignorar
            if (_stack.Count > 0 && _stack.Peek().screenId == screenId)
                return;

            // Guardar foco actual
            if (FocusManager.Instance != null)
                FocusManager.Instance.SaveAndClearFocus();

            // Si es modal, cerrar todas las capas overlay primero (limpieza)
            while (_stack.Count > 0 && _stack.Peek().layer == ScreenLayer.Overlay)
                Pop();

            _stack.Push((screenId, root, layer));

            // Mostrar
            root.RemoveFromClassList("hidden");
            root.BringToFront();

            if (layer == ScreenLayer.Modal && FocusManager.Instance != null)
                FocusManager.Instance.TrapFocus(root);

            if (logNavigation) Debug.Log($"[NavigationManager] Push({screenId}, layer:{layer}) — stackDepth:{_stack.Count}");
            OnStackChanged();
        }

        /// <summary>Quita la pantalla actual de la cima, restaura foco a la siguiente.</summary>
        public void Pop()
        {
            if (_stack.Count == 0) return;

            var (id, root, layer) = _stack.Pop();

            if (layer == ScreenLayer.Modal && FocusManager.Instance != null)
                FocusManager.Instance.ReleaseFocusTrap();

            root.AddToClassList("hidden");

            if (FocusManager.Instance != null)
                FocusManager.Instance.RestoreLastFocus();

            // Reactivar la pantalla debajo si existe
            if (_stack.Count > 0)
            {
                var (nextId, nextRoot, _) = _stack.Peek();
                nextRoot.RemoveFromClassList("hidden");
                nextRoot.BringToFront();
                if (logNavigation) Debug.Log($"[NavigationManager] Pop → active:{nextId} (stack:{_stack.Count})");
            }

            OnStackChanged();
        }

        /// <summary>
        /// Reemplaza la cima del stack con una nueva pantalla (útil para cambiar panels sin push extra).
        /// </summary>
        public void Replace(string screenId, VisualElement root, ScreenLayer layer = ScreenLayer.Overlay)
        {
            if (_stack.Count > 0)
            {
                var (oldId, oldRoot, oldLayer) = _stack.Pop();
                if (oldLayer == ScreenLayer.Modal && FocusManager.Instance != null)
                    FocusManager.Instance.ReleaseFocusTrap();
                oldRoot.AddToClassList("hidden");
            }

            Push(screenId, root, layer);
        }

        /// <summary>Limpia el stack completo (vuelta al Base layer). Útil al cargar una nueva escena.</summary>
        public void Clear()
        {
            while (_stack.Count > 0)
            {
                var (id, root, layer) = _stack.Pop();
                if (layer == ScreenLayer.Modal && FocusManager.Instance != null)
                    FocusManager.Instance.ReleaseFocusTrap();
            }
            _focusHistory.Clear();
            OnStackChanged();
        }

        /// <summary>Devuelve true si screenId está en la cima del stack.</summary>
        public bool IsTop(string screenId) => _stack.Count > 0 && _stack.Peek().screenId == screenId;

        /// <summary>Devuelve el screenId de la pantalla activa actual.</summary>
        public string CurrentScreen => _stack.Count > 0 ? _stack.Peek().screenId : string.Empty;

        /// <summary>Devuelve el Layer de la pantalla activa.</summary>
        public ScreenLayer CurrentLayer => _stack.Count > 0 ? _stack.Peek().layer : ScreenLayer.Base;

        /// <summary>Devuelve true si cualquier modal está activo en el stack.</summary>
        public bool HasModalActive(ScreenLayer targetLayer = ScreenLayer.Modal)
        {
            foreach (var (_, _, layer) in _stack)
            {
                if (layer == targetLayer) return true;
            }
            return false;
        }

        void OnStackChanged()
        {
            // Hook para FocusManager (se llama en Pop/Push automáticamente)
        }
    }
}