using UnityEngine;
using UnityEngine.UIElements;
using Echoes.UI;

public class InteractionPromptInitializer : MonoBehaviour
{
    void Awake()
    {
        if (Object.FindAnyObjectByType<InteractionPromptController>() != null) return;

        VisualTreeAsset visualTree = null;

#if UNITY_EDITOR
        visualTree = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/Resources/UI/InteractionPromptUI.uxml");
#endif
        if (visualTree == null)
            visualTree = Resources.Load<VisualTreeAsset>("UI/InteractionPromptUI");

        if (visualTree == null)
        {
            Debug.LogError("[InteractionPromptInitializer] No se encontro InteractionPromptUI.uxml");
            return;
        }

        var go = new GameObject("InteractionPromptUI");
        var uidoc = go.AddComponent<UIDocument>();
        uidoc.visualTreeAsset = visualTree;
        uidoc.sortingOrder = 200;

        var panel = Resources.Load<PanelSettings>("EchoesPanelSettings");
#if UNITY_EDITOR
        if (panel == null)
            panel = UnityEditor.AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/UI/EchoesPanelSettings.asset");
#endif
        if (panel != null) uidoc.panelSettings = panel;

        go.AddComponent<InteractionPromptController>();
    }
}
