using System.Collections.Generic;
using Echoes.VN;
using UnityEngine;

namespace Echoes.UI
{
    /// <summary>
    /// Tabla de textos diégéticos: pop-ups de inspección (voz interna de Aiden),
    /// prompts de VN choice gates y voces finales de epílogos.
    /// Carga Resources/VN_Text.json. Formo JSON plano (no YAML) porque Unity
    /// no trae parser YAML y el proyecto ya carga assets via JsonUtility.
    /// Estructura serializable plana (JsonUtility no soporta Dictionary ni
    /// serializa enums como string — usamos arrays indexados por orden de
    /// etapa: [0]=Conviction, [1]=Guilt, [2]=Realization, [3]=Acceptance).
    /// </summary>
    public static class VN_TextTable
    {
        [System.Serializable]
        public class StageTexts
        {
            public string conviction = "";
            public string guilt = "";
            public string realization = "";
            public string acceptance = "";
        }

        [System.Serializable]
        public class InteractionEntry
        {
            public string key = "interaction.default";
            public string title = "Objeto";
            public bool is_lyra_artifact = false;
            public StageTexts tone = new();
        }

        [System.Serializable]
        public class ChoiceEntry
        {
            public string node_id = "";
            public string prompt = "";
            public string cyan_label = "";
            public string amber_label = "";
            public bool is_micro = false;
            public int level_index = 0;
        }

        [System.Serializable]
        public class EpilogueEntry
        {
            public string ending_id = "";
            public string voice_final = "";
            public string narration = "";
        }

        [System.Serializable]
        public class Root
        {
            public List<InteractionEntry> interaction = new();
            public List<ChoiceEntry> choice = new();
            public List<EpilogueEntry> epilogue = new();
        }

        static Root _data;
        static bool _loaded;

        static void EnsureLoaded()
        {
            if (_loaded) return;
            _loaded = true;
            var ta = Resources.Load<TextAsset>("VN_Text");
            if (ta == null)
            {
                _data = new Root();
                Debug.LogWarning("[VN_TextTable] Resources/VN_Text.json not found — fallback texts in use.");
                return;
            }
            try
            {
                _data = JsonUtility.FromJson<Root>(ta.text) ?? new Root();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[VN_TextTable] Failed to parse JSON: {e.Message}");
                _data = new Root();
            }
        }

        static string Fallback(AidenStage stage)
        {
            return stage switch
            {
                AidenStage.Conviction => "Yo no necesito mirar aquí.",
                AidenStage.Guilt => "Pude haberlo hecho distinto.",
                AidenStage.Realization => "Esto también lo armé yo.",
                _ => "Puedo soltar esto sin romperlo."
            };
        }

        static string Resolve(StageTexts t, AidenStage s)
        {
            if (t == null) return "";
            return s switch
            {
                AidenStage.Conviction => t.conviction,
                AidenStage.Guilt => t.guilt,
                AidenStage.Realization => t.realization,
                _ => t.acceptance
            };
        }

        public class ResolvedEntry
        {
            public string title;
            public string text;
        }

        public static ResolvedEntry Get(string commentKey, AidenStage stage)
        {
            EnsureLoaded();
            if (_data?.interaction != null)
            {
                foreach (var e in _data.interaction)
                {
                    if (e.key != commentKey) continue;
                    string txt = Resolve(e.tone, stage);
                    if (string.IsNullOrEmpty(txt)) txt = Fallback(stage);
                    return new ResolvedEntry { title = e.title ?? "Objeto", text = txt };
                }
            }
            return new ResolvedEntry { title = "Objeto", text = Fallback(stage) };
        }

        public static ChoiceEntry GetChoice(string nodeId)
        {
            EnsureLoaded();
            if (_data?.choice == null) return null;
            foreach (var c in _data.choice) if (c.node_id == nodeId) return c;
            return null;
        }

        public static EpilogueEntry GetEpilogue(string endingId)
        {
            EnsureLoaded();
            if (_data?.epilogue == null) return null;
            foreach (var e in _data.epilogue) if (e.ending_id == endingId) return e;
            return null;
        }

        public static void Reload()
        {
            _loaded = false;
            EnsureLoaded();
        }
    }
}
