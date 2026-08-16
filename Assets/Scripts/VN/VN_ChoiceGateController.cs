using System;
using Echoes.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Echoes.VN
{
    public class VN_ChoiceGateController : MonoBehaviour
    {
        public static VN_ChoiceGateController Instance { get; private set; }
        public bool IsShowing => _choiceActive;

        const float RefW = 1920f;
        const float RefH = 1080f;
        const float InputGrace = 0.3f;

        [SerializeField] VN_ChoiceRegistry registry;
        [SerializeField] float fadeOutSeconds = 0.3f;

        bool _choiceActive;
        string _promptText = "";
        string _cyanLabel = "Cyan";
        string _amberLabel = "Amber";
        int _selectedIndex;
        float _openedAt;
        Action<bool> _onComplete;
        VN_ChoiceNode _currentNode;
        Texture2D _backgroundTex;
        Texture2D _portraitTex;
        Rect _portraitUV;
        SimpleFollowCamera _cachedCam;

        float CurrentScale => Mathf.Min(Screen.width / RefW, Screen.height / RefH);

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

        public void Show(int levelIndex, bool isMicro, Action<bool> onComplete)
        {
            if (registry == null)
            {
                registry = Resources.Load<VN_ChoiceRegistry>("VN_ChoiceRegistry");
                if (registry == null)
                {
                    Debug.LogWarning("[VN_ChoiceGate] Registry missing.");
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

            var entry = VN_TextTable.GetChoice(_currentNode.NodeId);
            _promptText = entry != null && !string.IsNullOrEmpty(entry.prompt) ? entry.prompt : "...";
            _cyanLabel = entry != null && !string.IsNullOrEmpty(entry.cyan_label) ? entry.cyan_label : "Abrir";
            _amberLabel = entry != null && !string.IsNullOrEmpty(entry.amber_label) ? entry.amber_label : "Mantener";

            _onComplete = onComplete;
            _selectedIndex = 0;
            _openedAt = Time.unscaledTime;
            _choiceActive = true;

            _backgroundTex = Resources.Load<Texture2D>("UI/void_fog_bg");

            var stage = AidenStageResolver.ResolveForCurrentLevel();
            string spriteName = stage switch
            {
                AidenStage.Conviction => "VN/Sprites/aiden/Aiden_Perturbada",
                AidenStage.Guilt => "VN/Sprites/aiden/Aiden_Pensativa",
                _ => "VN/Sprites/aiden/Aiden_Feliz",
            };
            var sp = Resources.Load<Sprite>(spriteName);
            if (sp != null)
            {
                _portraitTex = sp.texture;
                var tr = sp.textureRect;
                _portraitUV = new Rect(tr.x / _portraitTex.width, tr.y / _portraitTex.height, tr.width / _portraitTex.width, tr.height / _portraitTex.height);
            }
            else
            {
                _portraitTex = Resources.Load<Texture2D>(spriteName);
                _portraitUV = new Rect(0f, 0f, 1f, 1f);
            }

            var hud = FindAnyObjectByType<GameHUD>();
            hud?.SetVisible(false);

            FreezeCamera();
        }

        void Update()
        {
            if (!_choiceActive) return;
            if (Time.unscaledTime - _openedAt < InputGrace) return;

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                _selectedIndex = 0;
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                _selectedIndex = 1;
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                _selectedIndex = Mathf.Max(0, _selectedIndex - 1);
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                _selectedIndex = Mathf.Min(1, _selectedIndex + 1);

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
                OnChoiceMade(_selectedIndex == 0);
        }

        void OnGUI()
        {
            if (!_choiceActive) return;

            float scale = CurrentScale;
            float ox = (Screen.width - RefW * scale) * 0.5f;
            float oy = (Screen.height - RefH * scale) * 0.5f;
            var oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(ox, oy, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));

            DrawFullBackground();
            DrawPortrait();
            DrawChoiceBox(scale);

            GUI.matrix = oldMatrix;
        }

        static void DrawColored(Rect rect, Color c)
        {
            GUI.color = c;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        void DrawFullBackground()
        {
            if (_backgroundTex != null)
            {
                GUI.DrawTexture(new Rect(0f, 0f, RefW, RefH), _backgroundTex, ScaleMode.ScaleAndCrop);
            }
            DrawColored(new Rect(0f, 0f, RefW, RefH), new Color(0f, 0f, 0.04f, 0.78f));
        }

        void DrawPortrait()
        {
            Rect pr = new Rect(120f, 200f, 480f, 600f);
            DrawBox(pr, new Color(0.04f, 0.06f, 0.11f, 0.9f), new Color(1f, 0.7f, 0.18f, 0.7f), 3f);
            if (_portraitTex != null)
                GUI.DrawTextureWithTexCoords(new Rect(135f, 215f, 450f, 570f), _portraitTex, _portraitUV, true);
        }

        void DrawChoiceBox(float scale)
        {
            Rect boxRect = new Rect(660f, 320f, 1120f, 440f);
            DrawBox(boxRect, new Color(0.01f, 0.02f, 0.05f, 0.95f), new Color(1f, 0.7f, 0.18f, 1f), 4f);

            int titleSize = Mathf.RoundToInt(36 * scale);
            GUI.Label(new Rect(boxRect.x + 50f, boxRect.y + 40f, boxRect.width - 100f, 50f), "Elige tu camino", ScaledStyle(titleSize, new Color(1f, 0.7f, 0.18f), TextAnchor.MiddleCenter, FontStyle.Bold));

            int promptSize = Mathf.RoundToInt(24 * scale);
            GUI.Label(new Rect(boxRect.x + 50f, boxRect.y + 105f, boxRect.width - 100f, 90f), _promptText, ScaledStyle(promptSize, new Color(0.9f, 0.9f, 0.95f), TextAnchor.MiddleCenter));

            float bw = (boxRect.width - 130f) / 2f;
            float bh = 70f;
            float by = boxRect.y + 230f;

            DrawOptionCard(new Rect(boxRect.x + 50f, by, bw, bh), 0, _cyanLabel, new Color(0.2f, 0.85f, 1f), scale);
            DrawOptionCard(new Rect(boxRect.x + 80f + bw, by, bw, bh), 1, _amberLabel, new Color(1f, 0.72f, 0.22f), scale);

            int hintSize = Mathf.RoundToInt(18 * scale);
            GUI.Label(new Rect(boxRect.x + 50f, boxRect.yMax - 50f, boxRect.width - 100f, 30f), "[← →] navegar    [Enter / E] confirmar", ScaledStyle(hintSize, new Color(0.6f, 0.6f, 0.65f), TextAnchor.MiddleCenter));
        }

        void DrawOptionCard(Rect rect, int index, string label, Color accent, float scale)
        {
            bool selected = _selectedIndex == index;
            Color fill = selected ? new Color(accent.r * 0.15f, accent.g * 0.15f, accent.b * 0.15f, 0.95f) : new Color(0.03f, 0.04f, 0.06f, 0.8f);
            Color border = selected ? accent : new Color(accent.r * 0.3f, accent.g * 0.3f, accent.b * 0.3f, 0.5f);
            float bw = selected ? 4f : 2f;
            DrawBox(rect, fill, border, bw);

            var mouseRect = new Rect(rect.x * CurrentScale + (Screen.width - RefW * CurrentScale) * 0.5f, rect.y * CurrentScale + (Screen.height - RefH * CurrentScale) * 0.5f, rect.width * CurrentScale, rect.height * CurrentScale);
            if (mouseRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.Repaint)
            {
                if (_selectedIndex != index) _selectedIndex = index;
            }

            int labelSize = Mathf.RoundToInt(22 * scale);
            Color textColor = selected ? accent : new Color(0.7f, 0.7f, 0.75f);
            GUI.Label(rect, label, ScaledStyle(labelSize, textColor, TextAnchor.MiddleCenter, selected ? FontStyle.Bold : FontStyle.Normal));

            if (selected)
            {
                DrawColored(new Rect(rect.x - 8f, rect.y - 8f, rect.width + 16f, 3f), accent);
                DrawColored(new Rect(rect.x - 8f, rect.yMax + 5f, rect.width + 16f, 3f), accent);
            }
        }

        void OnChoiceMade(bool cyan)
        {
            if (!_choiceActive || _currentNode == null) return;
            _choiceActive = false;
            string flagKey = cyan ? _currentNode.CyanFlag : _currentNode.AmberFlag;
            VN_EndingFlags.Instance?.SetFlag(flagKey, true);
            VN_EndingFlags.Instance?.SaveToDisk();
            StartCoroutine(FadeOutAndCallback());
        }

        System.Collections.IEnumerator FadeOutAndCallback()
        {
            yield return new WaitForSecondsRealtime(fadeOutSeconds);
            UnfreezeCamera();
            var hud = FindAnyObjectByType<GameHUD>();
            hud?.SetVisible(true);
            var cb = _onComplete;
            _onComplete = null;
            _currentNode = null;
            cb?.Invoke(true);
        }

        void FreezeCamera()
        {
            if (_cachedCam == null) _cachedCam = FindAnyObjectByType<SimpleFollowCamera>();
            if (_cachedCam != null) _cachedCam.Frozen = true;
        }

        void UnfreezeCamera()
        {
            if (_cachedCam == null) _cachedCam = FindAnyObjectByType<SimpleFollowCamera>();
            if (_cachedCam != null) _cachedCam.Frozen = false;
        }

        static GUIStyle ScaledStyle(int fontSize, Color color, TextAnchor alignment, FontStyle fontStyle = FontStyle.Normal)
        {
            return new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                normal = { textColor = color },
                alignment = alignment,
                fontStyle = fontStyle,
                wordWrap = true
            };
        }

        static void DrawBox(Rect rect, Color fill, Color outline, float outlineWidth)
        {
            DrawColored(rect, fill);
            DrawColored(new Rect(rect.x, rect.y, rect.width, outlineWidth), outline);
            DrawColored(new Rect(rect.x, rect.yMax - outlineWidth, rect.width, outlineWidth), outline);
            DrawColored(new Rect(rect.x, rect.y, outlineWidth, rect.height), outline);
            DrawColored(new Rect(rect.xMax - outlineWidth, rect.y, outlineWidth, rect.height), outline);
        }
    }
}
