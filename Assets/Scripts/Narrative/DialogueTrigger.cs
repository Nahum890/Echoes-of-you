using System.Collections;
using UnityEngine;
using Echoes.Interaction;
using Echoes.UI;
using Echoes.VN;
using Echoes.Narrative.Data;

namespace Echoes.Narrative
{
    [RequireComponent(typeof(InteractableObject))]
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] InteractableData _data;
        [SerializeField] DialogueSequence _dialogueSequence;

        InteractableObject _interactable;
        bool _consumed;

        public InteractableData Data => _data;
        public bool IsConsumed => _consumed;

        void Awake()
        {
            _interactable = GetComponent<InteractableObject>();
        }

        public bool CanInteract()
        {
            if (_consumed) return false;
            if (_data == null) return true;
            if (_data.OneTimeOnly && MemorySystem.Instance != null && MemorySystem.Instance.HasBeenInspected(_data.InteractableId))
                return false;
            return true;
        }

        public void OnInteract()
        {
            if (!CanInteract()) return;

            if (_data != null)
            {
                switch (_data.InteractionType)
                {
                    case InteractionType.Inspect:
                        DoInspect();
                        break;
                    case InteractionType.Memory:
                        DoMemory();
                        break;
                    case InteractionType.Dialogue:
                        DoDialogue();
                        break;
                    case InteractionType.Choice:
                        DoChoice();
                        break;
                }
            }
            else
            {
                DoLegacyInspect();
            }
        }

        void DoInspect()
        {
            var stage = AidenStageResolver.ResolveForCurrentLevel();
            var entry = VN_TextTable.Get(_data.CommentKey, stage);
            var hud = FindAnyObjectByType<GameHUD>();
            if (hud != null)
                hud.ShowInspection(_data.DisplayName, entry.text);

            if (_data.OneTimeOnly)
                MarkConsumed();
        }

        void DoMemory()
        {
            var stage = AidenStageResolver.ResolveForCurrentLevel();
            var entry = VN_TextTable.Get(_data.CommentKey, stage);
            var hud = FindAnyObjectByType<GameHUD>();
            if (hud != null)
                hud.ShowInspection(_data.DisplayName, entry.text);

            if (MemorySystem.Instance != null)
                MemorySystem.Instance.RegisterMemory(_data.InteractableId, _data.MemoryEffect);

            ApplyVisualStateChange();
            MarkConsumed();
        }

        void DoDialogue()
        {
            var ctrl = NarrativeStateController.Instance;
            ctrl?.EnterNarrativeMode(InteractionType.Dialogue);

            var vnCtrl = FindAnyObjectByType<VN_DialogueController>();
            if (vnCtrl != null && _dialogueSequence != null)
            {
                var lines = BuildLines(_dialogueSequence);
                vnCtrl.PlaySequence(lines);
                StartCoroutine(WaitForDialogueEnd(vnCtrl, _dialogueSequence));
            }
            else
            {
                DoInspect();
                ctrl?.ExitNarrativeMode();
            }
        }

        void DoChoice()
        {
            var ctrl = NarrativeStateController.Instance;
            ctrl?.EnterNarrativeMode(InteractionType.Choice);

            var choice = _data.ChoiceEffect;
            if (choice != null)
            {
                ApplyChoiceEffects(choice);
            }

            var stage = AidenStageResolver.ResolveForCurrentLevel();
            var entry = VN_TextTable.Get(_data.CommentKey, stage);
            var hud = FindAnyObjectByType<GameHUD>();
            if (hud != null)
                hud.ShowInspection(_data.DisplayName, entry.text);

            ApplyVisualStateChange();
            MarkConsumed();
            ctrl?.ExitNarrativeMode();
        }

        void DoLegacyInspect()
        {
            var stage = AidenStageResolver.ResolveForCurrentLevel();
            var commentKey = _interactable.CommentKey;
            var entry = VN_TextTable.Get(commentKey, stage);
            var hud = FindAnyObjectByType<GameHUD>();
            if (hud != null)
                hud.ShowInspection(_interactable.DisplayName, entry.text);

            if (_interactable.IsLyraArtifact && VN_EndingFlags.Instance != null)
                VN_EndingFlags.Instance.BumpLyraArtifactSeen();
        }

        void ApplyChoiceEffects(DialogueChoice choice)
        {
            var flags = VN_EndingFlags.Instance;
            if (flags != null)
            {
                if (choice.FlagsAdded != null)
                {
                    for (int i = 0; i < choice.FlagsAdded.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(choice.FlagsAdded[i]))
                            flags.SetFlag(choice.FlagsAdded[i], true);
                    }
                }
                if (choice.FlagsRemoved != null)
                {
                    for (int i = 0; i < choice.FlagsRemoved.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(choice.FlagsRemoved[i]))
                            flags.SetFlag(choice.FlagsRemoved[i], false);
                    }
                }
            }

            var ctrl = NarrativeStateController.Instance;
            if (ctrl != null && choice.VariableChanges != null)
                ctrl.ApplyVariableChanges(choice.VariableChanges);

            if (choice.Effects != null)
                ApplyActions(choice.Effects);
        }

        void ApplyActions(NarrativeAction[] actions)
        {
            if (actions == null) return;
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] == null) continue;
                var a = actions[i];
                switch (a.Type)
                {
                    case NarrativeActionType.SetFlag:
                        VN_EndingFlags.Instance?.SetFlag(a.Target, true);
                        break;
                    case NarrativeActionType.ClearFlag:
                        VN_EndingFlags.Instance?.SetFlag(a.Target, false);
                        break;
                    case NarrativeActionType.SetVariable:
                        if (float.TryParse(a.Value, out var v))
                            NarrativeStateController.Instance?.SetVariable(a.Target, v);
                        break;
                    case NarrativeActionType.LoadScene:
                        var stm = SceneTransitionManager.Instance;
                        if (stm != null) stm.LoadScene(a.Target);
                        else UnityEngine.SceneManagement.SceneManager.LoadScene(a.Target);
                        break;
                }
            }
        }

        void ApplyVisualStateChange()
        {
            if (_data?.VisualStateAfter == null) return;
            var vsc = _data.VisualStateAfter;

            if (vsc.DisableObject)
            {
                gameObject.SetActive(false);
                return;
            }

            if (vsc.DisableRenderer)
            {
                var renderers = GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < renderers.Length; i++)
                    renderers[i].enabled = false;
                return;
            }

            var rend = GetComponentInChildren<Renderer>(true);
            if (rend == null) return;

            var block = new MaterialPropertyBlock();
            rend.GetPropertyBlock(block);

            if (vsc.ChangeColor)
                block.SetColor("_BaseColor", vsc.TargetColor);

            if (vsc.ChangeEmission)
            {
                block.SetColor("_EmissionColor", vsc.TargetColor * vsc.EmissionIntensity);
            }

            rend.SetPropertyBlock(block);

            if (vsc.ChangeScale)
                transform.localScale = vsc.TargetScale;
        }

        void MarkConsumed()
        {
            if (_data != null && _data.OneTimeOnly)
            {
                _consumed = true;
                _interactable.enabled = false;
            }
            NarrativeSaveBridge.Save();
        }

        VN_DialogueController.DialogueLine[] BuildLines(DialogueSequence seq)
        {
            var nodes = seq.Nodes;
            if (nodes == null || nodes.Length == 0)
                return System.Array.Empty<VN_DialogueController.DialogueLine>();

            var stage = AidenStageResolver.ResolveForCurrentLevel();
            var lines = new VN_DialogueController.DialogueLine[nodes.Length];

            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                string text = !string.IsNullOrEmpty(node.TextDirect)
                    ? node.TextDirect
                    : ResolveTextKey(node.TextKey, stage);

                lines[i] = new VN_DialogueController.DialogueLine
                {
                    characterName = node.SpeakerId == "aiden" ? "Aiden" : node.SpeakerId,
                    text = text,
                    spritePath = node.SpritePath,
                    position = MapPosition(node.SpritePos),
                    voiceClipPath = node.VoiceClipPath
                };
            }

            return lines;
        }

        static string ResolveTextKey(string key, AidenStage stage)
        {
            if (string.IsNullOrEmpty(key)) return "...";
            var entry = VN_TextTable.Get(key, stage);
            return entry?.text ?? "...";
        }

        static VN_DialogueController.DialogueLine.SpritePosition MapPosition(DialogueNode.SpritePosition pos)
        {
            return pos switch
            {
                DialogueNode.SpritePosition.Left => VN_DialogueController.DialogueLine.SpritePosition.Left,
                DialogueNode.SpritePosition.Center => VN_DialogueController.DialogueLine.SpritePosition.Center,
                DialogueNode.SpritePosition.Right => VN_DialogueController.DialogueLine.SpritePosition.Right,
                _ => VN_DialogueController.DialogueLine.SpritePosition.None
            };
        }

        IEnumerator WaitForDialogueEnd(VN_DialogueController vnCtrl, DialogueSequence seq)
        {
            float timeout = Time.realtimeSinceStartup + 120f;
            while (vnCtrl.IsActive && Time.realtimeSinceStartup < timeout)
                yield return null;

            if (seq.OnCompleteActions != null)
                ApplyActions(seq.OnCompleteActions);

            ApplyVisualStateChange();
            MarkConsumed();

            NarrativeStateController.Instance?.ExitNarrativeMode();
        }
    }
}
