using System;
using System.Collections.Generic;
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
            if (Application.isPlaying)
                DontDestroyOnLoad(gameObject);
        }

        public void Show(int levelIndex, bool isMicro, Action<bool> onComplete)
        {
            // Load registry if needed
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

            // Store completion callback
            _onComplete = onComplete;
            _selectedIndex = 0;
            _openedAt = Time.unscaledTime;

            // Play a short pre‑decision dialogue before showing the choice UI
            StartCoroutine(PlayPreDialogueAndShow(levelIndex));
        }

        // Coroutine: play contextual dialogue, then initialise the choice overlay UI
        private System.Collections.IEnumerator PlayPreDialogueAndShow(int levelIndex)
        {
            var lines = BuildPreChoiceDialogue(levelIndex);

            // Play via the VN dialogue system if available
            if (VN_DialogueController.Instance != null && lines != null && lines.Count > 0)
            {
                VN_DialogueController.Instance.PlaySequence(lines);
                // Wait until the dialogue finishes
                yield return new WaitUntil(() => !VN_DialogueController.Instance.IsActive);
                yield return new WaitForSecondsRealtime(0.1f);
            }

            // Now initialise the UI overlay (previous Show logic).
            // Set up portrait and background tex, hide HUD, freeze cam, and enable choice UI.
            _backgroundTex = Resources.Load<Texture2D>("UI/void_fog_bg");

            var stage = AidenStageResolver.ResolveForCurrentLevel();
            string spriteName = stage switch
            {
                AidenStage.Conviction => "VN/Sprites/aiden/Aiden_preocupada",
                AidenStage.Guilt => "VN/Sprites/aiden/Aiden_pensativa_preocupada",
                AidenStage.Realization => "VN/Sprites/aiden/Aiden_triste",
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

            var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var p in players)
            {
                if (p != null) p.SetInputLocked(true);
            }

            FreezeCamera();
            _choiceActive = true; // finally enable the choice UI
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
            DrawColored(new Rect(0f, 0f, RefW, RefH), new Color(0.075f, 0.078f, 0.063f, 0.85f));
        }

        void DrawPortrait()
        {
            Rect pr = new Rect(100f, 160f, 500f, 720f);
            DrawBox(pr, new Color(0.075f, 0.078f, 0.063f, 0.95f), new Color(0.58f, 0.56f, 0.53f, 0.4f), 1f);
            if (_portraitTex != null)
                GUI.DrawTextureWithTexCoords(new Rect(110f, 170f, 480f, 700f), _portraitTex, _portraitUV, true);
        }

        void DrawChoiceBox(float scale)
        {
            Rect boxRect = new Rect(640f, 260f, 1160f, 520f);
            DrawBox(boxRect, new Color(0.075f, 0.078f, 0.063f, 0.95f), new Color(0.58f, 0.56f, 0.53f, 0.4f), 1f);

            // Left accent bar
            DrawColored(new Rect(boxRect.x, boxRect.y, 3f, boxRect.height), new Color(0.39f, 0.83f, 0.98f, 1f));

            int tagSize = Mathf.RoundToInt(13 * scale);
            GUI.Label(new Rect(boxRect.x + 48f, boxRect.y + 36f, boxRect.width - 96f, 24f), "DIRECTORIO_DECISIÓN // RECUERDO", ScaledStyle(tagSize, new Color(0.58f, 0.56f, 0.53f), TextAnchor.MiddleLeft, FontStyle.Bold));

            int titleSize = Mathf.RoundToInt(34 * scale);
            GUI.Label(new Rect(boxRect.x + 48f, boxRect.y + 64f, boxRect.width - 96f, 48f), "Elige tu camino", ScaledStyle(titleSize, new Color(1f, 0.99f, 1f), TextAnchor.MiddleLeft, FontStyle.Bold));

            // Divider
            DrawColored(new Rect(boxRect.x + 48f, boxRect.y + 118f, boxRect.width - 96f, 1f), new Color(0.58f, 0.56f, 0.53f, 0.25f));

            int promptSize = Mathf.RoundToInt(24 * scale);
            GUI.Label(new Rect(boxRect.x + 48f, boxRect.y + 136f, boxRect.width - 96f, 100f), _promptText, ScaledStyle(promptSize, new Color(0.89f, 0.88f, 0.86f), TextAnchor.MiddleLeft));

            float bw = (boxRect.width - 120f) / 2f;
            float bh = 90f;
            float by = boxRect.y + 280f;

            DrawOptionCard(new Rect(boxRect.x + 48f, by, bw, bh), 0, _cyanLabel, new Color(0.39f, 0.83f, 0.98f), "OPCIÓN A // SINTONÍA", scale);
            DrawOptionCard(new Rect(boxRect.x + 72f + bw, by, bw, bh), 1, _amberLabel, new Color(0.91f, 0.88f, 0.83f), "OPCIÓN B // CONVICCIÓN", scale);

            int hintSize = Mathf.RoundToInt(13 * scale);
            GUI.Label(new Rect(boxRect.x + 48f, boxRect.yMax - 48f, boxRect.width - 96f, 28f), "[← → / A D] Seleccionar    •    [Enter / E / Clic] Confirmar", ScaledStyle(hintSize, new Color(0.58f, 0.56f, 0.53f), TextAnchor.MiddleCenter, FontStyle.Bold));
        }

        void DrawOptionCard(Rect rect, int index, string label, Color accent, string metaTag, float scale)
        {
            bool selected = _selectedIndex == index;
            Color fill = selected ? new Color(accent.r * 0.15f, accent.g * 0.15f, accent.b * 0.15f, 0.95f) : new Color(0.1f, 0.11f, 0.09f, 0.90f);
            Color border = selected ? accent : new Color(0.58f, 0.56f, 0.53f, 0.35f);
            float bw = selected ? 2f : 1f;
            DrawBox(rect, fill, border, bw);

            var mouseRect = new Rect(rect.x * CurrentScale + (Screen.width - RefW * CurrentScale) * 0.5f, rect.y * CurrentScale + (Screen.height - RefH * CurrentScale) * 0.5f, rect.width * CurrentScale, rect.height * CurrentScale);
            if (mouseRect.Contains(Event.current.mousePosition))
            {
                if (_selectedIndex != index) _selectedIndex = index;
                if (Event.current.type == EventType.MouseDown)
                {
                    OnChoiceMade(index == 0);
                }
            }

            int metaSize = Mathf.RoundToInt(11 * scale);
            GUI.Label(new Rect(rect.x + 16f, rect.y + 12f, rect.width - 32f, 18f), metaTag, ScaledStyle(metaSize, selected ? accent : new Color(0.58f, 0.56f, 0.53f), TextAnchor.MiddleLeft, FontStyle.Bold));

            int labelSize = Mathf.RoundToInt(22 * scale);
            Color textColor = selected ? new Color(1f, 1f, 1f) : new Color(0.85f, 0.85f, 0.85f);
            GUI.Label(new Rect(rect.x + 16f, rect.y + 32f, rect.width - 32f, 46f), label, ScaledStyle(labelSize, textColor, TextAnchor.MiddleLeft, FontStyle.Bold));

            if (selected)
            {
                DrawColored(new Rect(rect.x, rect.y, 3f, rect.height), accent);
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

            var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var p in players)
            {
                if (p != null) p.SetInputLocked(false);
            }

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

        static List<VN_DialogueController.DialogueLine> BuildPreChoiceDialogue(int level)
        {
            var lines = new List<VN_DialogueController.DialogueLine>();

            void AddLine(string charName, string text, string sprite, VN_DialogueController.DialogueLine.SpritePosition pos)
            {
                lines.Add(new VN_DialogueController.DialogueLine
                {
                    characterName = charName,
                    text = text,
                    spritePath = sprite,
                    position = pos,
                    voiceClipPath = ""
                });
            }

            switch (level)
            {
                case 1:
                    AddLine("Aiden", "Estas puertas... estaban cerradas con llave. Como si alguien no quisiera que nadie volviera a entrar.", "VN/Sprites/aiden/Aiden_pensativa", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "O tal vez las cerraste tú misma, Aiden. Porque dolía demasiado mirar hacia adentro.", "VN/Sprites/lyra/Lyra_Neutral", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    AddLine("Aiden", "Si abro este umbral, ya no podré fingir que todo sigue igual.", "VN/Sprites/aiden/Aiden_preocupada", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "No necesitas fingir. Solo necesitas decidir si vas a mirar o si vas a pasar de largo.", "VN/Sprites/lyra/Lyra_Sonrisa", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    break;

                case 2:
                    AddLine("Aiden", "Siento que acabo de caminar por este mismo pasillo. Las mismas sombras, el mismo frío.", "VN/Sprites/aiden/Aiden_pensativa_preocupada", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "Cuando intentamos evitar lo que nos asusta, solemos construir círculos.", "VN/Sprites/lyra/Lyra_Perturbada", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    AddLine("Aiden", "No es un círculo, es solo una coincidencia... ¿verdad?", "VN/Sprites/aiden/Aiden_ligeramente_enojada", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "Nombrar el patrón es la única forma de no volver a tropezar con él.", "VN/Sprites/lyra/Lyra_Neutral", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    break;

                case 3:
                    AddLine("Aiden", "El camino se dividió en dos. El eco tomó una ruta y yo tomé otra.", "VN/Sprites/aiden/Aiden_pensativa", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "¿Y por qué te asusta tanto que exista más de una manera de llegar?", "VN/Sprites/lyra/Lyra_Feliz", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    AddLine("Aiden", "Porque siempre creí que si me equivocaba de camino, todo se arruinaría.", "VN/Sprites/aiden/Aiden_triste", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "Aceptar otra versión de ti no es perderte. Es empezar a comprenderte.", "VN/Sprites/lyra/Lyra_Sonrisa", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    break;

                case 4:
                    AddLine("Aiden", "El aula vacía... recuerdo el peso de todas las palabras que me guardé aquel día.", "VN/Sprites/aiden/Aiden_triste", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "El silencio también fue una decisión, Aiden. No solo callaste para protegerte.", "VN/Sprites/lyra/Lyra_Neutral", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    AddLine("Aiden", "Callé porque tenía miedo de lo que pasaría si decía la verdad.", "VN/Sprites/aiden/Aiden_preocupada", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "Reconocer ese silencio no borra lo que pasó, pero te libera de seguir cargándolo.", "VN/Sprites/lyra/Lyra_Sonrisa", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    break;

                case 5:
                    AddLine("Aiden", "Es tu taquilla, Lyra. Sigue aquí, intacta, con ese candado oxidado...", "VN/Sprites/aiden/Aiden_preocupada_enplanmal", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "Guarda recuerdos que solían hacernos reír, y otros que aún queman.", "VN/Sprites/lyra/Lyra_Perturbada", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    AddLine("Aiden", "Si la toco... siento que voy a romper lo poco que queda.", "VN/Sprites/aiden/Aiden_triste", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "Tocarla no la destruye. Solo te recuerda que fuimos reales.", "VN/Sprites/lyra/Lyra_Sonrisa", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    break;

                case 6:
                    AddLine("Aiden", "A veces ya no sé si recuerdo las cosas como fueron, o como necesité inventarlas para soportarlo.", "VN/Sprites/aiden/Aiden_pensativa", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "La memoria no es una fotografía perfecta. Es un refugio que construiste cuando no tenías dónde esconderte.", "VN/Sprites/lyra/Lyra_Neutral", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    AddLine("Aiden", "¿Y si mi verdad solo fue una forma de defenderme?", "VN/Sprites/aiden/Aiden_preocupada", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "Entonces dale permiso a tu verdad de ser humana.", "VN/Sprites/lyra/Lyra_Sonrisa", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    break;

                case 7:
                    AddLine("Aiden", "Siempre quise tener esa última charla contigo. Cambiar lo que dije... arreglarlo.", "VN/Sprites/aiden/Aiden_ligeramente_enojada", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "No puedes reescribir una conversación que ya terminó, Aiden.", "VN/Sprites/lyra/Lyra_Neutral", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    AddLine("Aiden", "¿Entonces qué hago con todo esto que se quedó en el aire?", "VN/Sprites/aiden/Aiden_triste", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "Dejarlo abierto. Aceptar que no todas las historias terminan con una respuesta perfecta.", "VN/Sprites/lyra/Lyra_Sonrisa", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    break;

                case 8:
                    AddLine("Aiden", "Veo a la Aiden que intentó ser fuerte y a la que se rompió por completo. No encajan.", "VN/Sprites/aiden/Aiden_pensativa_preocupada", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "¿Quién dijo que debían encajar como piezas de un reloj?", "VN/Sprites/lyra/Lyra_Feliz", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    AddLine("Aiden", "Siento que si abrazo a una, traiciono a la otra.", "VN/Sprites/aiden/Aiden_preocupada", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "Ambas te trajeron hasta aquí. Las dos merecen ser escuchadas.", "VN/Sprites/lyra/Lyra_Sonrisa", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    break;

                case 9:
                    AddLine("Aiden", "Trato de coordinar cada mecanismo, cada eco, cada segundo... ¡y el lugar no me obedece!", "VN/Sprites/aiden/Aiden_enojada", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "Porque este lugar no es una máquina que puedas someter, Aiden.", "VN/Sprites/lyra/Lyra_Neutral", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    AddLine("Aiden", "Si no controlo lo que pasa, siento que me desmorono.", "VN/Sprites/aiden/Aiden_triste", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "Soltar el control no es caer. Es aprender a sostenerte sin forzar el mundo.", "VN/Sprites/lyra/Lyra_Sonrisa", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    break;

                case 10:
                    AddLine("Aiden", "Está ahí, brillando en medio del pasillo. Parece tan frágil...", "VN/Sprites/aiden/Aiden_pensativa", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "Es uno de nuestros fragmentos. De cuando creíamos que el tiempo era infinito.", "VN/Sprites/lyra/Lyra_Sonrisa", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    AddLine("Aiden", "Tengo miedo de acercarme y que se desvanezca como humo.", "VN/Sprites/aiden/Aiden_preocupada", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "Solo se desvanece si lo miras desde lejos sin atreverte a sentirlo.", "VN/Sprites/lyra/Lyra_Feliz", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    break;

                case 11:
                    AddLine("Aiden", "Las escaleras parecen infinitas hacia la luz. Pero pesa cada peldaño.", "VN/Sprites/aiden/Aiden_pensativa_preocupada", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "Subir no significa dejar atrás lo que fuiste abajo.", "VN/Sprites/lyra/Lyra_Neutral", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    AddLine("Aiden", "¿Puedo llegar arriba sin tener que empujarme hasta el agotamiento?", "VN/Sprites/aiden/Aiden_preocupada", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "Si dejas que tus ecos te acompañen en lugar de luchar contra ellos, no subirás sola.", "VN/Sprites/lyra/Lyra_Sonrisa", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    break;

                case 12:
                    AddLine("Aiden", "Te extraño con toda mi alma, pero también me dolía estar cerca de ti. Ambas cosas son ciertas.", "VN/Sprites/aiden/Aiden_triste", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "Y ninguna de las dos anula a la otra, Aiden.", "VN/Sprites/lyra/Lyra_Sonrisa", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    AddLine("Aiden", "Siempre busqué un culpable... a ti o a mí.", "VN/Sprites/aiden/Aiden_pensativa", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "No hay culpables en el dolor. Solo dos personas que hicieron lo mejor que pudieron.", "VN/Sprites/lyra/Lyra_Neutral", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    break;

                case 13:
                    AddLine("Aiden", "Este es el núcleo. El momento exacto donde todo se partió.", "VN/Sprites/aiden/Aiden_preocupada_enplanmal", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "Sostener este dolor fue tu forma de no olvidarme durante tanto tiempo.", "VN/Sprites/lyra/Lyra_Perturbada", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    AddLine("Aiden", "Tengo miedo de que si dejo de dolerme, te borres para siempre.", "VN/Sprites/aiden/Aiden_triste", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "Yo no existo en tu culpa, Aiden. Existo en lo que aprendiste a amar.", "VN/Sprites/lyra/Lyra_Sonrisa", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    break;

                case 14:
                    AddLine("Aiden", "El eco ya no espera mis órdenes. Camina hacia el final con paso firme.", "VN/Sprites/aiden/Aiden_pensativa", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "Porque ya aprendió lo que necesitaba. Ahora te toca a ti confiar.", "VN/Sprites/lyra/Lyra_Sonrisa", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    AddLine("Aiden", "Ya no siento la necesidad de corregir cada movimiento.", "VN/Sprites/aiden/Aiden_Feliz_Hablando", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "Eso es madurar con el recuerdo. Caminar a su lado sin intentar atarlo.", "VN/Sprites/lyra/Lyra_Feliz", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    break;

                case 15:
                    AddLine("Aiden", "La puerta de salida está frente a nosotras. El colegio se queda atrás.", "VN/Sprites/aiden/Aiden_triste", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "Este lugar siempre existirá en tu memoria, Aiden. Pero tú ya no tienes que vivir atrapada aquí.", "VN/Sprites/lyra/Lyra_Sonrisa", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    AddLine("Aiden", "¿Qué pasa con nosotras cuando cruce esa puerta?", "VN/Sprites/aiden/Aiden_pensativa", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "Tú sigues viviendo. Y yo... seré el eco más hermoso que alguna vez te acompañó.", "VN/Sprites/lyra/Lyra_Feliz", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    break;

                default:
                    AddLine("Aiden", $"¿Realmente debemos tomar esta decisión ahora?", "VN/Sprites/aiden/Aiden_pensativa", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    AddLine("Lyra", "Cada paso que diste en esta sala te trajo hasta este momento.", "VN/Sprites/lyra/Lyra_Neutral", VN_DialogueController.DialogueLine.SpritePosition.Right);
                    AddLine("Aiden", "Entonces no voy a huir.", "VN/Sprites/aiden/Aiden_Feliz", VN_DialogueController.DialogueLine.SpritePosition.Left);
                    break;
            }

            return lines;
        }
    }
}
