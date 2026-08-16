#if UNITY_EDITOR
using Echoes.VN;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Echoes.Editor
{
    /// <summary>Creates a standalone, deterministic scene for validating the VN presentation.</summary>
    [InitializeOnLoad]
    public static class VNDialogueTestSceneCreator
    {
        const string ScenePath = "Assets/Scenes/VN_Dialogue_Test.unity";

        static VNDialogueTestSceneCreator()
        {
            EditorApplication.delayCall += CreateIfMissing;
        }

        [MenuItem("Echoes/VN/Create or Repair Test Scene")]
        public static void CreateIfMissing()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
                return;

            Scene previousScene = SceneManager.GetActiveScene();
            Scene testScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(testScene);

            var cameraObject = new GameObject("Main Camera");
            var camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            camera.transform.SetPositionAndRotation(new Vector3(0f, 3.4f, -12f), Quaternion.Euler(12f, 0f, 0f));
            camera.backgroundColor = new Color(0.025f, 0.04f, 0.08f, 1f);

            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.3f;
            light.color = new Color(0.7f, 0.83f, 1f);
            lightObject.transform.rotation = Quaternion.Euler(48f, -28f, 0f);

            var harnessObject = new GameObject("VN Dialogue Test Harness");
            var harness = harnessObject.AddComponent<VNDialogueTestHarness>();
            harness.SetTestCamera(camera);

            CreateFloor();
            CreateInteractable("Interactuable_Reloj", new Vector3(-4f, 1f, 0f), PrimitiveType.Cylinder, "Reloj detenido", "El reloj marca las 03:17. Aiden parece recordar algo que aun no sucedio.", "VN/Sprites/aiden/Aiden_Pensativa", new Color(0.34f, 0.78f, 1f));
            CreateInteractable("Interactuable_Nota", new Vector3(0f, 1f, 0f), PrimitiveType.Cube, "Nota doblada", "La tinta dice: No olvides que la salida tambien puede ser un comienzo.", "VN/Sprites/aiden/Aiden_Perturbada", new Color(1f, 0.72f, 0.22f));
            CreateInteractable("Interactuable_Espejo", new Vector3(4f, 1.4f, 0f), PrimitiveType.Cube, "Espejo empañado", "Por un instante, el reflejo de Aiden sonríe antes que tú.", "VN/Sprites/aiden/Aiden_Feliz", new Color(0.72f, 0.48f, 1f), new Vector3(1.7f, 2.8f, 0.25f));

            EditorSceneManager.SaveScene(testScene, ScenePath);
            EditorSceneManager.CloseScene(testScene, true);
            if (previousScene.IsValid() && previousScene.isLoaded)
                SceneManager.SetActiveScene(previousScene);
            AssetDatabase.SaveAssets();
            Debug.Log("[VN Test] Created standalone scene at " + ScenePath);
        }

        static void CreateFloor()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Test Floor";
            floor.transform.localScale = new Vector3(2f, 1f, 1.2f);
            ApplyColor(floor, new Color(0.045f, 0.07f, 0.12f));
        }

        static void CreateInteractable(string name, Vector3 position, PrimitiveType type, string title, string text, string spritePath, Color color, Vector3? scale = null)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.position = position;
            if (scale.HasValue) go.transform.localScale = scale.Value;
            ApplyColor(go, color);
            go.AddComponent<VNDialogueTestInteractable>().Configure(title, text, spritePath);
        }

        static void ApplyColor(GameObject target, Color color)
        {
            var renderer = target.GetComponent<Renderer>();
            if (renderer == null) return;
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            renderer.sharedMaterial = material;
        }
    }
}
#endif
