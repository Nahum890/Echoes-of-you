using System;
using System.Collections.Generic;
using UnityEngine;

namespace Echoes.VN
{
    [Serializable]
    public struct VN_Line
    {
        public string speaker;
        [TextArea] public string text;
        public string spriteResourcePath;
    }

    public class VN_OverlayController : MonoBehaviour
    {
        public static VN_OverlayController Instance { get; private set; }

        const float RefW = 1920f;
        const float RefH = 1080f;
        const string DefaultSprite = "VN/Sprites/aiden/Aiden_Perturbada";
        const float InputGracePeriod = 0.35f;

        [SerializeField, Min(1f)] float charactersPerSecond = 55f;

        readonly List<VN_Line> _lines = new();
        int _lineIndex;
        float _lineStartedAt;
        float _openedAt;
        bool _dialogueOpen;
        Texture2D _portraitTexture;
        Rect _portraitUV;
        SimpleFollowCamera _cachedCamera;

        float CurrentScale => Mathf.Min(Screen.width / RefW, Screen.height / RefH);

        public bool IsOpen => _dialogueOpen;

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

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void Update()
        {
            if (!_dialogueOpen) return;

            if (Time.unscaledTime - _openedAt < InputGracePeriod) return;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                CloseDialogue();
                return;
            }

            if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
                Advance();
        }

        public void PlaySequence(IEnumerable<VN_Line> lines)
        {
            _lines.Clear();
            _lines.AddRange(lines);
            _lineIndex = 0;
            _dialogueOpen = _lines.Count > 0;
            _openedAt = Time.unscaledTime;
            if (_dialogueOpen)
            {
                SetPortrait(_lines[0].spriteResourcePath);
                FreezeCamera();
            }
            _lineStartedAt = Time.unscaledTime;
        }

        public void Play(string speaker, string text, string spritePath = "")
        {
            PlaySequence(new[] { new VN_Line { speaker = speaker, text = text, spriteResourcePath = spritePath } });
        }

        public void CloseDialogue()
        {
            _dialogueOpen = false;
            _lines.Clear();
            UnfreezeCamera();
        }

        void Advance()
        {
            if (_lineIndex >= _lines.Count) return;

            if (VisibleCharCount(_lines[_lineIndex].text) < _lines[_lineIndex].text.Length)
            {
                _lineStartedAt = Time.unscaledTime - _lines[_lineIndex].text.Length / charactersPerSecond;
                return;
            }

            _lineIndex++;
            if (_lineIndex >= _lines.Count)
            {
                CloseDialogue();
                return;
            }

            SetPortrait(_lines[_lineIndex].spriteResourcePath);
            _lineStartedAt = Time.unscaledTime;
        }

        void SetPortrait(string resourcePath)
        {
            string path = string.IsNullOrWhiteSpace(resourcePath) ? DefaultSprite : resourcePath;
            var sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
            {
                _portraitTexture = sprite.texture;
                var tr = sprite.textureRect;
                _portraitUV = new Rect(
                    tr.x / _portraitTexture.width,
                    tr.y / _portraitTexture.height,
                    tr.width / _portraitTexture.width,
                    tr.height / _portraitTexture.height);
                return;
            }

            _portraitTexture = Resources.Load<Texture2D>(path);
            if (_portraitTexture == null)
                _portraitTexture = Resources.Load<Texture2D>(DefaultSprite);
            _portraitUV = new Rect(0f, 0f, 1f, 1f);
        }

        void FreezeCamera()
        {
            if (_cachedCamera == null)
                _cachedCamera = FindAnyObjectByType<SimpleFollowCamera>();
            if (_cachedCamera != null)
                _cachedCamera.Frozen = true;
        }

        void UnfreezeCamera()
        {
            if (_cachedCamera == null)
                _cachedCamera = FindAnyObjectByType<SimpleFollowCamera>();
            if (_cachedCamera != null)
                _cachedCamera.Frozen = false;
        }

        int VisibleCharCount(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            return Mathf.Clamp(Mathf.FloorToInt((Time.unscaledTime - _lineStartedAt) * charactersPerSecond), 0, text.Length);
        }

        void OnGUI()
        {
            if (!_dialogueOpen || _lineIndex >= _lines.Count) return;

            float scale = CurrentScale;
            float ox = (Screen.width - RefW * scale) * 0.5f;
            float oy = (Screen.height - RefH * scale) * 0.5f;
            var oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(ox, oy, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));

            DrawDimBackground();
            DrawPortrait(scale);
            DrawDialogueBox(_lines[_lineIndex], scale);

            GUI.matrix = oldMatrix;
        }

        static void DrawColored(Rect rect, Color c)
        {
            GUI.color = c;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        static void DrawDimBackground()
        {
            DrawColored(new Rect(0f, 0f, RefW, RefH), new Color(0f, 0f, 0.02f, 0.55f));
        }

        void DrawPortrait(float scale)
        {
            Rect portraitRect = new Rect(80f, 140f, 560f, 580f);
            DrawBox(portraitRect, new Color(0.04f, 0.06f, 0.11f, 0.92f), new Color(1f, 0.7f, 0.18f, 0.85f), 3f);

            if (_portraitTexture == null)
            {
                GUI.Label(portraitRect, "Retrato no cargado", ScaledStyle(Mathf.RoundToInt(26 * scale), new Color(1f, 0.3f, 0.6f), TextAnchor.MiddleCenter, FontStyle.Bold));
                return;
            }

            GUI.DrawTextureWithTexCoords(new Rect(95f, 155f, 530f, 550f), _portraitTexture, _portraitUV, true);
        }

        static void DrawDialogueBox(VN_Line line, float scale)
        {
            DrawBox(new Rect(680f, 748f, 1180f, 280f), new Color(0.012f, 0.02f, 0.045f, 0.97f), new Color(1f, 0.7f, 0.18f, 1f), 3f);

            int spName = Mathf.RoundToInt(30 * scale);
            GUI.Label(new Rect(716f, 770f, 600f, 42f), line.speaker ?? "", ScaledStyle(spName, new Color(1f, 0.7f, 0.18f), TextAnchor.MiddleLeft, FontStyle.Bold));

            string fullText = line.text ?? string.Empty;
            string visibleText = fullText.Substring(0, VisibleCharCountLocal(fullText));
            int bodySize = Mathf.RoundToInt(27 * scale);
            GUI.Label(new Rect(716f, 826f, 1090f, 130f), visibleText, ScaledStyle(bodySize, Color.white, TextAnchor.UpperLeft));

            int hintSize = Mathf.RoundToInt(18 * scale);
            GUI.Label(new Rect(716f, 970f, 1100f, 32f), "[E / clic] avanzar    [Espacio] cerrar", ScaledStyle(hintSize, new Color(1f, 0.7f, 0.18f, 0.8f), TextAnchor.MiddleRight));
        }

        static int VisibleCharCountLocal(string text)
        {
            if (Instance == null || string.IsNullOrEmpty(text)) return 0;
            return Mathf.Clamp(Mathf.FloorToInt((Time.unscaledTime - Instance._lineStartedAt) * Instance.charactersPerSecond), 0, text.Length);
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
