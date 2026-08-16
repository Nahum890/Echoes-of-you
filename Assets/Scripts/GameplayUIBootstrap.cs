using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using Echoes.UI;

/// <summary>
/// RuntimeInitializeOnLoadMethod bootstrap: asegura que GameHUD, PauseMenu,
/// ModalManager y SettingsController existan en cada escena de gameplay.
/// No requiere modificaciones a las escenas — se ejecuta automaticamente.
/// </summary>
public static class GameplayUIBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureGameplayUI()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(sceneName)) return;

        // Skip non-gameplay scenes
        if (sceneName == "MainMenu" || sceneName == "CreditsScene" || sceneName == "VN_Dialogue_Test")
            return;

        var panel = UIBootstrap.PanelSettings;
        if (panel == null)
        {
            Debug.LogWarning("[GameplayUIBootstrap] No PanelSettings found; UI not created.");
            return;
        }

        // --- GameHUD ---
        if (Object.FindAnyObjectByType<GameHUD>() == null)
            CreateUIDocument<GameHUD>("GameHUD", "UI/GameHUDUI", panel, 10);

        // --- PauseMenu ---
        if (Object.FindAnyObjectByType<PauseMenu>() == null)
            CreateUIDocument<PauseMenu>("PauseMenu", "UI/PauseMenuUI", panel, 20);

        // --- ModalManager ---
        if (ModalManager.Instance == null)
        {
            var go = new GameObject("ModalManager");
            var doc = go.AddComponent<UIDocument>();
            doc.panelSettings = panel;
            doc.sortingOrder = 30;
            var mm = go.AddComponent<ModalManager>();
            var tmpl = Resources.Load<VisualTreeAsset>("UI/EchoesModal");
            mm.Setup(doc.rootVisualElement, tmpl);
        }

        // --- SettingsController ---
        if (SettingsController.Instance == null)
        {
            var go = new GameObject("SettingsController");
            var sc = go.AddComponent<SettingsController>();
            var tmpl = Resources.Load<VisualTreeAsset>("UI/SettingsUI");
            sc.Setup(null, tmpl);
        }
    }

    static T CreateUIDocument<T>(string name, string vtaPath, PanelSettings panel, int sort) where T : MonoBehaviour
    {
        var go = new GameObject(name);
        var doc = go.AddComponent<UIDocument>();
        doc.panelSettings = panel;
        doc.sortingOrder = sort;
        var vta = Resources.Load<VisualTreeAsset>(vtaPath);
        if (vta != null)
        {
            doc.visualTreeAsset = vta;
        }
        else
        {
            Debug.LogWarning($"[GameplayUIBootstrap] VisualTreeAsset not found at Resources/{vtaPath}");
        }

        var comp = go.AddComponent<T>();
        // Re-trigger OnEnable so it picks up the now-populated rootVisualElement
        comp.enabled = false;
        comp.enabled = true;
        return comp;
    }
}
