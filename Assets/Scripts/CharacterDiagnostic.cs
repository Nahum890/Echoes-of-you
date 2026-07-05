using UnityEngine;

/// <summary>
/// Attach this temporarily to the Player GameObject to diagnose
/// what model and avatar are actually being used at runtime.
/// Remove after verification.
/// </summary>
public class CharacterDiagnostic : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== CHARACTER DIAGNOSTIC ===");

        // Check EchoesLocomotionSettings
        EchoesLocomotionSettings settings = Resources.Load<EchoesLocomotionSettings>("EchoesLocomotionSettings");
        if (settings == null)
        {
            Debug.LogError("[DIAG] EchoesLocomotionSettings NOT FOUND in Resources!");
        }
        else
        {
            Debug.Log("[DIAG] Settings found.");
            Debug.Log("[DIAG] characterModelPrefab: " + (settings.characterModelPrefab != null ? settings.characterModelPrefab.name : "NULL"));
            Debug.Log("[DIAG] humanoidAvatar: " + (settings.humanoidAvatar != null ? settings.humanoidAvatar.name + " isValid=" + settings.humanoidAvatar.isValid + " isHuman=" + settings.humanoidAvatar.isHuman : "NULL"));
            Debug.Log("[DIAG] animatorController: " + (settings.animatorController != null ? settings.animatorController.name : "NULL"));
        }

        // Check PlayerVisual hierarchy
        Transform playerVisual = transform.Find("PlayerVisual");
        if (playerVisual == null)
        {
            Debug.LogWarning("[DIAG] No 'PlayerVisual' child found on this GameObject!");
        }
        else
        {
            Debug.Log("[DIAG] PlayerVisual children count: " + playerVisual.childCount);
            for (int i = 0; i < playerVisual.childCount; i++)
            {
                Transform child = playerVisual.GetChild(i);
                Debug.Log("[DIAG] Child[" + i + "]: " + child.name);
                for (int j = 0; j < child.childCount; j++)
                {
                    Transform grandchild = child.GetChild(j);
                    Debug.Log("[DIAG]   Grandchild[" + j + "]: " + grandchild.name);
                    SkinnedMeshRenderer smr = grandchild.GetComponentInChildren<SkinnedMeshRenderer>(true);
                    if (smr != null)
                        Debug.Log("[DIAG]     SkinnedMesh: " + smr.sharedMesh?.name);
                    Animator anim = grandchild.GetComponent<Animator>();
                    if (anim != null)
                    {
                        Debug.Log("[DIAG]     Animator avatar: " + (anim.avatar != null ? anim.avatar.name + " valid=" + anim.avatar.isValid + " human=" + anim.avatar.isHuman : "NULL"));
                        Debug.Log("[DIAG]     Animator controller: " + (anim.runtimeAnimatorController != null ? anim.runtimeAnimatorController.name : "NULL"));
                    }
                }
            }
        }

        // Check direct Animator
        Animator directAnim = GetComponentInChildren<Animator>(true);
        if (directAnim != null)
        {
            Debug.Log("[DIAG] Direct Animator on: " + directAnim.gameObject.name);
            Debug.Log("[DIAG] Avatar: " + (directAnim.avatar != null ? directAnim.avatar.name + " valid=" + directAnim.avatar.isValid + " human=" + directAnim.avatar.isHuman : "NULL"));
        }

        Debug.Log("=== END DIAGNOSTIC ===");
    }
}
