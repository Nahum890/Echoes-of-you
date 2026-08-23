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
    static bool _sceneHooked;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        if (!_sceneHooked)
        {
            SceneManager.sceneLoaded += (scene, mode) => EnsureGameplayUI();
            _sceneHooked = true;
        }
        EnsureGameplayUI();
    }

    public static void EnsureGameplayUI()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(sceneName)) return;

        // Skip non-gameplay scenes
        if (sceneName == "MainMenu" || sceneName == "CreditsScene")
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
        // Todos los UIDocument comparten el mismo PanelSettings, así que están en
        // un único panel de UI Toolkit y el picking lo decide sortingOrder. El menú
        // de pausa iba a 20, por debajo del overlay del VN (500) y del tutorial
        // (550): esas capas a pantalla completa se comían sus clics aunque no
        // estuvieran mostrando nada.
        var pauseMenu = Object.FindAnyObjectByType<PauseMenu>();
        if (pauseMenu == null)
            pauseMenu = CreateUIDocument<PauseMenu>("PauseMenu", "UI/PauseMenuUI", panel, PauseMenu.PauseSortingOrder);

        // Las escenas traen su propio PauseMenu serializado con sortingOrder 10;
        // hay que subirlo también a él.
        var pauseDoc = pauseMenu != null ? pauseMenu.GetComponent<UIDocument>() : null;
        if (pauseDoc != null && pauseDoc.sortingOrder < PauseMenu.PauseSortingOrder)
            pauseDoc.sortingOrder = PauseMenu.PauseSortingOrder;

        // --- ModalManager ---
        if (ModalManager.Instance == null)
        {
            var go = new GameObject("ModalManager");
            var doc = go.AddComponent<UIDocument>();
            doc.panelSettings = panel;
            doc.sortingOrder = PauseMenu.ModalSortingOrder; // por encima del propio menú
            var mm = go.AddComponent<ModalManager>();
            var tmpl = Resources.Load<VisualTreeAsset>("UI/EchoesModal");
            mm.Setup(doc.rootVisualElement, tmpl);
        }
        else
        {
            var modalDoc = ModalManager.Instance.GetComponent<UIDocument>();
            if (modalDoc != null && modalDoc.sortingOrder < PauseMenu.ModalSortingOrder)
                modalDoc.sortingOrder = PauseMenu.ModalSortingOrder;
        }

        // --- SettingsController ---
        if (SettingsController.Instance == null)
        {
            var go = new GameObject("SettingsController");
            var sc = go.AddComponent<SettingsController>();
            var tmpl = Resources.Load<VisualTreeAsset>("UI/SettingsUI");
            sc.Setup(null, tmpl);
        }

        // --- VN Dialogue & Choice System ---
        if (Echoes.VN.VN_DialogueController.Instance == null && Object.FindAnyObjectByType<Echoes.VN.VN_DialogueController>() == null)
        {
            var go = new GameObject("VNDialogueController");
            var doc = go.AddComponent<UIDocument>();
            doc.panelSettings = panel;
            var vta = Resources.Load<VisualTreeAsset>("UI/VN/VN_DialogueUI");
#if UNITY_EDITOR
            if (vta == null) vta = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/VN/VN_DialogueUI.uxml");
#endif
            if (vta != null) doc.visualTreeAsset = vta;
            go.AddComponent<Echoes.VN.VN_DialogueController>();
            if (Application.isPlaying) Object.DontDestroyOnLoad(go);
        }

        if (Echoes.VN.VN_ChoiceGateController.Instance == null && Object.FindAnyObjectByType<Echoes.VN.VN_ChoiceGateController>() == null)
        {
            var go = new GameObject("VNChoiceGateController");
            go.AddComponent<Echoes.VN.VN_ChoiceGateController>();
            if (Application.isPlaying) Object.DontDestroyOnLoad(go);
        }

        // --- Tutorial Overlay ---
        if (Echoes.UI.TutorialOverlayController.Instance == null && Object.FindAnyObjectByType<Echoes.UI.TutorialOverlayController>() == null)
        {
            var go = new GameObject("TutorialOverlayController");
            var doc = go.AddComponent<UIDocument>();
            doc.panelSettings = panel;
            doc.sortingOrder = 550;
            var vta = Resources.Load<VisualTreeAsset>("UI/Tutorial/TutorialOverlayUI");
#if UNITY_EDITOR
            if (vta == null) vta = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/Tutorial/TutorialOverlayUI.uxml");
#endif
            if (vta != null) doc.visualTreeAsset = vta;
            go.AddComponent<Echoes.UI.TutorialOverlayController>();
            if (Application.isPlaying) Object.DontDestroyOnLoad(go);
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
        if (doc.rootVisualElement != null)
        {
            var r = doc.rootVisualElement;
            r.style.position = Position.Absolute;
            r.style.left = 0;
            r.style.top = 0;
            r.style.right = 0;
            r.style.bottom = 0;
            r.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
            r.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
        }
        // Re-trigger OnEnable so it picks up the now-populated rootVisualElement
        comp.enabled = false;
        comp.enabled = true;
        return comp;
    }
}
