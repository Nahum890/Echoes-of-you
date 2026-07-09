using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Limpieza quirúrgica: elimina de todas las escenas el GameObject "GameOverUI"
/// que quedó huérfano tras borrar GameOverController.cs / GameOverUI.uxml.
/// NO regenera escenas ni toca iluminación, menús ni ningún otro objeto:
/// abre cada escena, borra solo el/los "GameOverUI" y la guarda si hubo cambios.
/// </summary>
public static class EchoesGameOverCleanup
{
    [MenuItem("Echoes of You/Cleanup/Remove Orphaned GameOverUI", false, 400)]
    public static void RemoveOrphanedGameOverUI()
    {
        // No perder cambios sin guardar de la escena abierta.
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        string originalScenePath = EditorSceneManager.GetActiveScene().path;

        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        int totalRemoved = 0;
        int scenesModified = 0;

        foreach (string guid in sceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            // Recolectar primero, borrar después (evita invalidar la iteración).
            List<GameObject> toDelete = new List<GameObject>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (t != null && t.gameObject.name == "GameOverUI")
                        toDelete.Add(t.gameObject);
                }
            }

            if (toDelete.Count == 0)
                continue;

            foreach (GameObject go in toDelete)
            {
                if (go != null) // pudo quedar destruido si era hijo de otro ya borrado
                {
                    Object.DestroyImmediate(go);
                    totalRemoved++;
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            scenesModified++;
            Debug.Log($"[GameOver Cleanup] {path}: eliminado GameOverUI");
        }

        Debug.Log($"[GameOver Cleanup] Listo — {totalRemoved} objeto(s) eliminado(s) en {scenesModified} escena(s).");

        // Restaurar la escena que estaba abierta.
        if (!string.IsNullOrEmpty(originalScenePath))
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
    }
}
