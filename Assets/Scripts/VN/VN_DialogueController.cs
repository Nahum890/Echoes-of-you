using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Echoes.VN
{
    /// <summary>
    /// VN_DialogueController — Visual Novel dialogue system.
    /// Manages a queue of dialogue lines with:
    /// - Typewriter text reveal
    /// - Character sprite switching (Lyra/Aiden, emotion variants)
    /// - Voice audio playback per line
    /// - Advance on [E] or click; skip on [Space]
    /// Integrates with VN_TextTable for line data.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class VN_DialogueController : MonoBehaviour
    {
        public static VN_DialogueController Instance { get; private set; }

        [Tooltip("Characters per second for typewriter effect.")]
        [SerializeField] float typewriterSpeed = 40f;

        [Tooltip("Delay after each line completes, before allowing advance (seconds).")]
        [SerializeField] float lineEndDelay = 0.1f;

        UIDocument _doc;
        VisualElement _root;
        Label _nameLabel;
        Label _textLabel;
        Label _counterLabel;
        VisualElement _spriteLeft;
        VisualElement _spriteCenter;
        VisualElement _spriteRight;
        VisualElement _dialoguePanel;
        VisualElement _container;

        readonly Queue<DialogueLine> _queue = new();
        DialogueLine _current;
        bool _active;
        bool _uiReady;
        bool _playRequested;
        bool _typing;
        Coroutine _uiInitializationCoroutine;
        Coroutine _typeCoroutine;
        AudioSource _voiceSource;

        /// <summary>Definition of a single dialogue line in the queue.</summary>
        [Serializable]
        public struct DialogueLine
        {
            public string characterName;
            public string text;
            public string spritePath;   // e.g. "VN/Sprites/lyra/neutral" (under Resources)
            public SpritePosition position;
            public string voiceClipPath; // e.g. "Audio/VN/voice/lyra_line_001" (under Resources)

            public enum SpritePosition { None, Left, Center, Right }
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // The dialogue overlay must be a top-level document. A child
            // UIDocument inherits its parent layout and can collapse when a
            // sibling UI (such as the choice gate) is hidden.
            if (transform.parent != null)
                transform.SetParent(null);

            DontDestroyOnLoad(gameObject);

            _doc = GetComponent<UIDocument>();
            if (_doc != null)
            {
                _doc.sortingOrder = 500;
            }

            _voiceSource = GetComponent<AudioSource>();
            if (_voiceSource == null)
                _voiceSource = gameObject.AddComponent<AudioSource>();
            _voiceSource.playOnAwake = false;
            _voiceSource.loop = false;
        }

        void OnEnable()
        {
            _doc ??= GetComponent<UIDocument>();
            if (_doc == null)
            {
                Debug.LogError("[VN_Dialogue] Missing UIDocument component.");
                return;
            }

            if (_uiInitializationCoroutine != null)
                StopCoroutine(_uiInitializationCoroutine);
            _uiInitializationCoroutine = StartCoroutine(InitializeUiWhenAttached());
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        IEnumerator InitializeUiWhenAttached()
        {
            _uiReady = false;
            _root = null;

            // UIDocument owns the panel root. Build the visual-novel tree only
            // after that root is connected to the runtime panel.
            const int maxFramesToWait = 180;
            for (int frame = 0; frame < maxFramesToWait; frame++)
            {
                _container = _doc != null ? _doc.rootVisualElement : null;
                if (_container != null && _container.panel != null)
                    break;

                yield return null;
            }

            if (_container == null || _container.panel == null)
            {
                Debug.LogError("[VN_Dialogue] UIDocument did not attach to a runtime panel.");
                yield break;
            }

            BuildRuntimeUi();
            Hide();
            _uiReady = true;
            _uiInitializationCoroutine = null;

            if (_playRequested && _queue.Count > 0)
                BeginPlayback();
        }

        void Update()
        {
            if (!_active) return;

            // Advance / skip
            if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
            {
                if (_typing)
                {
                    // Skip typewriter: show full line immediately
                    CompleteTypewriter();
                }
                else
                {
                    Advance();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                // Space = skip entire queue
                StopDialogue();
            }
        }

        /// <summary>Enqueue a single dialogue line.</summary>
        public void Enqueue(DialogueLine line) => _queue.Enqueue(line);

        /// <summary>Enqueue multiple dialogue lines at once.</summary>
        public void Enqueue(IEnumerable<DialogueLine> lines)
        {
            foreach (var l in lines) _queue.Enqueue(l);
        }

        /// <summary>Clear the queue and start playing.</summary>
        public void Play()
        {
            if (_queue.Count == 0)
            {
                Debug.LogWarning("[VN_Dialogue] Play() called with empty queue.");
                return;
            }

            _playRequested = true;
            if (_uiReady && !_active)
                BeginPlayback();
        }

        /// <summary>Replace the current dialogue with one complete sequence.</summary>
        public void PlaySequence(IEnumerable<DialogueLine> lines)
        {
            StopCurrentLine();
            _queue.Clear();
            _active = false;
            _playRequested = false;

            Enqueue(lines);
            Play();
        }

        /// <summary>Stop all dialogue, clear queue, hide UI.</summary>
        public void StopDialogue()
        {
            _queue.Clear();
            _current = default;
            _active = false;
            _playRequested = false;
            StopCurrentLine();
            if (_voiceSource != null) _voiceSource.Stop();
            Hide();
        }

        /// <summary>True if the dialogue UI is currently active.</summary>
        public bool IsActive => _active;
        /// <summary>True once the UXML tree is attached to its runtime panel.</summary>
        public bool IsReady => _uiReady;

        public IEnumerator WaitUntilReady(float timeoutSeconds = 3f)
        {
            float deadline = Time.realtimeSinceStartup + Mathf.Max(0.1f, timeoutSeconds);
            while (!_uiReady && Time.realtimeSinceStartup < deadline)
                yield return null;
        }

        void BeginPlayback()
        {
            if (!_uiReady || _queue.Count == 0 || _active)
                return;

            _playRequested = false;
            _active = true;
            _container.schedule.Execute(() =>
            {
                Show();
                Advance();
            }).ExecuteLater(1);
        }

        void Show()
        {
            if (_root != null)
                _root.RemoveFromClassList("vn-hidden");
        }

        void Hide()
        {
            if (_root != null)
                _root.AddToClassList("vn-hidden");
        }

        void BuildRuntimeUi()
        {
            _root = _container.Q<VisualElement>("vn-dialogue-root");
            if (_root == null)
            {
                Debug.LogError("[VN_Dialogue] 'vn-dialogue-root' not found in UXML.");
                return;
            }

            _spriteLeft = _root.Q<VisualElement>("sprite-left");
            _spriteCenter = _root.Q<VisualElement>("sprite-center");
            _spriteRight = _root.Q<VisualElement>("sprite-right");
            _dialoguePanel = _root.Q<VisualElement>("dialogue-panel");
            _nameLabel = _root.Q<Label>("character-name");
            _counterLabel = _root.Q<Label>("line-counter");
            _textLabel = _root.Q<Label>("dialogue-text");

            // Inline style fallbacks — guarantee visibility even if USS fails to load
            var amber = new Color(0.91f, 0.70f, 0.38f, 1f);
            var panelBg = new Color(0.047f, 0.055f, 0.07f, 0.96f);

            if (_root != null)
            {
                _root.style.position = Position.Absolute;
                _root.style.left = 0; _root.style.top = 0;
                _root.style.right = 0; _root.style.bottom = 0;
            }

            if (_dialoguePanel != null)
            {
                _dialoguePanel.style.position = Position.Absolute;
                _dialoguePanel.style.left = 64; _dialoguePanel.style.right = 64; _dialoguePanel.style.bottom = 48;
                _dialoguePanel.style.minHeight = 220;
                _dialoguePanel.style.backgroundColor = panelBg;
                _dialoguePanel.style.borderLeftWidth = 3; _dialoguePanel.style.borderRightWidth = 2;
                _dialoguePanel.style.borderTopWidth = 2; _dialoguePanel.style.borderBottomWidth = 2;
                _dialoguePanel.style.borderLeftColor = amber;
                _dialoguePanel.style.borderRightColor = amber;
                _dialoguePanel.style.borderTopColor = amber;
                _dialoguePanel.style.borderBottomColor = amber;
                _dialoguePanel.style.paddingLeft = 32; _dialoguePanel.style.paddingRight = 32;
                _dialoguePanel.style.paddingTop = 24; _dialoguePanel.style.paddingBottom = 16;
                _dialoguePanel.style.flexDirection = FlexDirection.Column;
            }

            if (_nameLabel != null) { _nameLabel.style.color = amber; _nameLabel.style.fontSize = 28; _nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold; }
            if (_counterLabel != null) { _counterLabel.style.color = amber; _counterLabel.style.fontSize = 16; }
            if (_textLabel != null) { _textLabel.style.color = Color.white; _textLabel.style.fontSize = 24; _textLabel.style.whiteSpace = WhiteSpace.Normal; }

            if (_spriteLeft != null) _spriteLeft.style.backgroundSize = new BackgroundSize { sizeType = BackgroundSizeType.Contain };
        }

        void StopCurrentLine()
        {
            if (_typeCoroutine != null)
            {
                StopCoroutine(_typeCoroutine);
                _typeCoroutine = null;
            }
            _typing = false;
        }

        void Advance()
        {
            if (_queue.Count == 0)
            {
                StopDialogue();
                return;
            }

            _current = _queue.Dequeue();

            // Update character name
            if (_nameLabel != null)
                _nameLabel.text = _current.characterName ?? string.Empty;

            // Update line counter
            if (_counterLabel != null)
                _counterLabel.text = $"L{_queue.Count + 1}";

            // Update sprite
            UpdateSprite(_current.spritePath, _current.position);

            // Play voice
            PlayVoice(_current.voiceClipPath);

            // Start typewriter
            if (_textLabel != null)
                _textLabel.text = string.Empty;

            _typing = true;
            if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);
            _typeCoroutine = StartCoroutine(TypeText(_current.text ?? string.Empty));
        }

        IEnumerator TypeText(string text)
        {
            if (_textLabel != null)
                _textLabel.text = string.Empty;

            float delay = 1f / Mathf.Max(1f, typewriterSpeed);
            for (int i = 0; i < text.Length; i++)
            {
                if (_textLabel != null)
                    _textLabel.text = text.Substring(0, i + 1);

                yield return new WaitForSecondsRealtime(delay);
            }

            _typing = false;

            if (_textLabel != null)
                _textLabel.text = text;

            if (lineEndDelay > 0f)
                yield return new WaitForSecondsRealtime(lineEndDelay);
        }

        void CompleteTypewriter()
        {
            if (_typeCoroutine != null)
            {
                StopCoroutine(_typeCoroutine);
                _typeCoroutine = null;
            }
            _typing = false;
            if (_textLabel != null && !string.IsNullOrEmpty(_current.text))
                _textLabel.text = _current.text;
        }

        void UpdateSprite(string spritePath, DialogueLine.SpritePosition pos)
        {
            // Clear all
            if (_spriteLeft != null)   _spriteLeft.style.backgroundImage = new StyleBackground();
            if (_spriteCenter != null) _spriteCenter.style.backgroundImage = new StyleBackground();
            if (_spriteRight != null)  _spriteRight.style.backgroundImage = new StyleBackground();

            if (string.IsNullOrEmpty(spritePath) || pos == DialogueLine.SpritePosition.None)
                return;

            // Load sprite from Resources
            var sprite = Resources.Load<Sprite>(spritePath);
            if (sprite == null)
            {
                // Fallback: try as Texture2D
                var tex = Resources.Load<Texture2D>(spritePath);
                if (tex != null)
                {
                    var bg = new StyleBackground(tex);
                    ApplySpriteTo(pos, bg);
                }
                return;
            }
            ApplySpriteTo(pos, new StyleBackground(sprite));
        }

        void ApplySpriteTo(DialogueLine.SpritePosition pos, StyleBackground bg)
        {
            switch (pos)
            {
                case DialogueLine.SpritePosition.Left:
                    if (_spriteLeft != null) _spriteLeft.style.backgroundImage = bg;
                    break;
                case DialogueLine.SpritePosition.Center:
                    if (_spriteCenter != null) _spriteCenter.style.backgroundImage = bg;
                    break;
                case DialogueLine.SpritePosition.Right:
                    if (_spriteRight != null) _spriteRight.style.backgroundImage = bg;
                    break;
            }
        }

        void PlayVoice(string clipPath)
        {
            if (_voiceSource == null) return;
            _voiceSource.Stop();

            if (string.IsNullOrEmpty(clipPath)) return;

            var clip = Resources.Load<AudioClip>(clipPath);
            if (clip != null)
            {
                _voiceSource.clip = clip;
                _voiceSource.Play();
            }
        }
    }
}
