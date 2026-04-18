using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI mínima: eco contador, barra de grabación, leyenda de controles.
/// </summary>
public class GameHUD : MonoBehaviour
{
    [SerializeField] Text echoCounterText;
    [SerializeField] Slider recordSlider;
    [SerializeField] Image recordFill;
    [SerializeField] Color recordingColor = new Color(1f, 0.45f, 0.2f, 1f);
    [SerializeField] Color idleColor = new Color(0.35f, 0.85f, 1f, 1f);

    public void SetEchoCount(int current, int max)
    {
        if (echoCounterText != null)
            echoCounterText.text = $"Ecos: {current} / {max}";
    }

    public void SetRecording(bool recording, float normalizedTime01)
    {
        if (recordSlider != null)
        {
            recordSlider.gameObject.SetActive(true);
            recordSlider.value = Mathf.Clamp01(normalizedTime01);
        }

        if (recordFill != null)
            recordFill.color = recording ? recordingColor : idleColor;
    }
}
