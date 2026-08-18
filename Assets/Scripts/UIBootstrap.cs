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
#else
                _panelSettings = Resources.Load<PanelSettings>("EchoesPanelSettings");
#endif
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