using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Echoes.VN
{
    public class VN_EpilogueController : MonoBehaviour
    {
        [SerializeField] float startupDelay = 0.5f;

        IEnumerator Start()
        {
            yield return new WaitForSeconds(startupDelay);
            EndingID ending = VN_EndingResolver.ResolveFromRuntime();
            string scene = VN_EndingResolver.EndingScene(ending);
            if (!string.IsNullOrEmpty(scene) && Application.CanStreamedLevelBeLoaded(scene))
            {
                Debug.Log($"[VN_EpilogueController] Loading epilogue additive: {scene}");
                yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
            }
            else
            {
                Debug.LogWarning($"[VN_EpilogueController] Cannot load epilogue scene '{scene}' - making Credits Scene work standalone.");
            }
        }
    }
}
