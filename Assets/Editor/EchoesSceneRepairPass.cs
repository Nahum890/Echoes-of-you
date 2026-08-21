using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// Repara la geometria de los 15 niveles. Nace de una auditoria que encontro
/// que el problema no eran los materiales sino que no estaban asignados:
///
///   92 renderers con material NULL   (dominados por suelos: Floor,
///                                     CorridorFloor, ClassroomFloor...)
///  123 renderers con 'Lit'           (el material gris por defecto de URP,
///                                     dominados por paredes: Wall_L/R/Front)
///
/// Las dos superficies que mas pantalla ocupan estaban sin material, asi que
/// daba igual lo afinados que estuvieran Mat_LinoleumFloor o Mat_WallTeal.
///
/// Ademas corrige los volumenes de niebla de los niveles 1-3, que estaban a
/// ~100 km del origen por un bug de separador decimal.
/// </summary>
public static class EchoesSceneRepairPass
{
    const string SceneRoot = "Assets/Scenes";
    const string MaterialRoot = "Assets/Materials/Echoes";

    // Umbral por encima del cual una coordenada solo puede ser un error: los
    // niveles miden decenas de unidades, no miles.
    const float StrayThreshold = 1000f;

    // Capitulo → material de pared. El mapa nivel→capitulo se reutiliza de
    // EchoesTechnicalArtPass para no tener dos fuentes de verdad.
    static readonly Dictionary<string, string> ChapterWall = new()
    {
        ["I"] = "Mat_WallTeal",        // Persistence
        ["II"] = "Mat_WallMustard",    // Coordination
        ["III"] = "Mat_WallSage",      // Confidence
        ["IV"] = "Mat_WallRose",       // Optimization
        ["V"] = "Mat_Token_wrongness-red", // Consequence
        ["VI"] = "Mat_LiminalPlaster", // Acceptance: el capitulo del vacio blanco
    };

    // El orden importa: se aplica la primera regla que casa, asi que lo
    // especifico va antes que lo generico ("classroomfloor" antes que "floor").
    static readonly (string token, string material)[] NameRules =
    {
        ("locker", "Mat_Arch_Locker"),
        ("taquilla", "Mat_Arch_Locker"),
        ("puerta", "Mat_Door"),
        ("door", "Mat_Door"),
        ("ceiling", "Mat_LiminalCeiling"),
        ("techo", "Mat_LiminalCeiling"),
        ("stair", "Mat_Arch_Stairs"),
        ("escalera", "Mat_Arch_Stairs"),
        ("step", "Mat_Arch_Stairs"),
        ("column", "Mat_Arch_Column"),
        ("columna", "Mat_Arch_Column"),
        ("pillar", "Mat_Arch_Column"),
        ("bench", "Mat_Arch_Seating"),
        ("banco", "Mat_Arch_Seating"),
        ("seat", "Mat_Arch_Seating"),
        ("fence", "Mat_Arch_Metal"),
        ("railing", "Mat_Arch_Metal"),
        ("baranda", "Mat_Arch_Metal"),
        ("resonance", "Mat_Plate"),
        ("plate", "Mat_Plate"),
        ("chalkboard", "Mat_Chalkboard"),
        ("pizarr", "Mat_Chalkboard"),
        ("cork", "Mat_CorkBoard"),
        ("floor", "Mat_LinoleumFloor"),
        ("suelo", "Mat_LinoleumFloor"),
        ("platform", "Mat_LinoleumFloor"),
        ("plataforma", "Mat_LinoleumFloor"),
        ("rooftop", "Mat_LinoleumFloor"),
        ("patio", "Mat_LinoleumFloor"),
        ("wall", null),   // null = material de pared del capitulo
        ("pared", null),
        ("muro", null),
    };

    [MenuItem("Echoes of You/Art/Repair Scene Surfaces (All Levels)", false, 32)]
    public static void RepairAllLevels()
    {
        var log = new StringBuilder();
        int totalMats = 0, totalStray = 0, totalLights = 0, totalBloom = 0;

        for (int level = 1; level <= 15; level++)
        {
            string levelName = "Level_" + level.ToString("00");
            string path = $"{SceneRoot}/{levelName}.unity";
            if (!File.Exists(path))
            {
                continue;
            }

            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            string chapter = EchoesTechnicalArtPass.LevelChapter.TryGetValue(level, out string ch) ? ch : "I";
            string wallMatName = ChapterWall.TryGetValue(chapter, out string w) ? w : "Mat_WallTeal";

            int mats = RepairMaterials(wallMatName, out var detail);
            int stray = RepairStrayObjects();
            int lights = RepairLights();
            int bloom = RepairBloom();

            totalMats += mats;
            totalStray += stray;
            totalLights += lights;
            totalBloom += bloom;

            log.AppendLine($"{levelName} (Ch {chapter}, pared={wallMatName}): " +
                           $"{mats} materiales, {stray} objetos recolocados, {lights} luces, {bloom} bloom");
            foreach (var d in detail.OrderByDescending(kv => kv.Value).Take(6))
            {
                log.AppendLine($"    {d.Key} x{d.Value}");
            }

            if (mats > 0 || stray > 0 || lights > 0 || bloom > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[Scene Repair] {totalMats} materiales asignados, {totalStray} objetos recolocados, " +
                  $"{totalLights} luces corregidas, {totalBloom} volumenes de bloom ajustados.\n" + log);
    }

    /// <summary>Asigna material a los renderers que lo tienen a NULL o con el
    /// material por defecto de URP. NO toca los que ya estan bien asignados.</summary>
    static int RepairMaterials(string wallMatName, out Dictionary<string, int> detail)
    {
        detail = new Dictionary<string, int>();
        int fixedCount = 0;

        foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Material[] mats = r.sharedMaterials;
            bool changed = false;

            for (int i = 0; i < mats.Length; i++)
            {
                bool isMissing = mats[i] == null;
                bool isDefault = mats[i] != null &&
                                 (mats[i].name == "Lit" || mats[i].name == "Default-Material");
                if (!isMissing && !isDefault)
                {
                    continue;
                }

                string matName = ResolveMaterialName(r.name, r.transform.parent != null ? r.transform.parent.name : "", wallMatName);
                Material replacement = LoadMaterial(matName);
                if (replacement == null)
                {
                    continue;
                }

                mats[i] = replacement;
                changed = true;
                fixedCount++;
                detail.TryGetValue(matName, out int n);
                detail[matName] = n + 1;
            }

            if (changed)
            {
                r.sharedMaterials = mats;
                EditorUtility.SetDirty(r);
            }
        }

        return fixedCount;
    }

