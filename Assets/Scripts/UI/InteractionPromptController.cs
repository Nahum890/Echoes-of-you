using UnityEngine;
using UnityEngine.UIElements;

namespace Echoes.UI
{
    /// <summary>
    /// InteractionPromptController — minimal proximity interaction prompt.
    /// Call ShowPrompt("[E]", "Inspeccionar", primary:false) when player enters range.
    /// Call HidePrompt() when player leaves range.
    /// Fade in/out via USS .prompt-hidden class transition (150ms linear).
    /// Host this on a UI GameObject with a UIDocument referencing InteractionPromptUI.uxml.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class InteractionPromptController : MonoBehaviour
    {
        public static InteractionPromptController Instance { get; private set; }

        UIDocument _doc;
        VisualElement _root;
        VisualElement _box;
        Label _keyIcon;
        Label _actionLabel;
        bool _visible;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void OnEnable()
        {
            _doc = GetComponent<UIDocument>();
            if (_doc == null) return;
            _root = _doc.rootVisualElement;
            if (_root == null) return;

            _box = _root.Q("prompt-box");
            _keyIcon = _root.Q<Label>("key-icon");
            _actionLabel = _root.Q<Label>("prompt-label");

            HidePrompt();
        }

        /// <summary>
        /// Show the interaction prompt with the given key icon and action text.
        /// </summary>
        /// <param name="key">Key icon text, e.g. "[E]" or "[X]"</param>
        /// <param name="action">Action label, e.g. "Inspeccionar" or "Grabar Eco"</param>
        /// <param name="primary">If true, border is amber (primary action); otherwise ivory.</param>
        public void ShowPrompt(string key, string action, bool primary = false)
        {
            if (_root == null) return;

            if (_keyIcon != null) _keyIcon.text = key;
            if (_actionLabel != null) _actionLabel.text = action;

            if (_box != null)
            {
                if (primary) _box.AddToClassList("prompt-box--primary");
                else _box.RemoveFromClassList("prompt-box--primary");
            }

            _root.RemoveFromClassList("prompt-hidden");
            _visible = true;
        }

        /// <summary>Hide the interaction prompt with a fade.</summary>
        public void HidePrompt()
        {
            _root?.AddToClassList("prompt-hidden");
            _visible = false;
        }

        /// <summary>True if the prompt is currently visible.</summary>
        public bool IsVisible => _visible;

        public void ShowPromptFor(Echoes.Interaction.InteractableObject obj)
        {
            if (obj == null)
            {
                HidePrompt();
                return;
            }
            ShowPrompt("[E]", obj.PromptText, obj.IsLyraArtifact);
        }
    }
}
