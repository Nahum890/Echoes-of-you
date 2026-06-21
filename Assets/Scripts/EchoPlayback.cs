using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    CharacterController _cc;
    readonly List<RecordFrame> _frames = new List<RecordFrame>();
    float _duration;
    float _time;
    bool _playing;

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
        _cc = GetComponent<CharacterController>();
        _cc.skinWidth = skinWidth;
        _cc.height = EchoHeight;
        _cc.radius = EchoRadius;
        _cc.center = new Vector3(0f, EchoHeight * 0.5f, 0f);
        EnsureVisualAnimator();
        EnsureOptionalComponent("EchoSpectralTrail");
        EnsureOptionalComponent("EchoTemporalVisual");
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
        ApplySavedEchoOpacity();
    }

    public void ApplySavedEchoOpacity()
    {
        float opacity = PlayerPrefs.GetFloat("EchoOpacity", 0.6f);
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            if (r.sharedMaterial != null)
            {
                Material mat = r.material;
                if (mat.HasProperty("_Color"))
                {
                    Color col = mat.color;
                    col.a = opacity;
                    mat.color = col;
                }
                if (mat.HasProperty("_Mode"))
                {
                    mat.SetFloat("_Mode", 3f);
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                }
            }
        }
    }

    public void BeginPlayback(IReadOnlyList<RecordFrame> frames, float duration, AudioClip voiceClip = null)
    {
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

        if (_audioSource != null)
        {
            _audioSource.clip = voiceClip;
            ConfigureSpatialVoicePlayback();
            RemoveVoiceDegradingFilters();

            if (voiceClip != null)
            {
                _audioSource.Play();
            }
        }
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

    public void FadeOutAndDestroy(float fadeSeconds = 0.55f)
    {
        if (_destroying)
            return;

        _destroying = true;
        GameFeelController.Instance?.PlayEchoFade(transform.position);
        if (!gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }

        StartCoroutine(FadeOutAndDestroyRoutine(Mathf.Max(0.05f, fadeSeconds)));
    }

    IEnumerator FadeOutAndDestroyRoutine(float fadeSeconds)
    {
        float startVolume = _audioSource != null ? _audioSource.volume : 0f;
        float elapsed = 0f;

        while (elapsed < fadeSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeSeconds);
            if (_audioSource != null)
                _audioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        StopPlayback();
        Destroy(gameObject);
    }

    void FixedUpdate()
    {
        if (!_playing || _frames.Count == 0)
            return;

        _time += Time.deltaTime;
        if (_time >= _duration)
            _time = 0f;

        RecordFrame.Evaluate(_frames, _time, out Vector3 nextPosition, out Quaternion nextRotation);

        _cc.enabled = false;
        transform.SetPositionAndRotation(nextPosition, nextRotation);
        _cc.enabled = true;
        _cc.Move(-transform.up * 0.001f);
    }

    void Update()
    {
        if (!_playing || _frames.Count == 0 || _anim == null || _anim.runtimeAnimatorController == null)
            return;

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
        EchoesLocomotionSettings settings = EchoesLocomotionSettings.Instance;
        GameObject source = settings != null ? settings.characterModelPrefab : null;
        if (source == null)
            source = Resources.Load<GameObject>(ResourcesPrefabPath);
        if (source == null)
            source = FindLivePlayerModelSource();

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

        Transform model = visual.Find("PlayerScaler/Model") ?? visual.Find("Model");
        if (model != null && model.GetComponentInChildren<Renderer>(true) != null)
            return model.gameObject;

        Animator animator = visual.GetComponentInChildren<Animator>(true);
        if (animator != null && animator.GetComponentInChildren<Renderer>(true) != null)
            return animator.gameObject;

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
        if (material.HasProperty("_Mode"))
        {
            material.SetFloat("_Mode", 3f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_ALPHABLEND_ON");
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

    static void DestroySafe(Object obj)
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
