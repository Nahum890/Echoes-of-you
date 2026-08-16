using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Echoes.VN;

public class LevelIntroTrigger : MonoBehaviour
{
    bool _playerLocked;

    const string SpriteScared   = "VN/Sprites/aiden/Aiden_Perturbada";
    const string SpriteThinking = "VN/Sprites/aiden/Aiden_Pensativa";
    const string SpriteCalm     = "VN/Sprites/aiden/Aiden_Feliz";

    void Start()
    {
        StartCoroutine(PlayIntro());
    }

    void Update()
    {
        if (_playerLocked && VN_OverlayController.Instance != null && !VN_OverlayController.Instance.IsOpen)
        {
            var pc = FindAnyObjectByType<PlayerController>();
            if (pc != null) pc.SetInputLocked(false);
            _playerLocked = false;
        }
    }

    IEnumerator PlayIntro()
    {
        var overlay = VN_OverlayController.Instance;
        if (overlay == null)
        {
            var go = new GameObject("VN_Overlay");
            go.AddComponent<VN_OverlayController>();
            DontDestroyOnLoad(go);
            yield return null;
            overlay = VN_OverlayController.Instance;
        }

        if (overlay == null)
        {
            Debug.LogError("[LevelIntroTrigger] VN_OverlayController could not be created!");
            yield break;
        }

        var pc = FindAnyObjectByType<PlayerController>();
        if (pc != null)
        {
            pc.SetInputLocked(true);
            _playerLocked = true;
        }

        var lines = BuildLinesForCurrentLevel();
        if (lines == null || lines.Length == 0)
            yield break;

        overlay.PlaySequence(lines);
        Debug.Log($"[LevelIntroTrigger] VN intro started for {SceneManager.GetActiveScene().name} ({lines.Length} lines).");
    }

    static VN_Line[] BuildLinesForCurrentLevel()
    {
        int level = GameProgress.GetSceneIndex(SceneManager.GetActiveScene().name) + 1;

        if (level == 1)
            return BuildLevel01Intro();

        string text = level switch
        {
            2  => "El pasillo vuelve a empezar. No voy a llamarlo un patron.",
            3  => "Hay dos caminos y ambos se parecen demasiado a mi.",
            4  => "El silencio llena el aula antes de que yo diga una palabra.",
            5  => "No tengo que tocar cada recuerdo para saber que pesa.",
            6  => "Esta memoria cambia cuando intento sostenerla demasiado fuerte.",
            7  => "El eco llega primero. Tal vez no esta intentando reemplazarme.",
            8  => "No necesito elegir una sola version de mi para avanzar.",
            9  => "Quiero ordenar este lugar, pero el lugar no me obedece.",
            10 => "Lo que dejo sin mirar sigue esperando en la luz.",
            11 => "Subir no es olvidar lo que queda abajo.",
            12 => "Dos verdades pueden doler sin que una borre a la otra.",
            13 => "Puedo mirar lo que duele sin convertirlo en mi unica historia.",
            14 => "El eco sabe el camino. Puedo acompanarlo sin controlarlo.",
            15 => "La puerta sigue aqui. Esta vez puedo decidir como cruzarla.",
            _  => "Algo en este lugar me esta pidiendo que escuche."
        };

        string spritePath = level switch
        {
            >= 13 => SpriteCalm,
            >= 5  => SpriteThinking,
            _     => SpriteScared
        };

        return new[]
        {
            new VN_Line { speaker = "Aiden", text = text, spriteResourcePath = spritePath }
        };
    }

    static VN_Line[] BuildLevel01Intro()
    {
        return new[]
        {
            new VN_Line { speaker = "Aiden", text = "...", spriteResourcePath = SpriteScared },
            new VN_Line { speaker = "Aiden", text = "Donde... estoy? Reconozco estos pasillos pero no deberia estar aqui.", spriteResourcePath = SpriteScared },
            new VN_Line { speaker = "Aiden", text = "Hay algo en el aire. Como si el lugar hubiera esperado mucho tiempo para mostrarse.", spriteResourcePath = SpriteScared },
            new VN_Line { speaker = "Aiden", text = "Veo objetos a mi alrededor. Parecen querer decirme algo... pero no ahora.", spriteResourcePath = SpriteThinking },
            new VN_Line { speaker = "Aiden", text = "Primero necesito entender que hago aqui. Quizas haya una salida.", spriteResourcePath = SpriteThinking },
            new VN_Line { speaker = "Aiden", text = "Respira. Ya veremos que tiene esto.", spriteResourcePath = SpriteCalm }
        };
    }
}
