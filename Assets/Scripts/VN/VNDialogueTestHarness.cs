using System;
using System.Collections.Generic;
using UnityEngine;

namespace Echoes.VN
{
    /// <summary>
    /// Self-contained visual-novel test screen. It deliberately uses IMGUI
    /// instead of UIDocument so it can prove that rendering, input and sprites
    /// work independently from the production UI Toolkit document.
    /// </summary>
    public sealed class VNDialogueTestHarness : MonoBehaviour
    {
        [Serializable]
        struct TestLine
        {
            public string speaker;
            [TextArea] public string text;
            public string spriteResourcePath;

            public TestLine(string speaker, string text, string spriteResourcePath)
            {
                this.speaker = speaker;
                this.text = text;
                this.spriteResourcePath = spriteResourcePath;
            }
        }

        const float ReferenceWidth = 1920f;
        const float ReferenceHeight = 1080f;
        const string DefaultSprite = "VN/Sprites/aiden/Aiden_Perturbada";

        [SerializeField] Camera testCamera;
        [SerializeField, Min(1f)] float charactersPerSecond = 55f;

        readonly List<TestLine> _lines = new();
        int _lineIndex;
        float _lineStartedAt;
        bool _dialogueOpen;
        Sprite _portrait;
        string _spriteStatus = "Cargando retrato...";

        public void SetTestCamera(Camera value) => testCamera = value;

        void Start()
        {
            CreateTestWorld();
            StartDialogue(new[]
            {
                new TestLine("Aiden", "Esta es una escena de prueba independiente. Si ves este cuadro y mi retrato, el renderizado de la novela visual funciona.", DefaultSprite),
                new TestLine("Sistema", "Haz clic en los tres objetos del escenario para abrir diálogos distintos. E o clic avanza; Espacio cierra el diálogo.", DefaultSprite)
            });
        }

        void CreateTestWorld()
        {
            if (GameObject.Find("VN Test Interactables") != null) return;

            var root = new GameObject("VN Test Interactables");
            CreateTestObject(root.transform, "Reloj detenido", new Vector3(-4f, 1f, 0f), PrimitiveType.Cylinder,
                "El reloj marca las 03:17. Aiden parece recordar algo que aun no sucedio.", "VN/Sprites/aiden/Aiden_Pensativa", new Color(0.34f, 0.78f, 1f), Vector3.one);
            CreateTestObject(root.transform, "Nota doblada", new Vector3(0f, 1f, 0f), PrimitiveType.Cube,
                "La tinta dice: No olvides que la salida tambien puede ser un comienzo.", "VN/Sprites/aiden/Aiden_Perturbada", new Color(1f, 0.72f, 0.22f), Vector3.one);
            CreateTestObject(root.transform, "Espejo empañado", new Vector3(4f, 1.4f, 0f), PrimitiveType.Cube,
                "Por un instante, el reflejo de Aiden sonríe antes que tú.", "VN/Sprites/aiden/Aiden_Feliz", new Color(0.72f, 0.48f, 1f), new Vector3(1.7f, 2.8f, 0.25f));

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "VN Test Floor";
            floor.transform.SetParent(root.transform);
            floor.transform.localScale = new Vector3(2f, 1f, 1.2f);
            Colorize(floor, new Color(0.045f, 0.07f, 0.12f));
        }

        static void CreateTestObject(Transform parent, string title, Vector3 position, PrimitiveType primitiveType, string text, string spritePath, Color color, Vector3 scale)
        {
            var target = GameObject.CreatePrimitive(primitiveType);
            target.name = "Interactuable — " + title;
            target.transform.SetParent(parent);
            target.transform.position = position;
            target.transform.localScale = scale;
            target.AddComponent<VNDialogueTestInteractable>().Configure(title, text, spritePath);
            Colorize(target, color);
        }

        static void Colorize(GameObject target, Color color)
        {
            var renderer = target.GetComponent<Renderer>();
            if (renderer == null) return;
            renderer.material.color = color;
        }

        void Update()
        {
            if (_dialogueOpen)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    CloseDialogue();
                    return;
                }

                if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
                    Advance();
                return;
            }

