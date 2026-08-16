using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Echoes.UI;

[RequireComponent(typeof(CharacterController))]
public class EchoPlayback : MonoBehaviour
{
    [SerializeField] float skinWidth = 0.08f;
    [SerializeField] Material _matEcho;

    const float EchoHeight = 2.1f;
    const float EchoRadius = 0.36f;
    const string ModelChildName = "Model";
    const string ScalerChildName = "EchoScaler";
    const string VisualChildName = "Visual";
    const string ResourcesPrefabPath = "EchoesCharacterVisual";

    // Ciclo de vida visual del eco (ECHO_GRAMMAR Tabla 8.1 / CONSTANTS_REGISTRY primitives.echo):
    // Latency 0.8s (alpha 0.20, congelado) → Playback (alpha 0.45) → Residual 2.5s (AnalogGhost, 0.30→0).
    const float LatencySeconds = 0.8f;
    const float LatencyAlpha = 0.2f;
    const float DefaultPlaybackAlpha = 0.45f;
    const float ResidualSeconds = 2.5f;
    const float ResidualStartAlpha = 0.3f;

    public enum EchoPlaybackPhase { Latency, Playback, Residual, Gone }
    public event System.Action<EchoPlaybackPhase> PhaseChanged;

    CharacterController _cc;
    readonly List<RecordFrame> _frames = new List<RecordFrame>();
    float _duration;
    float _time;
    bool _playing;
    float _latencyRemaining;
    AudioClip _pendingVoiceClip;

    Animator _anim;
    AudioSource _audioSource;
    float _delayedBlendSpeed;
    Vector3 _delayedLocalVelocity;
    bool _destroying;

    public bool IsPlaying => _playing;
    public float LoopDuration => _duration;

    void Awake()
    {
        transform.localScale = Vector3.one;
        gameObject.layer = LayerMask.NameToLayer("Echo"); // Layer 9
        _cc = GetComponent<CharacterController>();
        _cc.skinWidth = skinWidth;
        _cc.height = EchoHeight;
        _cc.radius = EchoRadius;
        _cc.center = new Vector3(0f, EchoHeight * 0.5f, 0f);
        EnsureVisualAnimator();
        EnsureOptionalComponent("EchoSpectralTrail");
        EnsureOptionalComponent("EchoTemporalVisual");
        EnsureOptionalComponent("CharacterPush");

        // Add CapsuleCollider so other CharacterControllers (like the player) collide with the echo physically
        CapsuleCollider cap = gameObject.GetComponent<CapsuleCollider>();
        if (cap == null)
            cap = gameObject.AddComponent<CapsuleCollider>();
        cap.height = EchoHeight;
        cap.radius = EchoRadius;
        cap.center = new Vector3(0f, EchoHeight * 0.5f, 0f);

        // Ignore collision with all PlayerOnlyBarrier colliders
        // Wrapped in try-catch because the tag may not exist in TagManager
        try
        {
            GameObject[] barriers = GameObject.FindGameObjectsWithTag("PlayerOnlyBarrier");
            foreach (var b in barriers)
            {
                Collider col = b.GetComponent<Collider>();
                if (col != null)
                    Physics.IgnoreCollision(_cc, col);
                Collider childCol = b.GetComponentInChildren<Collider>();
                if (childCol != null)
                    Physics.IgnoreCollision(_cc, childCol);
            }
        }
        catch (UnityException)
        {
            // Tag not defined — no barriers to ignore, which is fine
        }

        RemovePlayerOnlyAnimationBootstraps();
        _anim = ResolveEchoAnimator();
        
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        ConfigureSpatialVoicePlayback();
        RemoveVoiceDegradingFilters();
        
        var audioMgr = EchoesAudioManager.EnsureExists();
        if (audioMgr != null)
        {
            _audioSource.outputAudioMixerGroup = audioMgr.FindGroup("Echo");
        }

    }

    void Start()
    {
        // No pisar el alpha 0.2 del estado Latency (Start llega un frame
        // después de BeginPlayback).
        if (_latencyRemaining <= 0f)
            ApplySavedEchoOpacity();
    }

