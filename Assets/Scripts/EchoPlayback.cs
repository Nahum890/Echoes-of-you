using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EchoPlayback : MonoBehaviour
{
    [SerializeField] float skinWidth = 0.08f;
    [SerializeField] Material _matEcho;

    CharacterController _cc;
    readonly List<RecordFrame> _frames = new List<RecordFrame>();
    float _duration;
    float _time;
    bool _playing;

    Animator _anim;
    AudioSource _audioSource;

    public bool IsPlaying => _playing;
    public float LoopDuration => _duration;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _cc.skinWidth = skinWidth;
        EnsureVisualAnimator();
        EnsureOptionalComponent("EchoSpectralTrail");
        EnsureOptionalComponent("EchoTemporalVisual");
        EnsureOptionalComponent("PlayerLocomotionAnimator");
        _anim = ResolveEchoAnimator();
        
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.spatialBlend = 1f; // 3D sound
            _audioSource.loop = true;
            _audioSource.volume = 0.2f;
            _audioSource.minDistance = 2f;
            _audioSource.maxDistance = 15f;
            _audioSource.rolloffMode = AudioRolloffMode.Linear;
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

    public void BeginPlayback(IReadOnlyList<RecordFrame> frames, float duration)
    {
        PlayerAnimationRuntimeBootstrap.ApplyToHierarchy(gameObject);
        _anim = ResolveEchoAnimator();

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

        if (_audioSource != null && _audioSource.clip != null)
        {
            _audioSource.Play();
        }
    }

    public void StopPlayback()
    {
        _playing = false;
        if (_audioSource != null)
            _audioSource.Stop();
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
        _anim.speed = 1f;
        EchoesAnimatorParams.SetLocomotion(_anim, blendSpeed, localVelocity, true);
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
        Transform visualRoot = transform.Find("PlayerVisual");
        if (visualRoot == null)
            visualRoot = transform.Find("Visual");
        if (visualRoot == null)
        {
            GameObject root = new GameObject("Visual");
            root.transform.SetParent(transform, false);
            visualRoot = root.transform;
        }

        if (visualRoot.childCount == 0)
            CreateFallbackVisual(visualRoot);

        Transform modelRoot = visualRoot.GetChild(0);
        Animator visualAnimator = modelRoot.GetComponent<Animator>();
        if (visualAnimator == null)
            visualAnimator = modelRoot.GetComponentInChildren<Animator>(true);
        if (visualAnimator != null)
        {
            visualAnimator.applyRootMotion = false;
            visualAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            EnsureAnimatorController(visualAnimator);
        }

        SkinnedMeshRenderer[] renderers = modelRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].updateWhenOffscreen = true;
    }

    void CreateFallbackVisual(Transform visualRoot)
    {
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.name = "EchoCapsule";
        capsule.transform.SetParent(visualRoot, false);
        capsule.transform.localPosition = new Vector3(0f, 1.05f, 0f);
        capsule.transform.localRotation = Quaternion.identity;
        capsule.transform.localScale = new Vector3(0.8f, 1.05f, 0.8f);

        Collider capsuleCollider = capsule.GetComponent<Collider>();
        if (capsuleCollider != null)
            Destroy(capsuleCollider);

        Renderer rendererRef = capsule.GetComponent<Renderer>();
        if (rendererRef != null)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.32f, 0.92f, 1f, 0.45f);
            material.SetFloat("_Mode", 3f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_ALPHABLEND_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            rendererRef.sharedMaterial = material;
        }
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
