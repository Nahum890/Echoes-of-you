using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public static partial class UIBootstrap
{
    private static void EnsureInputSystemUI()
    {
        var es = Object.FindObjectOfType<EventSystem>();
        if (es == null)
        {
            var go = new GameObject("EventSystem");
            es = go.AddComponent<EventSystem>();
        }
        var standalone = es.GetComponent<StandaloneInputModule>();
        if (standalone != null)
            Object.DestroyImmediate(standalone);
        if (es.GetComponent<InputSystemUIInputModule>() == null)
            es.gameObject.AddComponent<InputSystemUIInputModule>();
    }
}
