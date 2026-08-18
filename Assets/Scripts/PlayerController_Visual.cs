using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// PlayerController partial — Visual setup, model alignment, camera focus, and animator asset repair.
/// </summary>
public partial class PlayerController
{
    void EnsureVisualAnimator()
    {
        Transform visualRoot = transform.Find("PlayerVisual");
        if (visualRoot == null)
        {
            GameObject root = new GameObject("PlayerVisual");
            root.transform.SetParent(transform, false);
            visualRoot = root.transform;
        }

        _visualRoot = visualRoot;
        _visualRoot.localPosition = Vector3.zero;
        _visualRoot.localRotation = Quaternion.identity;
        _visualRoot.localScale = Vector3.one;

        if (visualRoot.childCount == 0)
            CreateFallbackVisual(visualRoot);

        _modelRoot = visualRoot.GetChild(0);
        EnsureOptionalComponent("PlayerProceduralAnimator");

        Animator visualAnimator = _modelRoot.GetComponent<Animator>();
        if (visualAnimator == null)
            visualAnimator = _modelRoot.GetComponentInChildren<Animator>(true);
        if (visualAnimator != null)
        {
#if UNITY_EDITOR
            RepairAnimatorAssetLinks(visualAnimator);
#endif
            visualAnimator.applyRootMotion = false;
            visualAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            visualAnimator.updateMode = AnimatorUpdateMode.Normal;
        }

        SkinnedMeshRenderer[] renderers = _modelRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].updateWhenOffscreen = true;

        _anim = visualAnimator;
        AlignModelToFeet();
    }

    void LateUpdate()
    {
        AlignModelToFeet();
    }

#if UNITY_EDITOR
    void RepairAnimatorAssetLinks(Animator animator)
    {
        if (animator == null)
            return;

        if (animator.runtimeAnimatorController == null)
        {
            RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Prefabs/PlayerAnimController.controller");
            if (controller != null)
                animator.runtimeAnimatorController = controller;
        }

        // Intentionally Generic: avatar stays null (transform curves, no muscle retarget).
        // Only repair an assigned-but-broken avatar.
        if (animator.avatar != null && (!animator.avatar.isValid || !animator.avatar.isHuman))
        {
            const string modelPath = "Assets/3D Models/AidenModelo/Aiden3dModelo.fbx";
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(modelPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Avatar avatar && avatar.isValid)
                {
                    animator.avatar = avatar;
                    break;
                }
            }
        }

        EditorUtility.SetDirty(animator);
    }
#endif

    void CreateFallbackVisual(Transform visualRoot)
    {
        _anim = GetComponentInChildren<Animator>();

        GameObject missingModel = new GameObject("PlayerModelMissing");
        missingModel.transform.SetParent(visualRoot, false);
        Debug.LogWarning("PlayerController: no se encontro modelo de personaje; no se crea una capsula visual placeholder.");
    }

    void EnsureCameraFocus()
    {
        Transform focus = transform.Find("CameraFocus");
        if (focus == null)
        {
            GameObject focusObject = new GameObject("CameraFocus");
            focusObject.transform.SetParent(transform, false);
            focus = focusObject.transform;
        }

        float focusHeight = Mathf.Clamp(_controller.height * 0.68f, 1.05f, 1.3f);
        focus.localPosition = new Vector3(0f, focusHeight, 0.08f);
        focus.localRotation = Quaternion.identity;
        focus.localScale = Vector3.one;
    }

    void AlignModelToFeet()
    {
        if (_modelRoot == null || !TryGetModelBounds(out Bounds bounds))
            return;

        Vector3 up = transform.up.sqrMagnitude > 0.001f ? transform.up.normalized : Vector3.up;
        Vector3 lateralOffset = Vector3.ProjectOnPlane(bounds.center - transform.position, up);
        _modelRoot.position -= lateralOffset;

        if (!TryGetModelBounds(out bounds))
            return;

        Vector3 currentFeet = bounds.center - up * bounds.extents.y;
        float feetDelta = Vector3.Dot(transform.position - currentFeet, up);
        _modelRoot.position += up * feetDelta;
    }

    bool TryGetModelBounds(out Bounds bounds)
    {
        bounds = default;
        if (_modelRoot == null)
            return false;

        Renderer[] renderers = _modelRoot.GetComponentsInChildren<Renderer>(true);
        bool found = false;
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rendererRef = renderers[i];
            if (rendererRef == null)
                continue;

            if (!found)
            {
                bounds = rendererRef.bounds;
                found = true;
            }
            else
            {
                bounds.Encapsulate(rendererRef.bounds);
            }
        }

        return found;
    }

    void RefreshCameraReference()
    {
        if (Camera.main != null)
            _cam = Camera.main.transform;
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