    public void ApplySavedEchoOpacity()
    {
        SetEchoAlpha(PlayerPrefs.GetFloat("EchoOpacity", DefaultPlaybackAlpha));
    }

    /// Alpha del estado de reproducción (preferencia del jugador, default 0.45).
    float PlaybackAlpha => PlayerPrefs.GetFloat("EchoOpacity", DefaultPlaybackAlpha);

    void SetEchoAlpha(float opacity)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            if (r.sharedMaterial != null)
            {
                Material mat = r.material;
                if (mat.HasProperty("_BaseColor"))
                {
                    Color baseCol = mat.GetColor("_BaseColor");
                    baseCol.a = opacity;
                    mat.SetColor("_BaseColor", baseCol);
                }
                if (mat.HasProperty("_Color"))
                {
                    Color col = mat.GetColor("_Color");
                    col.a = opacity;
                    mat.SetColor("_Color", col);
                }
                if (mat.HasProperty("_Surface"))
                {
                    mat.SetFloat("_Surface", 1f);
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                }
            }
        }
    }

    public void BeginPlayback(IReadOnlyList<RecordFrame> frames, float duration, AudioClip voiceClip = null)
    {
        BeginPlayback(frames, duration, voiceClip, EchoPlaybackMode.Standard, 0f);
    }

    /// <summary>
    /// Begins echo playback with a specific mode and degradation rate.
    /// </summary>
    public void BeginPlayback(IReadOnlyList<RecordFrame> frames, float duration, AudioClip voiceClip,
        EchoPlaybackMode mode, float degradationRate)
    {
        playbackMode = mode;
        degradationPerReplay = degradationRate;
        _playCount = 0;

        EnsureVisualAnimator();
        _anim = ResolveEchoAnimator();
        ApplySavedEchoOpacity();

        _frames.Clear();
        if (frames != null)
        {
            for (int i = 0; i < frames.Count; i++)
                _frames.Add(frames[i]);
        }

        _duration = Mathf.Max(0.05f, duration);
        _time = 0f;
        _playing = _frames.Count > 0;

        if (!_playing)
            return;

        RecordFrame.Evaluate(_frames, 0f, out Vector3 position, out Quaternion rotation);
        _cc.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        _cc.enabled = true;

        // Estado Latency: 0.8s congelado a alpha 0.2 antes de empezar a moverse.
        _latencyRemaining = LatencySeconds;
        PhaseChanged?.Invoke(EchoPlaybackPhase.Latency);
        SetEchoAlpha(LatencyAlpha);
        if (_anim != null)
            _anim.speed = 0f;

        if (mode != EchoPlaybackMode.Standard)
            ApplyAnalogAudioFilters();

        _pendingVoiceClip = voiceClip;
        if (_audioSource != null)
        {
            _audioSource.clip = voiceClip;
            ConfigureSpatialVoicePlayback();
            if (mode == EchoPlaybackMode.Standard)
                RemoveVoiceDegradingFilters();
            // La voz arranca al terminar la latencia, junto con el movimiento.
        }
    }

    void EndLatency()
    {
        _latencyRemaining = 0f;
        SetEchoAlpha(PlaybackAlpha);
        PhaseChanged?.Invoke(EchoPlaybackPhase.Playback);
        if (_audioSource != null && _pendingVoiceClip != null)
            _audioSource.Play();
    }

    void ConfigureSpatialVoicePlayback()
    {
        if (_audioSource == null)
            return;

        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
        _audioSource.volume = 1f;
        _audioSource.pitch = 1f;
        _audioSource.spatialBlend = 1f;
        _audioSource.dopplerLevel = 0.05f;
        _audioSource.spread = 18f;
        _audioSource.minDistance = 4f;
        _audioSource.maxDistance = 42f;
        _audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        _audioSource.bypassEffects = false;
        _audioSource.bypassListenerEffects = false;
        _audioSource.bypassReverbZones = true;
    }

    void RemoveVoiceDegradingFilters()
    {
        AudioLowPassFilter lowPass = GetComponent<AudioLowPassFilter>();
        if (lowPass != null)
            DestroySafe(lowPass);

        AudioReverbFilter reverb = GetComponent<AudioReverbFilter>();
        if (reverb != null)
            DestroySafe(reverb);

        AudioDistortionFilter distortion = GetComponent<AudioDistortionFilter>();
        if (distortion != null)
            DestroySafe(distortion);
    }

    public void StopPlayback()
    {
        _playing = false;
        if (_audioSource != null)
            _audioSource.Stop();
    }

    public void FadeOutAndDestroy(float fadeSeconds = ResidualSeconds)
    {
        if (_destroying)
            return;

        _destroying = true;
        GameFeelController.Instance?.PlayEchoFade(transform.position);
        if (!gameObject.activeInHierarchy)
        {
PhaseChanged?.Invoke(EchoPlaybackPhase.Gone);
        Destroy(gameObject);
            return;
        }

        StartCoroutine(FadeOutAndDestroyRoutine(Mathf.Max(0.05f, fadeSeconds)));
    }

    IEnumerator FadeOutAndDestroyRoutine(float fadeSeconds)
    {
        // Estado Residual: el eco deja de moverse, cambia a AnalogGhost
        // (dithering Bayer + cap 15 FPS) y se desvanece de alpha 0.3 a 0.
        // No usar StopPlayback() aquí: cortaría la voz en seco en vez de fundirla.
        _playing = false;
        SwapToResidualMaterials();
        PhaseChanged?.Invoke(EchoPlaybackPhase.Residual);

        float startVolume = _audioSource != null ? _audioSource.volume : 0f;
        float elapsed = 0f;

        while (elapsed < fadeSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeSeconds);
            if (_audioSource != null)
                _audioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            SetEchoAlpha(Mathf.Lerp(ResidualStartAlpha, 0f, t));
            yield return null;
        }

        Destroy(gameObject);
    }

    void SwapToResidualMaterials()
    {
        Shader ghost = Shader.Find("Echoes/AnalogGhost");
        if (ghost == null)
            return; // Sin el shader, el fade de alpha sigue funcionando sobre el material actual

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            if (r == null)
                continue;

            Material m = new Material(ghost) { name = "Mat_Echo_Residual" };
            m.SetColor("_Color", new Color(0.31f, 0.765f, 0.91f, ResidualStartAlpha));
            m.SetColor("_EmissionColor", new Color(0f, 0.5f, 0.65f, 1f));
            m.SetFloat("_FPS", 15f);
            r.material = m;
        }
    }

    public EchoPlaybackMode playbackMode = EchoPlaybackMode.Standard;
    public float degradationPerReplay = 0.0f;
    private int _playCount = 0;

    void ApplyAnalogAudioFilters()
    {
        AudioLowPassFilter lp = GetComponent<AudioLowPassFilter>();
        if (lp == null) lp = gameObject.AddComponent<AudioLowPassFilter>();
        lp.cutoffFrequency = 2500f;

        AudioHighPassFilter hp = GetComponent<AudioHighPassFilter>();
        if (hp == null) hp = gameObject.AddComponent<AudioHighPassFilter>();
        hp.cutoffFrequency = 400f;
    }

    void FixedUpdate()
    {
        if (!_playing || _frames.Count == 0)
            return;

        if (_latencyRemaining > 0f)
        {
            _latencyRemaining -= Time.deltaTime;
            if (_latencyRemaining > 0f)
                return;
            EndLatency();
        }

        _time += Time.deltaTime;
        if (_time >= _duration)
        {
            _time = 0f;
            _playCount++;
        }

        // Degradation: time offset drifts slightly with each replay loop
        float effectiveTime = Mathf.Clamp(_time + (_playCount * degradationPerReplay), 0f, _duration);
        RecordFrame.Evaluate(_frames, effectiveTime, out Vector3 nextPosition, out Quaternion nextRotation);

        Vector3 moveOffset = nextPosition - transform.position;

        // Keep CharacterController active so it sweeps physically and pushes objects
        _cc.Move(moveOffset);

        // Snap to target if blocked to prevent permanent drift from the recorded path
        if (Vector3.Distance(transform.position, nextPosition) > 0.05f)
        {
            transform.position = nextPosition;
        }
        transform.rotation = nextRotation;

        // Mode specific behaviors
        if (playbackMode == EchoPlaybackMode.Ambient || playbackMode == EchoPlaybackMode.Inversion)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                float dist = Vector3.Distance(transform.position, player.transform.position);
                if (playbackMode == EchoPlaybackMode.Inversion)
                {
                    // Sync HUD feedback
                    var hud = UnityEngine.Object.FindAnyObjectByType<GameHUD>();
                    if (hud != null)
                    {
                        string syncStatus = dist < 0.5f ? "Sincronizado (<0.5m)" : (dist > 1.0f ? "Desincronizado (>1.0m)" : "Cerca");
                        hud.SetPrompt($"Mirror Sync: {syncStatus}", 0.1f);
                    }
                }
            }
        }
    }

    void Update()
    {
        if (!_playing || _frames.Count == 0 || _anim == null || _anim.runtimeAnimatorController == null)
            return;

        if (_latencyRemaining > 0f)
        {
            _anim.speed = 0f; // Modelo congelado durante la latencia
            return;
        }

        RecordFrame.Evaluate(_frames, _time, out Vector3 currentPosition, out _);
        RecordFrame.Evaluate(_frames, _time + Time.deltaTime, out Vector3 nextPosition, out _);

        Vector3 velocity = (nextPosition - currentPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);
        
        float blendSpeed = Mathf.Clamp(velocity.magnitude, 0f, 6.5f);

        // Eerie visual latency: Echo locomotion animations trail sluggishly behind physics frames
        _delayedBlendSpeed = Mathf.Lerp(_delayedBlendSpeed, blendSpeed, Time.deltaTime * 5f);
        _delayedLocalVelocity = Vector3.Lerp(_delayedLocalVelocity, localVelocity, Time.deltaTime * 5f);

        _anim.speed = 0.88f; // Eerily slowed playback animation speed
        EchoesAnimatorParams.SetLocomotion(_anim, _delayedBlendSpeed, _delayedLocalVelocity, true);
        EchoesAnimatorParams.SetBoolIfExists(_anim, "IsRecording", false);
        EchoesAnimatorParams.SetBoolIfExists(_anim, "IsEchoPlayback", _playing);
    }

    Animator ResolveEchoAnimator()
    {
        Transform visual = transform.Find("Visual");
        if (visual == null)
            visual = transform.Find("PlayerVisual");

        if (visual != null)
        {
            Animator modelAnim = visual.GetComponentInChildren<Animator>(true);
            if (modelAnim != null)
                return modelAnim;
        }

        return GetComponentInChildren<Animator>(true);
    }

    void EnsureVisualAnimator()
    {
        Transform playerVisual = transform.Find("PlayerVisual");
        if (playerVisual != null)
            DestroySafe(playerVisual.gameObject);

        Transform visualRoot = transform.Find(VisualChildName);
        if (visualRoot == null)
        {
            GameObject root = new GameObject(VisualChildName);
            root.transform.SetParent(transform, false);
            visualRoot = root.transform;
        }

        visualRoot.localPosition = Vector3.zero;
        visualRoot.localRotation = Quaternion.identity;
        visualRoot.localScale = Vector3.one;

        Transform model = FindModelTransform(visualRoot);
        if (model == null || !HasRenderableModel(model))
        {
            ClearVisualRoot(visualRoot);
            model = SpawnEchoModel(visualRoot);
        }

        if (model != null && HasRenderableModel(model))
            ConfigureEchoModel(model);
    }

    static Transform FindModelTransform(Transform visualRoot)
    {
        if (visualRoot == null)
            return null;

        Transform direct = visualRoot.Find(ModelChildName);
        if (direct != null)
            return direct;

        Transform scaler = visualRoot.Find(ScalerChildName);
        if (scaler != null)
        {
            Transform nested = scaler.Find(ModelChildName);
            if (nested != null)
                return nested;
        }

        Animator[] animators = visualRoot.GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < animators.Length; i++)
        {
            if (animators[i] != null)
                return animators[i].transform;
        }

        return null;
    }

    static bool HasRenderableModel(Transform model)
    {
        if (model == null)
            return false;

        return model.GetComponentInChildren<SkinnedMeshRenderer>(true) != null;
    }

    void ClearVisualRoot(Transform visualRoot)
    {
        if (visualRoot == null)
            return;

        for (int i = visualRoot.childCount - 1; i >= 0; i--)
            DestroySafe(visualRoot.GetChild(i).gameObject);
    }

    Transform SpawnEchoModel(Transform visualRoot)
    {
        // Priority 1: use the prefab configured in EchoesLocomotionSettings
        GameObject source = null;
        EchoesLocomotionSettings settings = EchoesLocomotionSettings.Instance;
        if (settings == null)
            settings = Resources.Load<EchoesLocomotionSettings>("EchoesLocomotionSettings");

        if (settings != null && settings.characterModelPrefab != null)
            source = settings.characterModelPrefab;

        // Priority 2: clone from live player hierarchy
        if (source == null)
            source = FindLivePlayerModelSource();

        // Priority 3: generic prefab from Resources (last resort).
        // "EchoesEchoVisual" reemplaza al antiguo "EchoesCharacterVisual" (LowPolyCharacter
        // bakeado) — usar un prefab dedicado para el eco evita el modelo lowpoly incorrecto.
        if (source == null)
            source = Resources.Load<GameObject>("EchoesEchoVisual");

        if (source == null)
            return null;

        GameObject scalerObject = new GameObject(ScalerChildName);
        scalerObject.transform.SetParent(visualRoot, false);
        scalerObject.transform.localPosition = Vector3.zero;
        scalerObject.transform.localRotation = Quaternion.identity;
        scalerObject.transform.localScale = Vector3.one * EchoesPresentationSettings.CharacterVisualScale;

        GameObject instance = Instantiate(source, scalerObject.transform);
        instance.name = ModelChildName;
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        foreach (Collider col in instance.GetComponentsInChildren<Collider>(true))
            DestroySafe(col);

        return instance.transform;
    }

    static GameObject FindLivePlayerModelSource()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player == null)
            return null;

        Transform visual = player.transform.Find("PlayerVisual");
        if (visual == null)
            return null;

        // Intentar primero el path canónico generado por PlayerCharacterVisualSetup:
        // PlayerVisual → PlayerScaler → Model (el Casual FBX configurado en LocomotionSettings)
        Transform model = visual.Find("PlayerScaler/Model");
        if (model == null)
            model = visual.Find("Model");

        // Si el path canónico falla (jerarquía inesperada), buscar por SkinnedMeshRenderer.
        // Esto evita el fallback previo al Animator, que devolvía el LowPolyCharacter
        // bakeado en el prefab del eco en lugar del modelo del jugador vivo.
        if (model == null || model.GetComponentInChildren<SkinnedMeshRenderer>(true) == null)
        {
            SkinnedMeshRenderer smr = visual.GetComponentInChildren<SkinnedMeshRenderer>(true);
            if (smr != null)
                model = smr.transform.parent != null ? smr.transform.parent : smr.transform;
        }

        if (model != null && model.GetComponentInChildren<Renderer>(true) != null)
            return model.gameObject;

        return null;
    }


    void ConfigureEchoModel(Transform model)
    {
        Transform scaler = model.parent;
        if (scaler == null || scaler.name != ScalerChildName)
        {
            Transform visualRoot = transform.Find(VisualChildName);
            GameObject scalerObject = new GameObject(ScalerChildName);
            scaler = scalerObject.transform;
            scaler.SetParent(visualRoot != null ? visualRoot : transform, false);
            scaler.localPosition = Vector3.zero;
            scaler.localRotation = Quaternion.identity;
            model.SetParent(scaler, false);
        }

        scaler.localPosition = Vector3.zero;
        scaler.localRotation = Quaternion.identity;
        scaler.localScale = Vector3.one * EchoesPresentationSettings.CharacterVisualScale;
        model.localPosition = Vector3.zero;
        model.localRotation = Quaternion.identity;
        model.localScale = Vector3.one;

        foreach (Collider col in model.GetComponentsInChildren<Collider>(true))
            DestroySafe(col);

        ApplyEchoMaterials(model.gameObject);

        Animator animator = model.GetComponent<Animator>();
        if (animator == null)
            animator = model.gameObject.AddComponent<Animator>();

        EchoesLocomotionSettings settings = EchoesLocomotionSettings.Instance;
        if (settings != null)
        {
            if (settings.animatorController != null)
                animator.runtimeAnimatorController = settings.animatorController;
            if (settings.humanoidAvatar != null && settings.humanoidAvatar.isValid)
                animator.avatar = settings.humanoidAvatar;
        }

        EnsureAnimatorController(animator);
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.enabled = true;
    }

    void ApplyEchoMaterials(GameObject root)
    {
        if (_matEcho == null)
        {
            _matEcho = Resources.Load<Material>("Mat_Echo");
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rendererRef = renderers[i];
            if (rendererRef == null)
                continue;

            Material[] materials = rendererRef.materials;
            for (int m = 0; m < materials.Length; m++)
            {
                Material material = _matEcho != null ? new Material(_matEcho) : new Material(materials[m]);
                ConfigureEchoMaterial(material);
                materials[m] = material;
            }

            rendererRef.materials = materials;
        }
    }

    static void ConfigureEchoMaterial(Material material)
    {
        if (material == null)
            return;

        if (material.HasProperty("_Color"))
            material.color = new Color(0.18f, 0.9f, 1f, 0.46f);
        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(0.02f, 0.65f, 1f, 1f) * 1.7f);
        }
        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
    }

    void RemovePlayerOnlyAnimationBootstraps()
    {
        PlayerLocomotionAnimator locomotionAnimator = GetComponent<PlayerLocomotionAnimator>();
        if (locomotionAnimator != null)
            DestroySafe(locomotionAnimator);

        PlayerAnimationRuntimeBootstrap animationBootstrap = GetComponent<PlayerAnimationRuntimeBootstrap>();
        if (animationBootstrap != null)
            DestroySafe(animationBootstrap);
    }

    static void DestroySafe(UnityEngine.Object obj)
    {
        if (obj == null)
            return;

        if (Application.isPlaying)
        {
            if (obj is GameObject go)
                go.SetActive(false);
            Destroy(obj);
        }
        else
            DestroyImmediate(obj);
    }

    static void EnsureAnimatorController(Animator animator)
    {
        if (animator == null || animator.runtimeAnimatorController != null)
            return;

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            Animator playerAnim = player.GetComponentInChildren<Animator>(true);
            if (playerAnim != null && playerAnim.runtimeAnimatorController != null)
            {
                animator.runtimeAnimatorController = playerAnim.runtimeAnimatorController;
                if (playerAnim.avatar != null && playerAnim.avatar.isValid)
                    animator.avatar = playerAnim.avatar;
                return;
            }
        }

#if UNITY_EDITOR
        RuntimeAnimatorController controller = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Prefabs/PlayerAnimController.controller");
        if (controller != null)
            animator.runtimeAnimatorController = controller;
#endif
    }

    void EnsureOptionalComponent(string typeName)
    {
        System.Type type = System.Type.GetType(typeName);
        if (type == null)
        {
            System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length && type == null; i++)
                type = assemblies[i].GetType(typeName);
        }

        if (type != null && GetComponent(type) == null)
            gameObject.AddComponent(type);
    }
}
