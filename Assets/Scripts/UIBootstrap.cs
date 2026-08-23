using UnityEngine;
using UnityEngine.UIElements;
using Echoes.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public static partial class UIBootstrap
{
    const string PanelSettingsPath = "Assets/UI/EchoesPanelSettings.asset";

    static PanelSettings _panelSettings;

    public static PanelSettings PanelSettings
    {
        get
        {
            if (_panelSettings == null)
            {
#if UNITY_EDITOR
                _panelSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
#endif
                // El asset dentro de Resources vive en Assets/Resources/UI/, así que
                // la ruta de carga es "UI/EchoesPanelSettings". La ruta antigua
                // ("EchoesPanelSettings") no existe: en build devolvía null y sin
                // PanelSettings no se creaba **ninguna** UI de gameplay — ni HUD ni
                // menú de pausa.
                if (_panelSettings == null)
                    _panelSettings = Resources.Load<PanelSettings>("UI/EchoesPanelSettings");
                if (_panelSettings == null)
                    _panelSettings = Resources.Load<PanelSettings>("EchoesPanelSettings");
                if (_panelSettings != null)
                {
                    _panelSettings.scale = GameSettings.UIScaleFactor;
                }
                EnsureInputSystemUI();
            }
            return _panelSettings;
        }
    }

    /// <summary>
    /// Instancia una pieza de UI con prefab y devuelve el UIDocument.
    /// </summary>
    public static UIDocument InstantiateUI(string prefabPath, string objectName, int sortingOrder = 0)
    {
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogWarning($"[UIBootstrap] Prefab not found at: {prefabPath}");
            return null;
        }

        GameObject instance = Object.Instantiate(prefab);
        instance.name = objectName;

        var doc = instance.GetComponent<UIDocument>();
        if (doc != null)
        {
            doc.sortingOrder = sortingOrder;
            var ps = PanelSettings;
            if (ps != null && doc.panelSettings == null)
                doc.panelSettings = ps;

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
        }

        return doc;
    }

    /// <summary>
    /// Busca o crea una instancia de UI que sea hija de la escena activa.
    /// </summary>
    public static T EnsureInScene<T>(string prefabPath, string objectName, int sortingOrder = 0) where T : MonoBehaviour
    {
        T existing = Object.FindAnyObjectByType<T>();
        if (existing != null)
            return existing;

        var doc = InstantiateUI(prefabPath, objectName, sortingOrder);
        if (doc == null)
            return null;

        return doc.gameObject.GetComponent<T>();
    }

    // --- Convenience helpers for core UI ---

    public static GameHUD EnsureGameHUD()
    {
        return Object.FindAnyObjectByType<GameHUD>();
    }

    public static PauseMenu EnsurePauseMenu()
    {
        return Object.FindAnyObjectByType<PauseMenu>();
    }
}