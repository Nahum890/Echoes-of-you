using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Validación visual del pase de arte técnico (criterios de aceptación):
///  - ≤ 48 luces por escena (CONS-LGT-002 → FAIL-LGT-01)
///  - 0 luces con sombras Soft
///  - 0 materiales magenta (shader de error o material nulo)
///  - Captura spawn / puzzle / salida por nivel y regresión visual contra
///    Docs/Art/ReferenceFrames (pixel diff ≤ 2%, SSIM ≥ 0.98).
///
/// Si no existe baseline para una vista, la captura actual SE CONVIERTE en
/// baseline y la vista se marca "baseline_created" (no se puede comparar
/// contra referencias que nunca existieron).
///
/// Salida: Reports/generated/visual_regression_report.json
/// </summary>
public static class EchoesVisualValidationPass
{
    const string SceneRoot = "Assets/Scenes";
    const string ReferenceRoot = "Docs/Art/ReferenceFrames";
    const string ReportPath = "Reports/generated/visual_regression_report.json";
    const int CaptureWidth = 1280;
    const int CaptureHeight = 720;
    const int MaxLightsPerScene = 48;
    const float PixelDiffTolerance = 4f / 255f; // delta por canal para contar un píxel como distinto

    [Serializable] class ViewResult
    {
        public string view;
        public string status;        // "ok" | "fail" | "baseline_created" | "no_target"
        public float pixel_diff_pct = -1f;
        public float ssim = -1f;
    }

    [Serializable] class LevelResult
    {
        public string scene;
        public int light_count;
        public bool lights_ok;
        public int soft_shadow_lights;
        public int magenta_materials;
        public List<ViewResult> views = new();
    }

    [Serializable] class Report
    {
        public string generated_utc;
        public string note = "SSIM/pixel-diff solo válidos con baseline previa en Docs/Art/ReferenceFrames";
        public List<LevelResult> levels = new();
    }

