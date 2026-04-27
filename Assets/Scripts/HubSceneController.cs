using UnityEngine;

public class HubSceneController : MonoBehaviour
{
    [SerializeField] string objectiveText = "Elige una memoria desbloqueada.";
    [SerializeField] string introLine = "Tus decisiones dejan puertas abiertas.";

    void Start()
    {
        GameHUD hud = FindAnyObjectByType<GameHUD>();
        if (hud == null)
            return;

        hud.SetObjective(objectiveText);
        hud.SetEchoCount(0, 0);
        hud.SetRecording(false, 0f);
        hud.SetEchoState(string.Empty);

        if (!string.IsNullOrEmpty(introLine))
            hud.ShowToast(introLine, new Color(0.95f, 0.96f, 0.98f, 1f), 2.4f);
    }
}