    static string ResolveMaterialName(string objectName, string parentName, string wallMatName)
    {
        string haystack = (objectName + " " + parentName).ToLowerInvariant();

        foreach (var (token, material) in NameRules)
        {
            if (haystack.Contains(token))
            {
                return material ?? wallMatName;
            }
        }

        // Cualquier otra cosa de arquitectura: yeso generico antes que nada.
        return "Mat_Architecture";
    }

    /// <summary>
    /// Los volumenes de niebla de los niveles 1-3 estaban en coordenadas como
    /// z=100001 cuando debian ser z=100.001: alguien formateo o parseo el numero
    /// en una locale donde el punto es separador de millares. Todas las
    /// coordenadas malas son exactamente x1000.
    /// </summary>
    static int RepairStrayObjects()
    {
        int fixedCount = 0;

        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Vector3 p = t.position;
            if (Mathf.Abs(p.x) <= StrayThreshold && Mathf.Abs(p.y) <= StrayThreshold && Mathf.Abs(p.z) <= StrayThreshold)
            {
                continue;
            }

            Vector3 corrected = new Vector3(
                Mathf.Abs(p.x) > StrayThreshold ? p.x / 1000f : p.x,
                Mathf.Abs(p.y) > StrayThreshold ? p.y / 1000f : p.y,
                Mathf.Abs(p.z) > StrayThreshold ? p.z / 1000f : p.z);

            // Si tras dividir sigue fuera de rango no es el bug de locale sino
            // otra cosa: mejor avisar y no tocarlo.
            if (Mathf.Abs(corrected.x) > StrayThreshold || Mathf.Abs(corrected.y) > StrayThreshold ||
                Mathf.Abs(corrected.z) > StrayThreshold)
            {
                Debug.LogWarning($"[Scene Repair] '{t.name}' esta en {p} y dividir por 1000 no lo arregla; lo dejo como esta.");
                continue;
            }

            Undo.RecordObject(t, "Repair stray");
            t.position = corrected;
            EditorUtility.SetDirty(t);
            fixedCount++;
        }

        return fixedCount;
    }

    /// <summary>Una luz con rango 0 no ilumina nada. La auditoria encontro
    /// varias en los niveles 6-15.</summary>
    static int RepairLights()
    {
        int fixedCount = 0;

        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (l.type != LightType.Point && l.type != LightType.Spot)
            {
                continue;
            }

            if (l.range > 0.01f)
            {
                continue;
            }

            l.range = 8f;
            EditorUtility.SetDirty(l);
            fixedCount++;
        }

        return fixedCount;
    }

    /// <summary>
    /// Las escenas tienen Bloom con intensidad 0.9 y umbral 0.25, que son los
    /// valores de RULE-PST-G01 INVERTIDOS (el spec manda intensidad 0.25,
    /// umbral 0.90). Con umbral 0.25 casi toda la imagen entra en bloom y se
    /// lava, robando contraste justo donde hace falta.
    /// </summary>
    static int RepairBloom()
    {
        int fixedCount = 0;

        foreach (var v in Object.FindObjectsByType<Volume>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (v.profile == null || !v.profile.TryGet(out Bloom bloom))
            {
                continue;
            }

            if (bloom.intensity.value <= 0.5f && bloom.threshold.value >= 0.5f)
            {
                continue; // ya cumple el spec
            }

            bloom.intensity.value = 0.25f;
            bloom.threshold.value = 0.90f;
            EditorUtility.SetDirty(v.profile);
            fixedCount++;
        }

        return fixedCount;
    }

    static Material LoadMaterial(string name)
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialRoot}/{name}.mat");
        if (mat == null)
        {
            Debug.LogWarning($"[Scene Repair] no encuentro {MaterialRoot}/{name}.mat");
        }
        return mat;
    }
}
