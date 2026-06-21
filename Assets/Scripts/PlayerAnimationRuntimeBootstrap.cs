using UnityEngine;

/// <summary>
/// Asigna AnimatorController + Avatar al modelo humanoide del jugador/eco.
/// </summary>
[DefaultExecutionOrder(-20)]
public class PlayerAnimationRuntimeBootstrap : MonoBehaviour
{
    static EchoesLocomotionSettings _settings;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void LoadSettings()
    {
        _settings = Resources.Load<EchoesLocomotionSettings>("EchoesLocomotionSettings");
    }

    void Awake()
    {
        ApplyToHierarchy(gameObject);
    }

    public static void ApplyToHierarchy(GameObject root)
    {
        if (root == null || root.GetComponent<EchoPlayback>() != null)
            return;

        if (_settings == null)
            _settings = Resources.Load<EchoesLocomotionSettings>("EchoesLocomotionSettings");

        Animator animator = FindHumanoidAnimator(root);
        if (animator == null)
            return;

        Transform scaler = root.transform.Find("PlayerScaler");
        if (scaler != null)
            scaler.localScale = Vector3.one * EchoesPresentationSettings.CharacterVisualScale;

        RuntimeAnimatorController controller = _settings != null ? _settings.animatorController : null;
        Avatar avatar = _settings != null ? _settings.humanoidAvatar : null;

        if (controller != null)
            animator.runtimeAnimatorController = controller;

        if (avatar != null && avatar.isValid)
            animator.avatar = avatar;

        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.updateMode = AnimatorUpdateMode.Normal;
        animator.speed = EchoesPresentationSettings.AnimationPlaybackSpeed;
        animator.enabled = true;

        SkinnedMeshRenderer[] renderers = animator.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].updateWhenOffscreen = true;

        animator.Rebind();
        animator.Update(0f);
    }

    static Animator FindHumanoidAnimator(GameObject root)
    {
        Transform visual = root.transform.Find("PlayerVisual");
        if (visual == null)
            visual = root.transform.Find("Visual");

        if (visual != null)
        {
            Transform model = visual.Find("Model");
            if (model == null)
            {
                for (int i = 0; i < visual.childCount; i++)
                {
                    model = visual.GetChild(i).Find("Model");
                    if (model != null)
                        break;
                }
            }

            if (model != null && model.TryGetComponent(out Animator modelAnim))
                return modelAnim;
        }

        Animator[] animators = root.GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < animators.Length; i++)
        {
            if (animators[i] != null && animators[i].gameObject != root)
                return animators[i];
        }

        return null;
    }
}
