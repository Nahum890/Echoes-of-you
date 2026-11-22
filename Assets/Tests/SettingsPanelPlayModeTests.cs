using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SettingsPanelPlayModeTests
{
    [UnitySetUp]
    public IEnumerator EntrarEnPlay()
    {
        yield return new EnterPlayMode();
    }

    [UnityTearDown]
    public IEnumerator SalirDePlay()
    {
        yield return new ExitPlayMode();
    }

    [UnityTest]
    public IEnumerator SettingsPanelRendersVisibleAndClickable_InPauseMenu()
    {
        SceneManager.LoadScene("Level_01");
        yield return null;
        yield return null;
        yield return null;

        var pauseType = System.AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType("Echoes.UI.PauseMenu"))
            .FirstOrDefault(t => t != null);
        Assert.IsNotNull(pauseType, "PauseMenu type not found in any assembly");

        var pause = Object.FindFirstObjectByType(pauseType);
        Assert.IsNotNull(pause, "PauseMenu instance not found in Level_01");

        pauseType.GetMethod("Pause", BindingFlags.Public | BindingFlags.Instance).Invoke(pause, null);
        pauseType.GetMethod("ShowSettings", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(pause, null);

        var go = ((Component)pause).gameObject;
        var doc = go.GetComponent<UIDocument>();
        Assert.IsNotNull(doc, "PauseMenu must have a UIDocument");
        var rootEl = doc.rootVisualElement;

        var pauseRoot = rootEl.Q("pause-root");
        var panel = rootEl.Q("pause-settings-panel");
        Assert.IsNotNull(pauseRoot, "pause-root not found");
        Assert.IsNotNull(panel, "pause-settings-panel not found");

        yield return null;
        yield return null;

        Assert.AreEqual(1, panel.childCount, "Settings clone must be added to the panel");

        var wrapper = panel[0];
        Assert.Greater(wrapper.worldBound.height, 100f,
            $"Settings wrapper collapsed to height {wrapper.worldBound.height} — UI invisible/unclickable");

        var settingsRoot = wrapper.Q("settingsRoot");
        Assert.IsNotNull(settingsRoot, "settingsRoot must exist in the clone");
        Assert.Greater(settingsRoot.worldBound.height, 100f,
            $"settingsRoot collapsed to height {settingsRoot.worldBound.height}");

        var btnBack = wrapper.Q<Button>("btnSettingsBack");
        Assert.IsNotNull(btnBack, "btnSettingsBack must exist");
        Assert.Greater(btnBack.worldBound.height, 10f, "btnSettingsBack collapsed");
        Assert.Greater(btnBack.worldBound.width, 50f, "btnSettingsBack collapsed");

        var tabVideo = wrapper.Q<Button>("tabVideo");
        Assert.IsNotNull(tabVideo, "tabVideo must exist");
        Assert.Greater(tabVideo.worldBound.height, 10f, "tabVideo collapsed");

        var panelVideo = wrapper.Q("panelVideo");
        Assert.IsNotNull(panelVideo, "panelVideo must exist");
        Assert.Greater(panelVideo.worldBound.height, 50f, "panelVideo collapsed");
    }
}