            if (Input.GetMouseButtonDown(0))
                TryInteractAtMouse();
        }

        public void StartInteraction(string objectName, string message, string spriteResourcePath)
        {
            StartDialogue(new[]
            {
                new TestLine(objectName, message, spriteResourcePath),
                new TestLine("Sistema", "Interacción registrada. Pulsa E o haz clic para terminar y probar otro objeto.", spriteResourcePath)
            });
        }

        void TryInteractAtMouse()
        {
            var cameraToUse = testCamera != null ? testCamera : Camera.main;
            if (cameraToUse == null) return;

            Ray ray = cameraToUse.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 100f)) return;

            hit.collider.GetComponent<VNDialogueTestInteractable>()?.Interact(this);
        }

        void StartDialogue(IEnumerable<TestLine> lines)
        {
            _lines.Clear();
            _lines.AddRange(lines);
            _lineIndex = 0;
            _dialogueOpen = _lines.Count > 0;
            SetPortrait(_dialogueOpen ? _lines[0].spriteResourcePath : DefaultSprite);
            _lineStartedAt = Time.unscaledTime;
        }

        void Advance()
        {
            if (_lineIndex >= _lines.Count) return;

            if (VisibleCharacterCount(_lines[_lineIndex].text) < _lines[_lineIndex].text.Length)
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

        void CloseDialogue()
        {
            _dialogueOpen = false;
            _lines.Clear();
        }

        void SetPortrait(string resourcePath)
        {
            _portrait = Resources.Load<Sprite>(string.IsNullOrWhiteSpace(resourcePath) ? DefaultSprite : resourcePath);
            _spriteStatus = _portrait != null ? "Retrato cargado: " + _portrait.name : "ERROR: no se encontro el Sprite en Resources";
        }

        int VisibleCharacterCount(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            return Mathf.Clamp(Mathf.FloorToInt((Time.unscaledTime - _lineStartedAt) * charactersPerSecond), 0, text.Length);
        }

        void OnGUI()
        {
            float scale = Mathf.Min(Screen.width / ReferenceWidth, Screen.height / ReferenceHeight);
            float offsetX = (Screen.width - ReferenceWidth * scale) * 0.5f;
            float offsetY = (Screen.height - ReferenceHeight * scale) * 0.5f;
            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(offsetX, offsetY, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));

            DrawHeader();
            if (_dialogueOpen && _lineIndex < _lines.Count)
                DrawDialogue(_lines[_lineIndex]);
            else
                DrawInteractionHint();

            GUI.matrix = oldMatrix;
        }

        void DrawHeader()
        {
            DrawBox(new Rect(36f, 30f, 620f, 110f), new Color(0.015f, 0.025f, 0.05f, 0.92f), new Color(1f, 0.7f, 0.18f, 1f), 2f);
            GUI.Label(new Rect(62f, 47f, 560f, 36f), "ECHOES OF YOU — PRUEBA VN", Style(25, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold));
            GUI.Label(new Rect(62f, 87f, 560f, 28f), _spriteStatus, Style(17, _portrait != null ? new Color(0.55f, 1f, 0.72f) : new Color(1f, 0.35f, 0.35f), TextAnchor.MiddleLeft));
        }

        void DrawInteractionHint()
        {
            DrawBox(new Rect(420f, 455f, 1080f, 150f), new Color(0.015f, 0.025f, 0.05f, 0.95f), new Color(1f, 0.7f, 0.18f, 1f), 2f);
            GUI.Label(new Rect(450f, 478f, 1020f, 40f), "PRUEBA DE INTERACCION", Style(27, new Color(1f, 0.7f, 0.18f), TextAnchor.MiddleCenter, FontStyle.Bold));
            GUI.Label(new Rect(450f, 526f, 1020f, 42f), "Haz clic sobre el reloj, la nota o el espejo para iniciar un dialogo.", Style(22, Color.white, TextAnchor.MiddleCenter));
        }

        void DrawDialogue(TestLine line)
        {
            GUI.DrawTexture(new Rect(0f, 0f, ReferenceWidth, ReferenceHeight), Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, new Color(0f, 0f, 0.02f, 0.28f), 0f, 0f);
            DrawPortrait();
            DrawBox(new Rect(60f, 748f, 1800f, 280f), new Color(0.012f, 0.02f, 0.045f, 0.98f), new Color(1f, 0.7f, 0.18f, 1f), 3f);
            GUI.Label(new Rect(96f, 775f, 600f, 40f), line.speaker, Style(30, new Color(1f, 0.7f, 0.18f), TextAnchor.MiddleLeft, FontStyle.Bold));

            string fullText = line.text ?? string.Empty;
            string visibleText = fullText.Substring(0, VisibleCharacterCount(fullText));
            GUI.Label(new Rect(96f, 835f, 1660f, 112f), visibleText, Style(27, Color.white, TextAnchor.UpperLeft));
            GUI.Label(new Rect(96f, 970f, 1660f, 32f), "[E / clic] avanzar    [Espacio] cerrar", Style(18, new Color(1f, 0.7f, 0.18f), TextAnchor.MiddleRight));
        }

        void DrawPortrait()
        {
            Rect portraitRect = new Rect(90f, 165f, 560f, 560f);
            DrawBox(portraitRect, new Color(0.04f, 0.06f, 0.11f, 0.88f), new Color(1f, 0.7f, 0.18f, 0.8f), 2f);
            if (_portrait == null)
            {
                GUI.Label(portraitRect, "RETRATO\nNO CARGADO", Style(28, Color.magenta, TextAnchor.MiddleCenter, FontStyle.Bold));
                return;
            }

            Rect textureRect = _portrait.textureRect;
            Rect uv = new Rect(textureRect.x / _portrait.texture.width, textureRect.y / _portrait.texture.height, textureRect.width / _portrait.texture.width, textureRect.height / _portrait.texture.height);
            GUI.DrawTextureWithTexCoords(new Rect(105f, 180f, 530f, 530f), _portrait.texture, uv, true);
        }

        static GUIStyle Style(int fontSize, Color color, TextAnchor alignment, FontStyle fontStyle = FontStyle.Normal)
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
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, fill, 0f, 0f);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, outlineWidth), Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, outline, 0f, 0f);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - outlineWidth, rect.width, outlineWidth), Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, outline, 0f, 0f);
            GUI.DrawTexture(new Rect(rect.x, rect.y, outlineWidth, rect.height), Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, outline, 0f, 0f);
            GUI.DrawTexture(new Rect(rect.xMax - outlineWidth, rect.y, outlineWidth, rect.height), Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, outline, 0f, 0f);
        }
    }
}
