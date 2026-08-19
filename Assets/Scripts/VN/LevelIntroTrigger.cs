using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using Echoes.VN;

public class LevelIntroTrigger : MonoBehaviour
{
    const string SpriteScared   = "VN/Sprites/aiden/Aiden_preocupada_enplanmal";
    const string SpriteThinking = "VN/Sprites/aiden/Aiden_pensativa";
    const string SpriteCalm     = "VN/Sprites/aiden/Aiden_Feliz";

    void Start()
    {
        StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        yield return new WaitForSeconds(0.2f);

        GameplayUIBootstrap.EnsureGameplayUI();

        var dc = VN_DialogueController.Instance;
        if (dc == null)
        {
            var panel = global::UIBootstrap.PanelSettings;
            var go = new GameObject("VNDialogueController");
            var doc = go.AddComponent<UIDocument>();
            doc.panelSettings = panel;
            var vta = Resources.Load<VisualTreeAsset>("UI/VN/VN_DialogueUI");
#if UNITY_EDITOR
            if (vta == null) vta = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/VN/VN_DialogueUI.uxml");
#endif
            if (vta != null) doc.visualTreeAsset = vta;
            dc = go.AddComponent<VN_DialogueController>();
            if (Application.isPlaying) DontDestroyOnLoad(go);
        }

        if (dc != null)
        {
            yield return dc.WaitUntilReady(2f);

            var lines = BuildLinesForCurrentLevel();
            if (lines != null && lines.Count > 0)
            {
                dc.PlaySequence(lines);
                Debug.Log($"[LevelIntroTrigger] VN intro started for {SceneManager.GetActiveScene().name} ({lines.Count} lines).");

                int level = GameProgress.GetSceneIndex(SceneManager.GetActiveScene().name) + 1;
                if (level == 1)
                {
                    // Wait until Aiden finishes speaking
                    yield return new WaitWhile(() => dc.IsActive);
                    yield return new WaitForSeconds(0.15f);

                    // Present Level 1 Tutorial Infographic
                    yield return Echoes.UI.TutorialOverlayController.ShowAndWait();
                }
            }
        }
        else
        {
            Debug.LogWarning("[LevelIntroTrigger] VN_DialogueController could not be initialized!");
        }
    }

    static List<VN_DialogueController.DialogueLine> BuildLinesForCurrentLevel()
    {
        int level = GameProgress.GetSceneIndex(SceneManager.GetActiveScene().name) + 1;

        if (level == 1)
            return BuildLevel01Intro();

        string text = level switch
        {
            2  => "El pasillo vuelve a empezar. No voy a llamarlo un patrón.",
            3  => "Hay dos caminos y ambos se parecen demasiado a mí.",
            4  => "El silencio llena el aula antes de que yo diga una palabra.",
            5  => "No tengo que tocar cada recuerdo para saber que pesa.",
            6  => "Esta memoria cambia cuando intento sostenerla demasiado fuerte.",
            7  => "El eco llega primero. Tal vez no está intentando reemplazarme.",
            8  => "No necesito elegir una sola versión de mí para avanzar.",
            9  => "Quiero ordenar este lugar, pero el lugar no me obedece.",
            10 => "Lo que dejo sin mirar sigue esperando en la luz.",
            11 => "Subir no es olvidar lo que queda abajo.",
            12 => "Dos verdades pueden doler sin que una borre a la otra.",
            13 => "Puedo mirar lo que duele sin convertirlo en mi única historia.",
            14 => "El eco sabe el camino. Puedo acompañarlo sin controlarlo.",
            15 => "La puerta sigue aquí. Esta vez puedo decidir cómo cruzarla.",
            _  => "Algo en este lugar me está pidiendo que escuche."
        };

        string spritePath = level switch
        {
            >= 13 => SpriteCalm,
            >= 5  => SpriteThinking,
            _     => SpriteScared
        };

        return new List<VN_DialogueController.DialogueLine>
        {
            new VN_DialogueController.DialogueLine
            {
                characterName = "Aiden",
                text = text,
                spritePath = spritePath,
                position = VN_DialogueController.DialogueLine.SpritePosition.Left
            }
        };
    }

    static List<VN_DialogueController.DialogueLine> BuildLevel01Intro()
    {
        return new List<VN_DialogueController.DialogueLine>
        {
            new VN_DialogueController.DialogueLine { characterName = "Aiden", text = "¿Dónde... estoy? Reconozco estos pasillos pero no debería estar aquí.", spritePath = SpriteScared, position = VN_DialogueController.DialogueLine.SpritePosition.Left },
            new VN_DialogueController.DialogueLine { characterName = "Aiden", text = "Hay algo en el aire. Como si el lugar hubiera esperado mucho tiempo para mostrarse.", spritePath = SpriteScared, position = VN_DialogueController.DialogueLine.SpritePosition.Left },
            new VN_DialogueController.DialogueLine { characterName = "Aiden", text = "Veo objetos a mi alrededor. Parecen querer decirme algo... pero no ahora.", spritePath = SpriteThinking, position = VN_DialogueController.DialogueLine.SpritePosition.Left },
            new VN_DialogueController.DialogueLine { characterName = "Aiden", text = "Primero necesito entender qué hago aquí. Quizás haya una salida.", spritePath = SpriteThinking, position = VN_DialogueController.DialogueLine.SpritePosition.Left },
            new VN_DialogueController.DialogueLine { characterName = "Aiden", text = "Respira. Ya veremos qué tiene esto.", spritePath = SpriteCalm, position = VN_DialogueController.DialogueLine.SpritePosition.Left }
        };
    }
}
