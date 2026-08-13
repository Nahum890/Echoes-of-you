using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Echoes.VN;

public class LevelIntroTrigger : MonoBehaviour
{
    VN_DialogueController _vn;
    bool _playerLocked;

    void Start()
    {
        StartCoroutine(PlayIntroWhenUiIsReady());
    }

    void Update()
    {
        if (_playerLocked && _vn != null && !_vn.IsActive)
        {
            var pc = FindAnyObjectByType<PlayerController>();
            if (pc != null) pc.SetInputLocked(false);
            _playerLocked = false;
        }
    }

    IEnumerator PlayIntroWhenUiIsReady()
    {
        _vn = VN_DialogueController.Instance ?? FindAnyObjectByType<VN_DialogueController>();

        if (_vn == null)
        {
            var prefab = Resources.Load<GameObject>("EchoesVNBootstrap");
            if (prefab != null)
            {
                var go = Instantiate(prefab);
                go.name = "EchoesVNBootstrap";
                DontDestroyOnLoad(go);
                _vn = VN_DialogueController.Instance ?? FindAnyObjectByType<VN_DialogueController>();
            }
        }

        if (_vn == null)
        {
            Debug.LogError("[LevelIntroTrigger] VN_DialogueController not found after spawn attempt!");
            yield break;
        }

        float deadline = Time.realtimeSinceStartup + 5f;
        while (_vn.IsReady == false && Time.realtimeSinceStartup < deadline)
            yield return null;

        if (!_vn.IsReady)
        {
            Debug.LogError("[LevelIntroTrigger] VN UI did not become ready; player remains unlocked.");
            yield break;
        }

        var pc = FindAnyObjectByType<PlayerController>();
        if (pc != null)
        {
            pc.SetInputLocked(true);
            _playerLocked = true;
        }

        _vn.PlaySequence(BuildLinesForCurrentLevel());
        Debug.Log($"[LevelIntroTrigger] VN dialogue started for {SceneManager.GetActiveScene().name}.");
    }

    static VN_DialogueController.DialogueLine[] BuildLinesForCurrentLevel()
    {
        int level = GameProgress.GetSceneIndex(SceneManager.GetActiveScene().name) + 1;
        string text = level switch
        {
            1 => "Todo esta demasiado silencioso... Siento que algo no encaja aqui.",
            2 => "El pasillo vuelve a empezar. No voy a llamarlo un patron.",
            3 => "Hay dos caminos y ambos se parecen demasiado a mi.",
            4 => "El silencio llena el aula antes de que yo diga una palabra.",
            5 => "No tengo que tocar cada recuerdo para saber que pesa.",
            6 => "Esta memoria cambia cuando intento sostenerla demasiado fuerte.",
            7 => "El eco llega primero. Tal vez no esta intentando reemplazarme.",
            8 => "No necesito elegir una sola version de mi para avanzar.",
            9 => "Quiero ordenar este lugar, pero el lugar no me obedece.",
            10 => "Lo que dejo sin mirar sigue esperando en la luz.",
            11 => "Subir no es olvidar lo que queda abajo.",
            12 => "Dos verdades pueden doler sin que una borre a la otra.",
            13 => "Puedo mirar lo que duele sin convertirlo en mi unica historia.",
            14 => "El eco sabe el camino. Puedo acompanarlo sin controlarlo.",
            15 => "La puerta sigue aqui. Esta vez puedo decidir como cruzarla.",
            _ => "Algo en este lugar me esta pidiendo que escuche."
        };

        string spritePath = level >= 13
            ? "VN/Sprites/aiden/Aiden_Feliz"
            : level >= 5
                ? "VN/Sprites/aiden/Aiden_Pensativa"
                : "VN/Sprites/aiden/Aiden_Perturbada";

        return new[]
        {
            new VN_DialogueController.DialogueLine
            {
                characterName = "Aiden",
                text = text,
                spritePath = spritePath,
                position = VN_DialogueController.DialogueLine.SpritePosition.Left,
                voiceClipPath = ""
            }
        };
    }
}
