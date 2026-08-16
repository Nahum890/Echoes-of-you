using UnityEngine;

namespace Echoes.Narrative.Data
{
    [System.Serializable]
    public class DialogueNode
    {
        [Header("Identity")]
        [SerializeField] string _nodeId = "";
        [SerializeField] string _speakerId = "";
        [SerializeField] string _textKey = "";
        [SerializeField] string _textDirect = "";

        [Header("Visual")]
        [SerializeField] string _spritePath = "";
        [SerializeField] SpritePosition _spritePosition = SpritePosition.None;
        [SerializeField] string _voiceClipPath = "";

        [Header("Flow")]
        [SerializeField] bool _autoAdvance = false;
        [SerializeField] float _advanceDelay = 0f;

        [Header("Conditions")]
        [SerializeField] string[] _conditions = System.Array.Empty<string>();

        [Header("Choices")]
        [SerializeField] DialogueChoice[] _choices = System.Array.Empty<DialogueChoice>();

        public string NodeId => _nodeId;
        public string SpeakerId => _speakerId;
        public string TextKey => _textKey;
        public string TextDirect => _textDirect;
        public string SpritePath => _spritePath;
        public SpritePosition SpritePos => _spritePosition;
        public string VoiceClipPath => _voiceClipPath;
        public bool AutoAdvance => _autoAdvance;
        public float AdvanceDelay => _advanceDelay;
        public string[] Conditions => _conditions;
        public DialogueChoice[] Choices => _choices;

        public enum SpritePosition { None, Left, Center, Right }
    }
}
