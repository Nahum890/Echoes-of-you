using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    private Light targetLight;
    private AudioSource audioSource;
    
    [Header("Flicker Settings")]
    public float baseIntensity = 5f;
    public float minIntensity = 0.2f;
    public float maxIntensity = 1.2f;
    public float flickerSpeed = 0.08f;
    public float intensityVariance = 0.4f;

    public System.Action<float> OnIntensityChange;
    
    [Header("Audio Settings")]
    public float maxHumVolume = 0.15f;

    private float nextActionTime = 0f;

    void Start()
    {
        targetLight = GetComponent<Light>();
        audioSource = GetComponent<AudioSource>();
        
        // Cache the base intensity if it is already set on the light
        if (targetLight != null)
        {
            baseIntensity = targetLight.intensity;
        }
    }

    void Update()
    {
        if (targetLight == null) return;

        // Accesibilidad > "Reducir destellos": el fluorescente se queda fijo en su
        // intensidad base en vez de parpadear. El toggle existía en el menú pero no
        // tenía ningún consumidor en el juego.
        if (EchoesSettings.ReduceFlashes)
        {
            if (!Mathf.Approximately(targetLight.intensity, baseIntensity))
            {
                targetLight.intensity = baseIntensity;
                OnIntensityChange?.Invoke(baseIntensity);
                if (audioSource != null && audioSource.isPlaying)
                    audioSource.volume = maxHumVolume;
            }
            return;
        }

        if (Time.time >= nextActionTime)
        {
            // Genera una intensidad fluctuante para simular mal contacto eléctrico o desgaste
            float noise = Random.Range(minIntensity, maxIntensity);
            targetLight.intensity = baseIntensity * noise;
            OnIntensityChange?.Invoke(targetLight.intensity);
            
            // Si hay un zumbido de audio, sincroniza su volumen con la intensidad
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.volume = maxHumVolume * noise;
            }

            // Intervalo aleatorio rápido de parpadeo
            nextActionTime = Time.time + Random.Range(flickerSpeed * 0.5f, flickerSpeed * 1.5f);
        }
    }
}
