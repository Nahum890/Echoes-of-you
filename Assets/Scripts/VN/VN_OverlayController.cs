using System;
using System.Collections;
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

        public bool IsOpen => VN_DialogueController.Instance != null && VN_DialogueController.Instance.IsActive;

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

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void PlaySequence(IEnumerable<VN_Line> lines)
        {
            StartCoroutine(PlaySequenceRoutine(lines));
        }

        IEnumerator PlaySequenceRoutine(IEnumerable<VN_Line> lines)
        {
            float timeout = Time.realtimeSinceStartup + 3f;
            while (VN_DialogueController.Instance == null && Time.realtimeSinceStartup < timeout)
                yield return null;

            var dc = VN_DialogueController.Instance;
            if (dc != null)
            {
                yield return dc.WaitUntilReady(2f);

                var dialogueLines = new List<VN_DialogueController.DialogueLine>();
                foreach (var l in lines)
                {
                    dialogueLines.Add(new VN_DialogueController.DialogueLine
                    {
                        characterName = l.speaker,
                        text = l.text,
                        spritePath = l.spriteResourcePath
                    });
                }
                dc.PlaySequence(dialogueLines);
            }
            else
            {
                Debug.LogError("[VN_OverlayController] VN_DialogueController not available.");
            }
        }

        public void Play(string speaker, string text, string spritePath = "")
        {
            PlaySequence(new[] { new VN_Line { speaker = speaker, text = text, spriteResourcePath = spritePath } });
        }

        public void CloseDialogue()
        {
            VN_DialogueController.Instance?.StopDialogue();
        }
    }
}
