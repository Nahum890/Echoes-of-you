using UnityEngine;
using Echoes.VN;

public class LevelIntroTrigger : MonoBehaviour
{
    VN_DialogueController _vn;
    bool _playerLocked;

    void Start()
    {
        Invoke(nameof(PlayIntro), 1.0f);
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

    void PlayIntro()
    {
        _vn = FindAnyObjectByType<VN_DialogueController>();
        if (_vn == null)
        {
            Debug.LogError("[LevelIntroTrigger] VN_DialogueController not found!");
            return;
        }

        var pc = FindAnyObjectByType<PlayerController>();
        if (pc != null)
        {
            pc.SetInputLocked(true);
            _playerLocked = true;
        }

        var lines = new VN_DialogueController.DialogueLine[]
        {
            new VN_DialogueController.DialogueLine
            {
                characterName = "Aiden",
                text = "Todo esta demasiado silencioso... Siento que algo no encaja aqui.",
                spritePath = "VN/Sprites/aiden/Aiden_Perturbada",
                position = VN_DialogueController.DialogueLine.SpritePosition.Left,
                voiceClipPath = ""
            },
            new VN_DialogueController.DialogueLine
            {
                characterName = "Aiden",
                text = "Tengo que moverme y buscar respuestas.",
                spritePath = "VN/Sprites/aiden/Aiden_Pensativa",
                position = VN_DialogueController.DialogueLine.SpritePosition.Left,
                voiceClipPath = ""
            }
        };

        _vn.Enqueue(lines);
        _vn.Play();
        Debug.Log("[LevelIntroTrigger] VN dialogue started.");
    }
}