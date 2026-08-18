using UnityEngine;
using UnityEngine.UIElements;

namespace Echoes.UI
{
    /// <summary>
    /// Ensures a single NavigationHUD UIDocument exists in the scene.
    /// This runs early (Awake) on a bootstrapper GameObject.
    /// </summary>
    public class NavigationHUDInitializer : MonoBehaviour
    {
        void Awake()
        {
            // If a NavigationHUD already exists, do nothing.
            if (NavigationHUD.Instance != null || Object.FindAnyObjectByType<NavigationHUD>() != null) return;

            var go = new GameObject("NavigationHUD");
            var uidoc = go.AddComponent<UIDocument>();

            // Load the UXML from Resources (Assets/Resources/UI/NavigationHUD/NavigationHUD.uxml)
            var visualTree = Resources.Load<VisualTreeAsset>("UI/NavigationHUD/NavigationHUD");
            if (visualTree == null)
            {
                Debug.LogError("[NavigationHUDInitializer] Could not load NavigationHUD.uxml from Resources.");
                return;
            }
            uidoc.visualTreeAsset = visualTree;
            uidoc.sortingOrder = 25; // on top of normal 3D scene and HUD

            var panel = UIBootstrap.PanelSettings;
            if (panel != null) uidoc.panelSettings = panel;

            // Re-enable UIDocument to force tree cloning
            uidoc.enabled = false;
            uidoc.enabled = true;

            go.AddComponent<NavigationHUD>();
            Debug.Log("[NavigationHUDInitializer] NavigationHUD GameObject created and component added.");
        }
    }
}