    [MenuItem("Echoes of You/Technical Art/Validate All Levels (Visual)", false, 405)]
    public static void ValidateAllLevels()
    {
        Directory.CreateDirectory(ReferenceRoot);
        Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));

        var report = new Report { generated_utc = DateTime.UtcNow.ToString("o") };

        for (int level = 1; level <= 15; level++)
        {
            string scenePath = $"{SceneRoot}/Level_{level:00}.unity";
            if (!File.Exists(scenePath))
                continue;

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            report.levels.Add(ValidateScene(scene));
        }

        File.WriteAllText(ReportPath, JsonUtility.ToJson(report, true));
        Debug.Log($"[VisualValidation] Reporte escrito en {ReportPath}");
    }

    static LevelResult ValidateScene(Scene scene)
    {
        var result = new LevelResult { scene = scene.name };

        // Luces
        var lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude);
        result.light_count = lights.Length;
        result.lights_ok = lights.Length <= MaxLightsPerScene;
        foreach (var light in lights)
            if (light.shadows == LightShadows.Soft)
                result.soft_shadow_lights++;

        // Materiales magenta (shader roto o material nulo)
        foreach (var rendererRef in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude))
        {
            foreach (var mat in rendererRef.sharedMaterials)
            {
                if (mat == null || mat.shader == null ||
                    mat.shader.name == "Hidden/InternalErrorShader")
                {
                    result.magenta_materials++;
                }
            }
        }

        // Capturas: spawn / puzzle / salida
        result.views.Add(CaptureAndCompare(scene.name, "spawn", FindSpawnFocus()));
        result.views.Add(CaptureAndCompare(scene.name, "puzzle", FindPuzzleFocus()));
        result.views.Add(CaptureAndCompare(scene.name, "exit", FindExitFocus()));
        return result;
    }

    // ─── Focos de vista ──────────────────────────────────────────────────

    static Transform FindSpawnFocus()
    {
        var player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
        if (player != null) return player.transform;
        var marker = UnityEngine.Object.FindAnyObjectByType<LevelSpawnMarker>();
        return marker != null ? marker.transform : null;
    }

    static Transform FindPuzzleFocus()
    {
        var goal = UnityEngine.Object.FindAnyObjectByType<LevelGoal>();
        if (goal != null) return goal.transform;
        var plate = UnityEngine.Object.FindAnyObjectByType<PressurePlate>();
        return plate != null ? plate.transform : null;
    }

    static Transform FindExitFocus()
    {
        var exit = UnityEngine.Object.FindAnyObjectByType<LevelExit>();
        return exit != null ? exit.transform : null;
    }

    // ─── Captura y regresión ─────────────────────────────────────────────

    static ViewResult CaptureAndCompare(string sceneName, string viewName, Transform focus)
    {
        var view = new ViewResult { view = viewName };
        if (focus == null)
        {
            view.status = "no_target";
            return view;
        }

        Texture2D capture = CaptureView(focus);
        string baselinePath = $"{ReferenceRoot}/{sceneName}_{viewName}.png";

        try
        {
            if (!File.Exists(baselinePath))
            {
                File.WriteAllBytes(baselinePath, capture.EncodeToPNG());
                view.status = "baseline_created";
                return view;
            }

            var baseline = new Texture2D(2, 2);
            baseline.LoadImage(File.ReadAllBytes(baselinePath));
            if (baseline.width != capture.width || baseline.height != capture.height)
            {
                view.status = "fail"; // resolución distinta a la referencia
                UnityEngine.Object.DestroyImmediate(baseline);
                return view;
            }

            Color[] reference = baseline.GetPixels();
            Color[] current = capture.GetPixels();
            view.pixel_diff_pct = PixelDiffPercent(reference, current);
            view.ssim = Ssim(reference, current, capture.width, capture.height);
            view.status = (view.pixel_diff_pct <= 2f && view.ssim >= 0.98f) ? "ok" : "fail";
            UnityEngine.Object.DestroyImmediate(baseline);
            return view;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(capture);
        }
    }

    static Texture2D CaptureView(Transform focus)
    {
        var camObject = new GameObject("~VisualValidationCamera");
        var cam = camObject.AddComponent<Camera>();
        cam.fieldOfView = 52f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 500f;

        // Encuadre determinista: detrás y por encima del foco, mirando al foco.
        Vector3 offset = new Vector3(-5.5f, 3.2f, -9.5f); // offset base de cámara del juego
        cam.transform.position = focus.position + offset;
        cam.transform.LookAt(focus.position + Vector3.up * 1.2f);

        var rt = new RenderTexture(CaptureWidth, CaptureHeight, 24);
        cam.targetTexture = rt;
        cam.Render();

        RenderTexture.active = rt;
        var tex = new Texture2D(CaptureWidth, CaptureHeight, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, CaptureWidth, CaptureHeight), 0, 0);
        tex.Apply();

        RenderTexture.active = null;
        cam.targetTexture = null;
        UnityEngine.Object.DestroyImmediate(rt);
        UnityEngine.Object.DestroyImmediate(camObject);
        return tex;
    }

    // ─── Métricas ────────────────────────────────────────────────────────

    static float PixelDiffPercent(Color[] a, Color[] b)
    {
        int different = 0;
        for (int i = 0; i < a.Length; i++)
        {
            float delta = Mathf.Max(
                Mathf.Abs(a[i].r - b[i].r),
                Mathf.Abs(a[i].g - b[i].g),
                Mathf.Abs(a[i].b - b[i].b));
            if (delta > PixelDiffTolerance)
                different++;
        }
        return 100f * different / a.Length;
    }

    /// SSIM global por ventanas 8×8 sobre luminancia (aprox. estándar, C1/C2
    /// de la definición con L=1).
    static float Ssim(Color[] a, Color[] b, int width, int height)
    {
        const int win = 8;
        const float c1 = 0.01f * 0.01f;
        const float c2 = 0.03f * 0.03f;

        float total = 0f;
        int windows = 0;

        for (int by = 0; by + win <= height; by += win)
        for (int bx = 0; bx + win <= width; bx += win)
        {
            float meanA = 0f, meanB = 0f;
            for (int y = 0; y < win; y++)
            for (int x = 0; x < win; x++)
            {
                int i = (by + y) * width + bx + x;
                meanA += Luma(a[i]);
                meanB += Luma(b[i]);
            }
            int n = win * win;
            meanA /= n; meanB /= n;

            float varA = 0f, varB = 0f, cov = 0f;
            for (int y = 0; y < win; y++)
            for (int x = 0; x < win; x++)
            {
                int i = (by + y) * width + bx + x;
                float da = Luma(a[i]) - meanA;
                float db = Luma(b[i]) - meanB;
                varA += da * da;
                varB += db * db;
                cov += da * db;
            }
            varA /= n - 1; varB /= n - 1; cov /= n - 1;

            float ssim = ((2f * meanA * meanB + c1) * (2f * cov + c2)) /
                         ((meanA * meanA + meanB * meanB + c1) * (varA + varB + c2));
            total += ssim;
            windows++;
        }

        return windows > 0 ? total / windows : 0f;
    }

    static float Luma(Color c) => 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
}
