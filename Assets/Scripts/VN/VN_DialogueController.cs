using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Echoes.VN
{
    /// <summary>
    /// VN_DialogueController — Visual Novel dialogue system matching the narrative mockup.
    /// Manages:
    /// - Player movement freezing during dialogue
    /// - Left (Aiden) and Right (Lyra) character positioning & active dimming
    /// - Typewriter text reveal with blinking cursor
    /// - Log, Auto, and Skip buttons
    /// - Mouse & Keyboard navigation ([E], Click, [Space])
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class VN_DialogueController : MonoBehaviour
    {
        public static VN_DialogueController Instance { get; private set; }

        public static VN_DialogueController EnsureExists()
        {
            if (Instance != null) return Instance;
            var existing = FindAnyObjectByType<VN_DialogueController>();
            if (existing != null)
            {
                Instance = existing;
                return Instance;
            }

            var panel = global::UIBootstrap.PanelSettings;
            var go = new GameObject("VNDialogueController");
            var doc = go.AddComponent<UIDocument>();
            doc.panelSettings = panel;
            doc.sortingOrder = 500;
            var vta = Resources.Load<VisualTreeAsset>("UI/VN/VN_DialogueUI");
#if UNITY_EDITOR
            if (vta == null) vta = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/VN/VN_DialogueUI.uxml");
#endif
            if (vta != null) doc.visualTreeAsset = vta;
            Instance = go.AddComponent<VN_DialogueController>();
            if (Application.isPlaying) DontDestroyOnLoad(go);
            return Instance;
        }

        [Tooltip("Characters per second for typewriter effect.")]
        [SerializeField] float typewriterSpeed = 45f;

        [Tooltip("Delay after each line completes, before allowing advance (seconds).")]
        [SerializeField] float lineEndDelay = 0.1f;

        UIDocument _doc;
        VisualElement _root;
        Label _nameLabel;
        Label _textLabel;
        Label _cursorLabel;
        Label _counterLabel;
        VisualElement _spriteLeft;
        VisualElement _spriteCenter;
        VisualElement _spriteRight;
        VisualElement _dialoguePanel;
        VisualElement _container;
        Button _btnBacklog;
        Button _btnAuto;
        Button _btnSkip;
        Label _advancePrompt;

        readonly Queue<DialogueLine> _queue = new();
        DialogueLine _current;
        bool _active;
        bool _uiReady;
        bool _playRequested;
        bool _typing;
        bool _autoAdvance;
        bool _sequenceHasLyra;
        bool _sequenceHasAiden;
        Coroutine _uiInitializationCoroutine;
        Coroutine _typeCoroutine;
        Coroutine _autoCoroutine;
        AudioSource _voiceSource;

        /// <summary>Definition of a single dialogue line in the queue.</summary>
        [Serializable]
        public struct DialogueLine
        {
            public string characterName;
            public string text;
            public string spritePath;   // e.g. "VN/Sprites/lyra/Lyra_Neutral" (under Resources)
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

            if (transform.parent != null)
                transform.SetParent(null);

            if (Application.isPlaying)
                DontDestroyOnLoad(gameObject);

            _doc = GetComponent<UIDocument>();
            if (_doc != null)
            {
                if (_doc.panelSettings == null)
                    _doc.panelSettings = global::UIBootstrap.PanelSettings;

                if (_doc.visualTreeAsset == null)
                {
                    _doc.visualTreeAsset = Resources.Load<VisualTreeAsset>("UI/VN/VN_DialogueUI");
#if UNITY_EDITOR
                    if (_doc.visualTreeAsset == null)
                        _doc.visualTreeAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/VN/VN_DialogueUI.uxml");
#endif
                }

                _doc.sortingOrder = 500;
                _doc.enabled = true;
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

            // Advance or complete typewriter
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                if (_typing)
                {
                    CompleteTypewriter();
                }
                else
                {
                    Advance();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
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

            _sequenceHasLyra = false;
            _sequenceHasAiden = false;

            if (lines != null)
            {
                foreach (var l in lines)
                {
                    string n = l.characterName ?? "";
                    if (n.IndexOf("lyra", StringComparison.OrdinalIgnoreCase) >= 0 || l.position == DialogueLine.SpritePosition.Right)
                        _sequenceHasLyra = true;
                    if (n.IndexOf("aiden", StringComparison.OrdinalIgnoreCase) >= 0 || l.position == DialogueLine.SpritePosition.Left)
                        _sequenceHasAiden = true;
                }
            }

            Enqueue(lines);
            Play();
        }

        /// <summary>Stop all dialogue, clear queue, hide UI, unlock player.</summary>
        public void StopDialogue()
        {
            _queue.Clear();
            _current = default;
            _active = false;
            _playRequested = false;
            _autoAdvance = false;
            StopCurrentLine();
            if (_autoCoroutine != null)
            {
                StopCoroutine(_autoCoroutine);
                _autoCoroutine = null;
            }
            if (_voiceSource != null) _voiceSource.Stop();
            Hide();
            LockPlayerMovement(false);
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
            LockPlayerMovement(true);

            Show();
            Advance();
        }

        void Show()
        {
            if (_container != null)
            {
                _container.style.position = Position.Absolute;
                _container.style.left = 0;
                _container.style.top = 0;
                _container.style.right = 0;
                _container.style.bottom = 0;
                _container.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                _container.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                _container.style.flexGrow = 1;
                _container.style.display = DisplayStyle.Flex;
            }
            if (_root != null)
            {
                _root.style.position = Position.Absolute;
                _root.style.left = 0;
                _root.style.top = 0;
                _root.style.right = 0;
                _root.style.bottom = 0;
                _root.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                _root.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                _root.style.flexGrow = 1;
                _root.RemoveFromClassList("vn-hidden");
                _root.style.display = DisplayStyle.Flex;
            }
        }

        void Hide()
        {
            if (_root != null)
            {
                _root.AddToClassList("vn-hidden");
                _root.style.display = DisplayStyle.None;
            }
            if (_spriteRight != null)
            {
                _spriteRight.style.display = DisplayStyle.None;
                _spriteRight.style.backgroundImage = new StyleBackground();
            }
            if (_spriteLeft != null)
            {
                _spriteLeft.style.display = DisplayStyle.None;
                _spriteLeft.style.backgroundImage = new StyleBackground();
            }
            if (_spriteCenter != null)
            {
                _spriteCenter.style.display = DisplayStyle.None;
                _spriteCenter.style.backgroundImage = new StyleBackground();
            }
        }

        void LockPlayerMovement(bool locked)
        {
            var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var p in players)
            {
                if (p != null) p.SetInputLocked(locked);
            }
        }

        void BuildRuntimeUi()
        {
            if (_container != null)
            {
                _container.style.position = Position.Absolute;
                _container.style.left = 0;
                _container.style.top = 0;
                _container.style.right = 0;
                _container.style.bottom = 0;
                _container.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                _container.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                _container.style.flexGrow = 1;
            }

            _root = _container.Q<VisualElement>("vn-dialogue-root");
            if (_root == null && _doc != null && _doc.visualTreeAsset != null)
            {
                _doc.visualTreeAsset.CloneTree(_container);
                _root = _container.Q<VisualElement>("vn-dialogue-root");
            }
            if (_root == null)
            {
                _root = _container.Q<VisualElement>(className: "vn-dialogue-root") ?? _container;
            }

            if (_root != null)
            {
                _root.style.position = Position.Absolute;
                _root.style.left = 0;
                _root.style.top = 0;
                _root.style.right = 0;
                _root.style.bottom = 0;
                _root.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                _root.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                _root.style.flexGrow = 1;

                var gradientBg = LoadBackground("UI/VN/vn_gradient_bg");
                if (gradientBg.value.texture != null || gradientBg.value.sprite != null)
                {
                    _root.style.backgroundImage = gradientBg;
                    _root.style.unityBackgroundScaleMode = ScaleMode.StretchToFill;
                }
            }

            _spriteLeft = _root.Q<VisualElement>("sprite-left");
            _spriteCenter = _root.Q<VisualElement>("sprite-center");
            _spriteRight = _root.Q<VisualElement>("sprite-right");
            _dialoguePanel = _root.Q<VisualElement>("dialogue-panel");
            _nameLabel = _root.Q<Label>("character-name");
            _counterLabel = _root.Q<Label>("line-counter");
            _textLabel = _root.Q<Label>("dialogue-text");
            _cursorLabel = _root.Q<Label>("typewriter-cursor");

            _btnBacklog = _root.Q<Button>("btn-backlog");
            _btnAuto = _root.Q<Button>("btn-auto");
            _btnSkip = _root.Q<Button>("btn-skip");
            _advancePrompt = _root.Q<Label>("advance-prompt");

            if (_btnSkip != null)
            {
                _btnSkip.clicked += () => StopDialogue();
            }
            if (_btnAuto != null)
            {
                _btnAuto.clicked += () =>
                {
                    _autoAdvance = !_autoAdvance;
                    _btnAuto.text = _autoAdvance ? "AUTO [ON]" : "AUTO";
                    if (_autoAdvance && !_typing)
                    {
                        StartAutoAdvanceTimer();
                    }
                };
            }
            if (_btnBacklog != null)
            {
                _btnBacklog.clicked += () =>
                {
                    Debug.Log($"[VN_Dialogue] Backlog requested. Current: {_current.characterName}: {_current.text}");
                };
            }

            if (_dialoguePanel != null)
            {
                _dialoguePanel.RegisterCallback<ClickEvent>(_ =>
                {
                    if (_typing)
                        CompleteTypewriter();
                    else
                        Advance();
                });
            }
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
            if (_autoCoroutine != null)
            {
                StopCoroutine(_autoCoroutine);
                _autoCoroutine = null;
            }

            if (_queue.Count == 0)
            {
                StopDialogue();
                return;
            }

            _current = _queue.Dequeue();

            // Character styling & name
            string name = _current.characterName ?? string.Empty;
            if (_nameLabel != null)
            {
                _nameLabel.text = name;
                _nameLabel.RemoveFromClassList("vn-character-name--aiden");
                _nameLabel.RemoveFromClassList("vn-character-name--lyra");

                if (name.IndexOf("aiden", StringComparison.OrdinalIgnoreCase) >= 0)
                    _nameLabel.AddToClassList("vn-character-name--aiden");
                else if (name.IndexOf("lyra", StringComparison.OrdinalIgnoreCase) >= 0)
                    _nameLabel.AddToClassList("vn-character-name--lyra");
            }

            // Update sprite positioning (Aiden left, Lyra right)
            UpdateCharacterPortraits(_current);

            // Play voice
            PlayVoice(_current.voiceClipPath);

            // Start typewriter
            if (_textLabel != null)
                _textLabel.text = string.Empty;

            _typing = true;
            if (_cursorLabel != null) _cursorLabel.style.visibility = Visibility.Visible;

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

            if (_autoAdvance)
            {
                StartAutoAdvanceTimer();
            }
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

            if (_autoAdvance)
            {
                StartAutoAdvanceTimer();
            }
        }

        void StartAutoAdvanceTimer()
        {
            if (_autoCoroutine != null) StopCoroutine(_autoCoroutine);
            _autoCoroutine = StartCoroutine(AutoAdvanceDelay());
        }

        IEnumerator AutoAdvanceDelay()
        {
            yield return new WaitForSecondsRealtime(1.8f);
            if (_active && !_typing)
                Advance();
        }

        void UpdateCharacterPortraits(DialogueLine line)
        {
            string name = line.characterName ?? "";
            bool isAiden = name.IndexOf("aiden", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isLyra = name.IndexOf("lyra", StringComparison.OrdinalIgnoreCase) >= 0;

            if (line.position == DialogueLine.SpritePosition.None)
            {
                if (_spriteLeft != null) { _spriteLeft.style.display = DisplayStyle.None; _spriteLeft.style.backgroundImage = new StyleBackground(); }
                if (_spriteRight != null) { _spriteRight.style.display = DisplayStyle.None; _spriteRight.style.backgroundImage = new StyleBackground(); }
                if (_spriteCenter != null) { _spriteCenter.style.display = DisplayStyle.None; _spriteCenter.style.backgroundImage = new StyleBackground(); }
                return;
            }

            if (line.position == DialogueLine.SpritePosition.Center)
            {
                if (_spriteLeft != null) { _spriteLeft.style.display = DisplayStyle.None; _spriteLeft.style.backgroundImage = new StyleBackground(); }
                if (_spriteRight != null) { _spriteRight.style.display = DisplayStyle.None; _spriteRight.style.backgroundImage = new StyleBackground(); }
                if (_spriteCenter != null)
                {
                    _spriteCenter.style.display = DisplayStyle.Flex;
                    if (!string.IsNullOrEmpty(line.spritePath))
                        _spriteCenter.style.backgroundImage = LoadBackground(line.spritePath);
                }
                return;
            }

            if (_spriteCenter != null) { _spriteCenter.style.display = DisplayStyle.None; _spriteCenter.style.backgroundImage = new StyleBackground(); }

            // Aiden on Left
            if (_spriteLeft != null)
            {
                _spriteLeft.style.borderTopLeftRadius = 8;
                _spriteLeft.style.borderTopRightRadius = 8;
                _spriteLeft.style.borderBottomLeftRadius = 8;
                _spriteLeft.style.borderBottomRightRadius = 8;

                if (isAiden || line.position == DialogueLine.SpritePosition.Left || (_sequenceHasAiden && !isLyra))
                {
                    string aidenPath = isAiden && !string.IsNullOrEmpty(line.spritePath) ? line.spritePath : "VN/Sprites/aiden/Aiden_Neutral";
                    var bg = LoadBackground(aidenPath);
                    _spriteLeft.style.backgroundImage = bg;
                    _spriteLeft.style.display = DisplayStyle.Flex;
                    _spriteLeft.RemoveFromClassList("vn-sprite--active");
                    _spriteLeft.RemoveFromClassList("vn-sprite--dimmed");
                    if (!_spriteLeft.ClassListContains("vn-sprite--aiden"))
                        _spriteLeft.AddToClassList("vn-sprite--aiden");

                    if (isAiden)
                    {
                        _spriteLeft.AddToClassList("vn-sprite--active");
                        // Bright active border for speaking Aiden
                        var activeCol = new Color(0.91f, 0.89f, 0.84f, 0.85f);
                        _spriteLeft.style.borderTopColor = activeCol;
                        _spriteLeft.style.borderBottomColor = activeCol;
                        _spriteLeft.style.borderLeftColor = activeCol;
                        _spriteLeft.style.borderRightColor = activeCol;
                        _spriteLeft.style.borderTopWidth = 2;
                        _spriteLeft.style.borderBottomWidth = 2;
                        _spriteLeft.style.borderLeftWidth = 2;
                        _spriteLeft.style.borderRightWidth = 2;
                    }
                    else
                    {
                        _spriteLeft.AddToClassList("vn-sprite--dimmed");
                        var dimCol = new Color(0.58f, 0.56f, 0.53f, 0.20f);
                        _spriteLeft.style.borderTopColor = dimCol;
                        _spriteLeft.style.borderBottomColor = dimCol;
                        _spriteLeft.style.borderLeftColor = dimCol;
                        _spriteLeft.style.borderRightColor = dimCol;
                        _spriteLeft.style.borderTopWidth = 1;
                        _spriteLeft.style.borderBottomWidth = 1;
                        _spriteLeft.style.borderLeftWidth = 1;
                        _spriteLeft.style.borderRightWidth = 1;
                    }
                }
                else if (_sequenceHasAiden && isLyra)
                {
                    string aidenPath = "VN/Sprites/aiden/Aiden_Neutral";
                    var bg = LoadBackground(aidenPath);
                    _spriteLeft.style.backgroundImage = bg;
                    _spriteLeft.style.display = DisplayStyle.Flex;
                    _spriteLeft.RemoveFromClassList("vn-sprite--active");
                    if (!_spriteLeft.ClassListContains("vn-sprite--aiden"))
                        _spriteLeft.AddToClassList("vn-sprite--aiden");
                    _spriteLeft.AddToClassList("vn-sprite--dimmed");

                    var dimCol = new Color(0.58f, 0.56f, 0.53f, 0.20f);
                    _spriteLeft.style.borderTopColor = dimCol;
                    _spriteLeft.style.borderBottomColor = dimCol;
                    _spriteLeft.style.borderLeftColor = dimCol;
                    _spriteLeft.style.borderRightColor = dimCol;
                    _spriteLeft.style.borderTopWidth = 1;
                    _spriteLeft.style.borderBottomWidth = 1;
                    _spriteLeft.style.borderLeftWidth = 1;
                    _spriteLeft.style.borderRightWidth = 1;
                }
                else
                {
                    _spriteLeft.style.display = DisplayStyle.None;
                    _spriteLeft.style.backgroundImage = new StyleBackground();
                }
            }

            // Lyra on Right (ONLY shown if Lyra is present/speaking in this scene!)
            if (_spriteRight != null)
            {
                _spriteRight.style.borderTopLeftRadius = 8;
                _spriteRight.style.borderTopRightRadius = 8;
                _spriteRight.style.borderBottomLeftRadius = 8;
                _spriteRight.style.borderBottomRightRadius = 8;

                if (isLyra || line.position == DialogueLine.SpritePosition.Right)
                {
                    string lyraPath = isLyra && !string.IsNullOrEmpty(line.spritePath) ? line.spritePath : "VN/Sprites/lyra/Lyra_Neutral";
                    var bg = LoadBackground(lyraPath);
                    _spriteRight.style.backgroundImage = bg;
                    _spriteRight.style.display = DisplayStyle.Flex;
                    _spriteRight.RemoveFromClassList("vn-sprite--active");
                    _spriteRight.RemoveFromClassList("vn-sprite--dimmed");
                    if (!_spriteRight.ClassListContains("vn-sprite--lyra"))
                        _spriteRight.AddToClassList("vn-sprite--lyra");

                    if (isLyra)
                    {
                        _spriteRight.AddToClassList("vn-sprite--active");
                        // Bright glowing cyan border for speaking Lyra!
                        var activeCol = new Color(0.39f, 0.83f, 0.98f, 0.90f);
                        _spriteRight.style.borderTopColor = activeCol;
                        _spriteRight.style.borderBottomColor = activeCol;
                        _spriteRight.style.borderLeftColor = activeCol;
                        _spriteRight.style.borderRightColor = activeCol;
                        _spriteRight.style.borderTopWidth = 2;
                        _spriteRight.style.borderBottomWidth = 2;
                        _spriteRight.style.borderLeftWidth = 2;
                        _spriteRight.style.borderRightWidth = 2;
                    }
                    else
                    {
                        _spriteRight.AddToClassList("vn-sprite--dimmed");
                        var dimCol = new Color(0.58f, 0.56f, 0.53f, 0.20f);
                        _spriteRight.style.borderTopColor = dimCol;
                        _spriteRight.style.borderBottomColor = dimCol;
                        _spriteRight.style.borderLeftColor = dimCol;
                        _spriteRight.style.borderRightColor = dimCol;
                        _spriteRight.style.borderTopWidth = 1;
                        _spriteRight.style.borderBottomWidth = 1;
                        _spriteRight.style.borderLeftWidth = 1;
                        _spriteRight.style.borderRightWidth = 1;
                    }
                }
                else if (_sequenceHasLyra && isAiden)
                {
                    string lyraPath = "VN/Sprites/lyra/Lyra_Neutral";
                    var bg = LoadBackground(lyraPath);
                    _spriteRight.style.backgroundImage = bg;
                    _spriteRight.style.display = DisplayStyle.Flex;
                    _spriteRight.RemoveFromClassList("vn-sprite--active");
                    if (!_spriteRight.ClassListContains("vn-sprite--lyra"))
                        _spriteRight.AddToClassList("vn-sprite--lyra");
                    _spriteRight.AddToClassList("vn-sprite--dimmed");

                    var dimCol = new Color(0.58f, 0.56f, 0.53f, 0.20f);
                    _spriteRight.style.borderTopColor = dimCol;
                    _spriteRight.style.borderBottomColor = dimCol;
                    _spriteRight.style.borderLeftColor = dimCol;
                    _spriteRight.style.borderRightColor = dimCol;
                    _spriteRight.style.borderTopWidth = 1;
                    _spriteRight.style.borderBottomWidth = 1;
                    _spriteRight.style.borderLeftWidth = 1;
                    _spriteRight.style.borderRightWidth = 1;
                }
                else
                {
                    // Lyra is NOT in this scene - completely hidden
                    _spriteRight.style.display = DisplayStyle.None;
                    _spriteRight.style.backgroundImage = new StyleBackground();
                }
            }
        }

        StyleBackground LoadBackground(string path)
        {
            if (string.IsNullOrEmpty(path)) return new StyleBackground();

            var sprite = Resources.Load<Sprite>(path);
            if (sprite != null) return new StyleBackground(sprite);

            var tex = Resources.Load<Texture2D>(path);
            if (tex != null) return new StyleBackground(tex);

            return new StyleBackground();
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

