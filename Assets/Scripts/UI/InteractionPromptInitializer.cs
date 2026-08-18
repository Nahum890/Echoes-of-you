using UnityEngine;
using UnityEngine.UIElements;
using Echoes.UI;

public class InteractionPromptInitializer : MonoBehaviour
{
    void Awake()
    {
        // Ensure a singleton InteractionPromptController exists, but we do NOT create a UI document here.
        // The visual UI for the prompt lives in NavigationHUD, which already contains the prompt elements.
        if (Object.FindAnyObjectByType<InteractionPromptController>() != null) return;
        var go = new GameObject("InteractionPromptController");
        go.AddComponent<InteractionPromptController>();
    }
}
