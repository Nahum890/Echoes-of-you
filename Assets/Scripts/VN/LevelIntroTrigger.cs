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

        var dc = VN_DialogueController.EnsureExists();

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

        if (SceneManager.GetActiveScene().name == "Level_02")
            return new List<VN_DialogueController.DialogueLine>
            {
                new() { characterName = "Aiden", text = "Quedó prendida la luz de aire. Desde el pasillo siempre parecía que había alguien acá.", spritePath = SpriteThinking, position = VN_DialogueController.DialogueLine.SpritePosition.Left },
                new() { characterName = "Lyra", text = "Si vas a estornudar, avisá. La última vez nos escuchó hasta la bibliotecaria.", spritePath = "VN/Sprites/lyra/Lyra_Sonrisa", position = VN_DialogueController.DialogueLine.SpritePosition.Right },
                new() { characterName = "Aiden", text = "Me tapo la nariz por costumbre. El libro de emisiones sigue abierto en nuestra mesa.", spritePath = SpriteCalm, position = VN_DialogueController.DialogueLine.SpritePosition.Left }
            };

        if (SceneManager.GetActiveScene().name == "Level_03")
            return new List<VN_DialogueController.DialogueLine>
            {
                new() { characterName = "Aiden", text = "El laboratorio huele a vinagre. En la lista de socios seguimos siendo dos. La tercera firma es tuya con bigote.", spritePath = "VN/Sprites/aiden/Aiden_Pensativa", position = VN_DialogueController.DialogueLine.SpritePosition.Left },
                new() { characterName = "Lyra", text = "Era el tesorero. Un club serio necesita tesorero, aunque deba tres mandarinas.", spritePath = "VN/Sprites/lyra/Lyra_Sonrisa", position = VN_DialogueController.DialogueLine.SpritePosition.Right },
                new() { characterName = "Aiden", text = "Debajo del manual quedó el recibo. «Dos copias: una para cada una». Esta vez voy a terminar la mía.", spritePath = SpriteCalm, position = VN_DialogueController.DialogueLine.SpritePosition.Left }
            };

        if (SceneManager.GetActiveScene().name == "Level_05")
            return new List<VN_DialogueController.DialogueLine>
            {
                new() { characterName = "Aiden", text = "Huele a madera recién barrida. Siempre llegábamos antes de que abrieran las puertas para oír cómo sonaba el salón vacío.", spritePath = "VN/Sprites/aiden/Aiden_Pensativa", position = VN_DialogueController.DialogueLine.SpritePosition.Left },
                new() { characterName = "Lyra", text = "Probando, uno, dos… ¿Me oís desde la última fila? Si me oís, hacé como que aplaudís.", spritePath = "VN/Sprites/lyra/Lyra_Sonrisa", position = VN_DialogueController.DialogueLine.SpritePosition.Right },
                new() { characterName = "Aiden", text = "Levanté las manos antes de darme cuenta. Todavía estoy esperando que se ría por el altavoz.", spritePath = SpriteCalm, position = VN_DialogueController.DialogueLine.SpritePosition.Left }
            };

        if (level == 6)
            return new List<VN_DialogueController.DialogueLine>
            {
                new() { characterName = "Aiden", text = "Han dejado encendida la luz de préstamos. Desde la puerta casi parece que Lyra sigue de turno.", spritePath = SpriteThinking, position = VN_DialogueController.DialogueLine.SpritePosition.Left },
                new() { characterName = "Lyra", text = "Guardame el atlas abajo del mostrador. Si lo devuelvo mañana, me gano un día más con nuestra isla.", spritePath = "VN/Sprites/lyra/Lyra_Sonrisa", position = VN_DialogueController.DialogueLine.SpritePosition.Right },
                new() { characterName = "Aiden", text = "Eso fue un jueves. Me acuerdo porque los viernes me tocaba borrar las fechas del sello. A ver si todavía está en el cajón.", spritePath = SpriteCalm, position = VN_DialogueController.DialogueLine.SpritePosition.Left }
            };

        string text = level switch
        {
            2  => "El pasillo vuelve a empezar. No voy a llamarlo un patrón.",
            3  => "Hay dos caminos y ambos se parecen demasiado a mí.",
            4  => "El timbre ya sonó. El cronómetro de Lyra sigue donde lo dejamos.",
            5  => "No tengo que tocar cada recuerdo para saber que pesa.",
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
