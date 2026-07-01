using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Temporal afterimages: current echo plus delayed ghosts at 0.1s, 0.2s, 0.3s.
/// </summary>
[DisallowMultipleComponent]
public class EchoSpectralTrail : MonoBehaviour
{
    [SerializeField] float snapshotInterval = 0.04f;
    [SerializeField] float[] delayTiers = { 0.1f, 0.2f, 0.3f };
    [SerializeField] float afterimageLifetime = 0.5f;
    [SerializeField] Color afterimageColor = new Color(0.72f, 0.88f, 0.96f, 0.18f);
    [SerializeField] int maxAfterimagesPerTier = 4;

    readonly List<PoseSample> _history = new List<PoseSample>(96);
    Queue<GameObject>[] _tierPools;
    Renderer[] _sourceRenderers;
    float _nextSnapshotTime;
    Material _ghostMaterial;

    struct PoseSample
    {
        public float time;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    void Awake()
    {
        RefreshSources();
        _tierPools = new Queue<GameObject>[delayTiers.Length];
        for (int i = 0; i < _tierPools.Length; i++)
            _tierPools[i] = new Queue<GameObject>();
    }

    void OnEnable()
    {
        _nextSnapshotTime = Time.time;
        _history.Clear();
    }

    void Update()
    {
        if (ShouldPauseTrail())
            return;

        RecordPose();
        SpawnDelayedAfterimages();
        TrimHistory();
    }

    static bool ShouldPauseTrail()
    {
        EchoRecorder recorder = FindAnyObjectByType<EchoRecorder>();
        return recorder != null && recorder.IsRecording;
    }

    void RecordPose()
    {
        if (Time.time < _nextSnapshotTime)
            return;

        _nextSnapshotTime = Time.time + snapshotInterval;
        _history.Add(new PoseSample
        {
            time = Time.time,
            position = transform.position,
            rotation = transform.rotation,
            scale = transform.lossyScale
        });
    }

    void SpawnDelayedAfterimages()
    {
        for (int tier = 0; tier < delayTiers.Length; tier++)
        {
            float delay = delayTiers[tier];
            if (!TryFindPoseAtDelay(delay, out PoseSample pose))
                continue;

            float tierAlpha = afterimageColor.a * Mathf.Lerp(0.85f, 0.35f, tier / Mathf.Max(1f, delayTiers.Length - 1f));
            if (Time.frameCount % (2 + tier) != 0)
                continue;

            SpawnAfterimage(pose, tierAlpha, _tierPools[tier], maxAfterimagesPerTier);
        }
    }

    bool TryFindPoseAtDelay(float delay, out PoseSample pose)
    {
        float targetTime = Time.time - delay;
        pose = default;
        PoseSample? best = null;
        float bestDelta = float.MaxValue;

        for (int i = 0; i < _history.Count; i++)
        {
            float delta = Mathf.Abs(_history[i].time - targetTime);
            if (delta < bestDelta)
            {
                bestDelta = delta;
                best = _history[i];
            }
        }

        if (!best.HasValue || bestDelta > 0.12f)
            return false;

        pose = best.Value;
        return true;
    }

    void TrimHistory()
    {
        float maxDelay = 0.4f;
        for (int i = _history.Count - 1; i >= 0; i--)
        {
            if (Time.time - _history[i].time > maxDelay)
                _history.RemoveAt(i);
        }
    }

    void RefreshSources()
    {
        _sourceRenderers = GetComponentsInChildren<Renderer>(true);
    }

    void SpawnAfterimage(PoseSample pose, float alpha, Queue<GameObject> pool, int maxCount)
    {
        if (_sourceRenderers == null || _sourceRenderers.Length == 0)
            RefreshSources();
        if (_sourceRenderers == null || _sourceRenderers.Length == 0)
            return;

        GameObject root = new GameObject("EchoAfterimage");
        root.transform.SetPositionAndRotation(pose.position, pose.rotation);

        Color color = afterimageColor;
        color.a = alpha;

        for (int i = 0; i < _sourceRenderers.Length; i++)
        {
            Renderer source = _sourceRenderers[i];
            if (source == null || source is ParticleSystemRenderer)
                continue;

            Mesh mesh = ResolveMesh(source);
            if (mesh == null)
                continue;

            GameObject ghost = new GameObject(source.name + "_ghost");
            ghost.transform.SetParent(root.transform, true);
            ghost.transform.localScale = source.transform.lossyScale;

            MeshFilter filter = ghost.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            MeshRenderer rendererRef = ghost.AddComponent<MeshRenderer>();
            rendererRef.sharedMaterial = GetGhostMaterial(color);
            rendererRef.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        pool.Enqueue(root);
        Destroy(root, afterimageLifetime);
        while (pool.Count > maxCount)
        {
            GameObject old = pool.Dequeue();
            if (old != null)
                Destroy(old);
        }
    }

    void OnDisable()
    {
        _history.Clear();
    }

    Mesh ResolveMesh(Renderer rendererRef)
    {
        if (rendererRef.TryGetComponent(out MeshFilter filter))
            return filter.sharedMesh;

        if (rendererRef is SkinnedMeshRenderer skinned)
        {
            Mesh baked = new Mesh();
            skinned.BakeMesh(baked);
            return baked;
        }

        return null;
    }

    Material GetGhostMaterial(Color color)
    {
        if (_ghostMaterial == null)
        {
            _ghostMaterial = new Material(Shader.Find("Sprites/Default"));
            _ghostMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        Material instance = new Material(_ghostMaterial);
        instance.color = color;
        return instance;
    }
}
