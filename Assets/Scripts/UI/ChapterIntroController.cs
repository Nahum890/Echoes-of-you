using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Echoes.UI
{
    /// <summary>
    /// ChapterIntroController — shows a chapter intro screen when a level loads.
    /// Displays "CAPITULO XX", the level name, description, and a "Comenzar" button.
    /// Auto-hides after 3 seconds OR when the button is pressed / any key pressed.
    /// While visible, gameplay input is blocked (callers should check IsIntroActive).
    /// Host on a UI GameObject with UIDocument referencing ChapterIntroUI.uxml.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class ChapterIntroController : MonoBehaviour
    {
        public static ChapterIntroController Instance { get; private set; }

        [Tooltip("Auto-hide after this many seconds (-1 = manual only).")]
        [SerializeField] float autoHideSeconds = 3f;

        UIDocument _doc;
        VisualElement _root;
        Label _chapterLabel;
        Label _chapterTitle;
        Label _chapterDescription;
        Button _btnStart;

        bool _active;
        Coroutine _autoHideCo;

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

            _chapterLabel = _root.Q<Label>("chapter-label");
            _chapterTitle = _root.Q<Label>("chapter-title");
            _chapterDescription = _root.Q<Label>("chapter-description");
            _btnStart = _root.Q<Button>("btn-start");

            if (_btnStart != null) _btnStart.clicked += Hide;

            Hide();
        }

        void Update()
        {
            if (!_active) return;

            // Any key dismisses the intro
            if (Input.anyKeyDown && _autoHideCo != null)
            {
                Hide();
            }
        }

        /// <summary>True while the intro overlay is blocking the screen.</summary>
        public bool IsIntroActive => _active;

        /// <summary>
        /// Show the chapter intro screen.
        /// </summary>
        /// <param name="chapterNumber">e.g. 1 for "CAPITULO 01"</param>
        /// <param name="title">Level display name, e.g. "Entrada Escolar"</param>
        /// <param name="description">Short descriptive line</param>
        public void Show(int chapterNumber, string title, string description = null)
        {
            if (_root == null) return;

            if (_chapterLabel != null)
                _chapterLabel.text = $"CAPITULO {chapterNumber:D2}";

            if (_chapterTitle != null)
                _chapterTitle.text = title ?? "Memoria";

            if (_chapterDescription != null)
                _chapterDescription.text = description ?? string.Empty;

            _root.RemoveFromClassList("chapter-hidden");
            _active = true;

            if (_autoHideCo != null) StopCoroutine(_autoHideCo);
            if (autoHideSeconds > 0f) _autoHideCo = StartCoroutine(AutoHideDelay());
        }

        /// <summary>Hide the intro overlay and resume gameplay.</summary>
        public void Hide()
        {
            _root?.AddToClassList("chapter-hidden");
            _active = false;
            if (_autoHideCo != null)
            {
                StopCoroutine(_autoHideCo);
                _autoHideCo = null;
            }
        }

        IEnumerator AutoHideDelay()
        {
            yield return new WaitForSecondsRealtime(autoHideSeconds);
            Hide();
        }
    }
}
