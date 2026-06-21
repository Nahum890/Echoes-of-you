using UnityEngine;

/// <summary>
/// Garantiza PlayerVisual → PlayerScaler → Model con Animator humanoide.
/// </summary>
[DefaultExecutionOrder(-25)]
public class PlayerCharacterVisualSetup : MonoBehaviour
{
    const string ModelChildName = "Model";
    const string ScalerChildName = "PlayerScaler";
    const string VisualChildName = "PlayerVisual";
    const string ResourcesPrefabPath = "EchoesCharacterVisual";

    void Awake()
    {
        EnsureOn(transform);
    }

    public static void EnsureOn(Transform player)
    {
        if (player == null)
            return;

        Transform visualRoot = player.Find(VisualChildName);
        if (visualRoot == null)
        {
            GameObject visualObject = new GameObject(VisualChildName);
            visualObject.transform.SetParent(player, false);
            visualRoot = visualObject.transform;
        }

        visualRoot.localPosition = Vector3.zero;
        visualRoot.localRotation = Quaternion.identity;
        visualRoot.localScale = Vector3.one;

        Transform model = FindModelTransform(visualRoot);
        if (model == null || !HasSkinnedMesh(model))
        {
            ClearFallbackMeshes(visualRoot);
            model = SpawnModel(visualRoot);
        }

        if (model == null)
            return;

        Transform scaler = model.parent;
        if (scaler == null || scaler.name != ScalerChildName)
        {
            GameObject scalerObject = new GameObject(ScalerChildName);
            scaler = scalerObject.transform;
            scaler.SetParent(visualRoot, false);
            scaler.localPosition = Vector3.zero;
            scaler.localRotation = Quaternion.identity;
            model.SetParent(scaler, false);
        }

        scaler.localScale = Vector3.one * EchoesPresentationSettings.CharacterVisualScale;
        model.localPosition = Vector3.zero;
        model.localRotation = Quaternion.identity;
        model.localScale = Vector3.one;
        RemoveOverlappingFallbacks(visualRoot, model);

        Animator animator = model.GetComponent<Animator>();
        if (animator == null)
            animator = model.gameObject.AddComponent<Animator>();

        EchoesLocomotionSettings settings = Resources.Load<EchoesLocomotionSettings>("EchoesLocomotionSettings");
        if (settings != null)
        {
            if (settings.animatorController != null)
                animator.runtimeAnimatorController = settings.animatorController;
            if (settings.humanoidAvatar != null && settings.humanoidAvatar.isValid)
                animator.avatar = settings.humanoidAvatar;
        }

        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.enabled = true;
    }

    static Transform FindModelTransform(Transform visualRoot)
    {
        Transform direct = visualRoot.Find(ModelChildName);
        if (direct != null)
            return direct;

        for (int i = 0; i < visualRoot.childCount; i++)
        {
            Transform child = visualRoot.GetChild(i);
            if (child.name == ModelChildName)
                return child;

            Transform nested = child.Find(ModelChildName);
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

    static bool HasSkinnedMesh(Transform model)
    {
        return model != null && model.GetComponentInChildren<SkinnedMeshRenderer>(true) != null;
    }

    static void ClearFallbackMeshes(Transform visualRoot)
    {
        for (int i = visualRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = visualRoot.GetChild(i);
            if (child.name.Contains("Capsule") || child.name.Contains("Fallback"))
                DestroySafe(child.gameObject);
        }
    }

    static void RemoveOverlappingFallbacks(Transform visualRoot, Transform activeModel)
    {
        if (visualRoot == null || activeModel == null)
            return;

        Transform activeRoot = activeModel.parent != null && activeModel.parent.name == ScalerChildName
            ? activeModel.parent
            : activeModel;

        for (int i = visualRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = visualRoot.GetChild(i);
            if (child == activeRoot || child == activeModel)
                continue;

            bool looksLikeFallback = child.name.Contains("Capsule") ||
                                     child.name.Contains("Fallback") ||
                                     child.GetComponentInChildren<MeshRenderer>(true) != null ||
                                     child.GetComponentInChildren<SkinnedMeshRenderer>(true) != null;
            if (looksLikeFallback)
                DestroySafe(child.gameObject);
        }
    }

    static Transform SpawnModel(Transform visualRoot)
    {
        EchoesLocomotionSettings settings = Resources.Load<EchoesLocomotionSettings>("EchoesLocomotionSettings");
        GameObject source = settings != null ? settings.characterModelPrefab : null;
        if (source == null)
            source = Resources.Load<GameObject>(ResourcesPrefabPath);

        if (source == null)
            return null;

        GameObject scalerObject = new GameObject(ScalerChildName);
        scalerObject.transform.SetParent(visualRoot, false);

        GameObject instance = Instantiate(source, scalerObject.transform);
        instance.name = ModelChildName;
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        foreach (Collider col in instance.GetComponentsInChildren<Collider>(true))
            DestroySafe(col);

        return instance.transform;
    }

    static void DestroySafe(Object obj)
    {
        if (obj == null)
            return;

        if (Application.isPlaying)
        {
            if (obj is GameObject go)
                go.SetActive(false);
            Object.Destroy(obj);
        }
        else
            Object.DestroyImmediate(obj);
    }
}
