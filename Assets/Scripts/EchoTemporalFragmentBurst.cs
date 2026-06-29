using UnityEngine;

/// <summary>
/// Geometric temporal burst when an echo is created — no smoke.
/// </summary>
public static class EchoTemporalFragmentBurst
{
    public static void Play(Vector3 position)
    {
        GameObject root = new GameObject("EchoTemporalBurst");
        root.transform.position = position;

        ParticleSystem ps = root.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.35f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.55f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.14f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.55f, 0.92f, 1f, 0.9f),
            new Color(0.85f, 0.95f, 1f, 0.35f));
        main.maxParticles = 64;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 48), new ParticleSystem.Burst(0.05f, 24) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.35f;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.radial = new ParticleSystem.MinMaxCurve(1.2f, 3.5f);

        var trails = ps.trails;
        trails.enabled = true;
        trails.lifetime = 0.12f;
        trails.widthOverTrail = new ParticleSystem.MinMaxCurve(1f, 0f);

        var rendererRef = root.GetComponent<ParticleSystemRenderer>();
        rendererRef.renderMode = ParticleSystemRenderMode.Stretch;
        rendererRef.lengthScale = 2.2f;
        rendererRef.material = new Material(Shader.Find("Sprites/Default"));

        ps.Play(true);
        Object.Destroy(root, 1.2f);
    }
}
