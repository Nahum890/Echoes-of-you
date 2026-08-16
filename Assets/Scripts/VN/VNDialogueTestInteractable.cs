using UnityEngine;

namespace Echoes.VN
{
    /// <summary>Clickable object used only by VN_Dialogue_Test to exercise input and dialogue.</summary>
    [RequireComponent(typeof(Collider))]
    public sealed class VNDialogueTestInteractable : MonoBehaviour
    {
        [SerializeField] string objectTitle = "Objeto";
        [SerializeField, TextArea] string dialogueText = "Has interactuado con un objeto de prueba.";
        [SerializeField] string spriteResourcePath = "VN/Sprites/aiden/Aiden_Perturbada";

        public void Configure(string title, string text, string spritePath)
        {
            objectTitle = title;
            dialogueText = text;
            spriteResourcePath = spritePath;
        }

        public void Interact(VNDialogueTestHarness dialogue)
        {
            if (dialogue != null)
                dialogue.StartInteraction(objectTitle, dialogueText, spriteResourcePath);
        }
    }
}
