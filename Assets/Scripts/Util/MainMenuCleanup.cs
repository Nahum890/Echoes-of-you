using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuCleanup : MonoBehaviour
{
    static readonly string[] MainMenuRootsToDisable = {
        "AtmosphereController",
        "Directional Light",
        "Main Camera",
        "MenuFloor",
        "MenuHorizon_PillarL_Near",
        "MenuHorizon_PillarR_Near",
        "MenuHorizon_PillarL_Far",
        "MenuHorizon_PillarR_Far",
        "MenuMonolith",
        "--- DISTANT VISUALS ---",
        "MenuGlow",
        "EnvironmentParticles",
        "MainMenuUI",
        "MainMenuCinematicWorld",
        "SettingsController",
    };

    void Start()
    {
        DisableMainMenuRoots();
    }

    public static void DisableMainMenuRoots()
    {
        var mainMenu = SceneManager.GetSceneByName("MainMenu");
        if (!mainMenu.IsValid()) return;

        foreach (var rootName in MainMenuRootsToDisable)
        {
            foreach (var root in mainMenu.GetRootGameObjects())
            {
                if (root.name == rootName)
                {
                    root.SetActive(false);
                    break;
                }
            }
        }

        Debug.Log("[MainMenuCleanup] Disabled 15 overlapping MainMenu roots while Level is active.");
    }
}