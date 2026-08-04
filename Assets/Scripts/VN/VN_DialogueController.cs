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

        readonly Queue<DialogueLine> _queue = new();
        DialogueLine _current;
        bool _active;
        bool _typing;
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
            DontDestroyOnLoad(gameObject);

            _voiceSource = gameObject.AddComponent<AudioSource>();
            _voiceSource.playOnAwake = false;
            _voiceSource.loop = false;
        }

        void OnEnable()
        {
            _doc = GetComponent<UIDocument>();
            if (_doc == null) return;
            _root = _doc.rootVisualElement;
            if (_root == null) return;

            _nameLabel     = _root.Q<Label>("character-name");
            _textLabel     = _root.Q<Label>("dialogue-text");
            _counterLabel  = _root.Q<Label>("line-counter");
            _spriteLeft    = _root.Q("sprite-left");
            _spriteCenter  = _root.Q("sprite-center");
            _spriteRight   = _root.Q("sprite-right");
            _dialoguePanel = _root.Q("dialogue-panel");

            Hide();
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
            Show();
            Advance();
        }

        /// <summary>Stop all dialogue, clear queue, hide UI.</summary>
        public void StopDialogue()
        {
            _queue.Clear();
            _current = default;
            _active = false;
            if (_typeCoroutine != null)
            {
                StopCoroutine(_typeCoroutine);
                _typeCoroutine = null;
            }
            _typing = false;
            if (_voiceSource != null) _voiceSource.Stop();
            Hide();
        }

        /// <summary>True if the dialogue UI is currently active.</summary>
        public bool IsActive => _active;

        void Show()
        {
            _active = true;
            _root?.RemoveFromClassList("vn-hidden");
        }

        void Hide()
        {
            _root?.AddToClassList("vn-hidden");
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

            float delay = 1f / typewriterSpeed;
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
