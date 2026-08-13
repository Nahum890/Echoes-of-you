using System;
using System.Collections;
using Echoes.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Echoes.VN
{
    public class VN_ChoiceGateController : MonoBehaviour
    {
        public static VN_ChoiceGateController Instance { get; private set; }

        public bool IsShowing => _active;

        [SerializeField] VN_ChoiceRegistry registry;
        [SerializeField] UIDocument document;
        [SerializeField] float fadeOutSeconds = 0.2f;

        VisualElement _root;
        Label _promptLabel;
        Button _cyanButton;
        Button _amberButton;
        Label _hintLabel;
        VisualElement _spriteLeft;
        VisualElement _spriteRight;

        bool _active;
        Action<bool> _onComplete;
        VN_ChoiceNode _currentNode;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable()
        {
            if (document == null) document = GetComponent<UIDocument>();
            if (document == null) return;
            _root = document.rootVisualElement;
            if (_root == null) return;
            _promptLabel = _root.Q<Label>("gate-prompt");
            _cyanButton = _root.Q<Button>("gate-cyan");
            _amberButton = _root.Q<Button>("gate-amber");
            _hintLabel = _root.Q<Label>("gate-hint");
            _spriteLeft  = _root.Q("gate-sprite-left");
            _spriteRight = _root.Q("gate-sprite-right");
            _root.AddToClassList("hidden");
            if (_cyanButton != null) _cyanButton.clicked += () => OnChoiceMade(true);
            if (_amberButton != null) _amberButton.clicked += () => OnChoiceMade(false);
        }

        void Update()
        {
            if (!_active || _currentNode == null) return;
            if (Input.GetKeyDown(KeyCode.A)) OnChoiceMade(true);
            else if (Input.GetKeyDown(KeyCode.D)) OnChoiceMade(false);
        }

        public void Show(int levelIndex, bool isMicro, Action<bool> onComplete)
        {
            if (registry == null)
            {
                // Fallback robusto: cargar desde Resources si no asignado en Inspector.
                registry = Resources.Load<VN_ChoiceRegistry>("VN_ChoiceRegistry");
                if (registry == null)
                {
                    Debug.LogWarning("[VN_ChoiceGate] Registry null — creando runtime fallback vacío.");
                    registry = ScriptableObject.CreateInstance<VN_ChoiceRegistry>();
                }
            }

            _currentNode = registry.GetNode(levelIndex, isMicro);
            if (_currentNode == null)
            {
                Debug.LogWarning($"[VN_ChoiceGate] Node not found for L{levelIndex} micro={isMicro}");
                onComplete?.Invoke(true);
                return;
            }
            _onComplete = onComplete;
            var hud = FindAnyObjectByType<GameHUD>();
            if (hud != null) hud.SetVisible(false);

            var entry = VN_TextTable.GetChoice(_currentNode.NodeId);
            if (_promptLabel != null) _promptLabel.text = entry != null && !string.IsNullOrEmpty(entry.prompt) ? entry.prompt : "...";
            if (_cyanButton != null) _cyanButton.text = entry != null && !string.IsNullOrEmpty(entry.cyan_label) ? entry.cyan_label : "Cyan";
            if (_amberButton != null) _amberButton.text = entry != null && !string.IsNullOrEmpty(entry.amber_label) ? entry.amber_label : "Amber";
            if (_hintLabel != null) _hintLabel.text = "A = abrir  /  D = mantener";

            if (_root != null) _root.RemoveFromClassList("hidden");
            UpdateAidenSprite();
            _active = true;
        }

        void UpdateAidenSprite()
        {
            if (_spriteLeft == null && _spriteRight == null) return;

            var stage = AidenStageResolver.ResolveForCurrentLevel();
            string spriteName = stage switch
            {
                AidenStage.Conviction => "VN/Sprites/aiden/Aiden_Perturbada",
                AidenStage.Guilt => "VN/Sprites/aiden/Aiden_Pensativa",
                _ => "VN/Sprites/aiden/Aiden_Feliz",
            };

            Texture2D tex = Resources.Load<Texture2D>(spriteName);
            var bg = tex != null ? new StyleBackground(tex) : new StyleBackground();
            if (_spriteRight != null) _spriteRight.style.backgroundImage = bg;
            if (_spriteLeft != null) _spriteLeft.style.backgroundImage = new StyleBackground();
        }

        void OnChoiceMade(bool cyan)
        {
            if (!_active || _currentNode == null) return;
            _active = false;
            string key = cyan ? _currentNode.CyanFlag : _currentNode.AmberFlag;
            VN_EndingFlags.Instance?.SetFlag(key, true);
            StartCoroutine(FadeOutThenCallback());
        }

        IEnumerator FadeOutThenCallback()
        {
            yield return new WaitForSecondsRealtime(fadeOutSeconds);
            if (_root != null) _root.AddToClassList("hidden");
            var cb = _onComplete;
            _onComplete = null;
            _currentNode = null;
            cb?.Invoke(true);
        }
    }
}
