using UnityEngine;

/// <summary>
/// Creates subtle atmospheric effects: floating dust motes and low-lying ground fog.
/// Follows the camera for seamless coverage across level geometry.
/// </summary>
public class AtmosphereController : MonoBehaviour
{
    [Header("Dust Particles")]
    [SerializeField] int maxDustMotes = 60;
    [SerializeField] float particleSize = 0.06f;
    [SerializeField] Vector3 spawnVolume = new Vector3(24f, 12f, 24f);

    [Header("Ground Fog (Void Cover)")]
    [SerializeField] bool enableGroundFog = true;
    [SerializeField] int maxFogParticles = 150;
    [SerializeField] float fogParticleSize = 15f;
    [SerializeField] Vector3 fogVolume = new Vector3(80f, 6f, 80f);

    ParticleSystem _dustSystem;
    ParticleSystem _fogSystem;

    void Awake()
    {
        CreateDustSystem();

        if (enableGroundFog)
            CreateGroundFogSystem();

        // La niebla se configura por escena desde EchoesProductionBuilder.SetupAtmosphere
    }

    void CreateDustSystem()
    {
        GameObject go = new GameObject("AtmosphericDust");
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;

        _dustSystem = go.AddComponent<ParticleSystem>();
        var main = _dustSystem.main;
        main.maxParticles = maxDustMotes;
        main.startSize = particleSize;
        main.startLifetime = 12f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = new Color(0.7f, 0.78f, 0.92f, 0.05f);

        var emission = _dustSystem.emission;
        emission.rateOverTime = 5f;

        var shape = _dustSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = spawnVolume;

        var velocity = _dustSystem.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.01f, 0.01f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);

        var noise = _dustSystem.noise;
        noise.enabled = true;
        noise.strength = 0.12f;
        noise.frequency = 0.4f;
        noise.scrollSpeed = 0.1f;
        noise.quality = ParticleSystemNoiseQuality.Medium;

        // Fade in/out over lifetime for softness
        var colorOverLifetime = _dustSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.7f, 0.78f, 0.92f), 0f),
                new GradientColorKey(new Color(0.75f, 0.82f, 0.95f), 0.5f),
                new GradientColorKey(new Color(0.7f, 0.78f, 0.92f), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.05f, 0.25f),
                new GradientAlphaKey(0.10f, 0.75f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = grad;

        var sizeOverLifetime = _dustSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(0.8f, 1.2f);

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
    }

    void CreateGroundFogSystem()
    {
        GameObject go = new GameObject("GroundFog");
        go.transform.SetParent(transform);
        go.transform.localPosition = new Vector3(0f, -8f, 0f);

        _fogSystem = go.AddComponent<ParticleSystem>();
        var main = _fogSystem.main;
        main.maxParticles = maxFogParticles;
        main.startSize = new ParticleSystem.MinMaxCurve(fogParticleSize * 0.8f, fogParticleSize * 1.2f);
        main.startLifetime = 12f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = new Color(0.04f, 0.05f, 0.08f, 0.35f); // Opacidad más suave e índigo profundo
        main.startSpeed = 0f;

        var emission = _fogSystem.emission;
        emission.rateOverTime = 12f;

        var shape = _fogSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = fogVolume;

        var velocity = _fogSystem.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);

        var noise = _fogSystem.noise;
        noise.enabled = true;
        noise.strength = 0.06f;
        noise.frequency = 0.2f;
        noise.scrollSpeed = 0.05f;

        // Very soft fade in/out
        var colorOverLifetime = _fogSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient fogGrad = new Gradient();
        fogGrad.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.04f, 0.06f, 0.10f), 0f),
                new GradientColorKey(new Color(0.06f, 0.08f, 0.12f), 0.5f),
                new GradientColorKey(new Color(0.04f, 0.06f, 0.10f), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.4f, 0.3f),
                new GradientAlphaKey(0.4f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = fogGrad;

        // Scale up over lifetime to simulate fog spreading
        var sizeOverLifetime = _fogSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve(
            new Keyframe(0f, 0.6f),
            new Keyframe(0.5f, 1f),
            new Keyframe(1f, 1.1f)
        );
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingFudge = 10f; // Render behind other particles
    }

    void Update()
    {
        // Keep both particle volumes centered on the camera
        if (Camera.main != null)
        {
            transform.position = Camera.main.transform.position;
        }
    }
}
