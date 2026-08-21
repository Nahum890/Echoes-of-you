using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Hace que los niveles lean como una escuela y no como un almacen de objetos
/// ambar. Complementa a EchoesSceneRepairPass, que solo toca lo que esta a NULL
/// o con el material por defecto: este pase rescata geometria que SI tiene
/// material asignado, pero el equivocado.
///
/// El hallazgo que lo motiva: Mat_Token_memory-amber era el material MAS usado
/// del bloque jugable (209 asignaciones, presente en los 6 niveles), y 150 de
/// ellas eran mobiliario de aula:
///
///     100 x drawer   (cajoneras)
///      50 x desk     (pupitres)
///
/// Es decir, los pupitres y las cajoneras estaban pintados con un token de
/// color liso, SIN textura y con emision 1.20. El mobiliario que define un aula
/// estaba emitiendo luz.
///
/// Las ~59 restantes son props narrativos (cuadernos, fotos, mochilas, listas
/// de asistencia). Ahi el ambar SI es correcto: es el lenguaje visual de
/// "memoria" del juego. Por eso este pase NO reasigna a ciegas, sino que
/// conserva explicitamente esa familia — ver NarrativeKeep.
///
/// Las tres fases son idempotentes: se puede ejecutar tantas veces como haga
/// falta y a la segunda no cambia nada.
/// </summary>
public static class EchoesSchoolFurniturePass
{
    const string SceneRoot = "Assets/Scenes";
    const string MaterialRoot = "Assets/Materials/Echoes";

    // Solo el bloque jugable. Los niveles 7-15 existen pero estan fuera del
    // build (GameProgress.BlockLevelCount = 6) y nadie los ha verificado nunca,
    // asi que no se tocan.
    const int FirstLevel = 1;
    const int LastLevel = 6;

    /// <summary>Mobiliario: nombre de objeto → material de escuela. El orden
    /// importa, gana la primera regla que casa.</summary>
    static readonly (string token, string material)[] FurnitureRules =
    {
        ("drawer",    "Mat_Arch_Metal"),    // cajonera: chapa pintada
        ("cajon",     "Mat_Arch_Metal"),
        ("desk",      "Mat_Arch_Seating"),  // pupitre: tablero de madera
        ("pupitre",   "Mat_Arch_Seating"),
        ("chair",     "Mat_Arch_Seating"),
        ("silla",     "Mat_Arch_Seating"),
        ("shelf",     "Mat_Arch_Seating"),
        ("estanteria","Mat_Arch_Seating"),
        ("radiador",  "Mat_Arch_Metal"),
        ("radiator",  "Mat_Arch_Metal"),
    };

    /// <summary>Props donde el ambar es intencional y NO se toca. Se comprueba
    /// contra el nombre del objeto y el de su padre, porque las mochilas y los
    /// cuadernos vienen como submallas ("Bag sides", "Top Strip"...).</summary>
    static readonly string[] NarrativeKeep =
    {
        "notebook", "cuaderno", "foto", "photo", "registro", "records",
        "attendance", "mochila", "bag", "backpack", "backal", "coat",
        "stopwatch", "beaker", "strip", "memoria", "memory", "eco", "echo",
    };

    /// <summary>Materiales que quedaron en URP/Lit y deberian usar el shader del
    /// juego. Sin esto no reciben el look PS1 ni — mas visible — la niebla por
    /// capitulo, porque el MixFog vive en los shaders Echoes/*.
    ///
    /// Mat_Glass se queda fuera a proposito: Echoes/PS1World es RenderType
    /// Opaque y volveria opaco el cristal.</summary>
    static readonly string[] ShaderMigration =
    {
        "Mat_Door",
        "Mat_CorkBoard",
        "Mat_PlasterWall",
        "Mat_Plate",   // PS1World tiene _EmissionColor [HDR], asi que conserva
                       // el brillo que da feedback al pisar la placa
    };

    // Tiles por unidad de mundo para KenneyTiling. Primera pasada, pensada para
    // ajustarse mirando el resultado: a resolucion interna baja (672x378)
    // repetir demasiado produce moare en cuanto la sala es grande.
    const float FloorTilesPerUnit = 0.35f;
    const float WallTilesPerUnit = 0.5f;

    // Por debajo de esto no merece la pena reescalar UV: son props, no
    // superficies.
    const float LargeSurfaceMeters = 4f;

    static readonly string[] FloorMaterials = { "Mat_LinoleumFloor", "Mat_Floor", "Mat_Arch_Stairs" };
    static readonly string[] WallMaterials =
    {
        "Mat_WallTeal", "Mat_WallRose", "Mat_WallMustard", "Mat_WallSage",
        "Mat_PlasterWall", "Mat_Architecture",
        "Mat_LiminalPlaster_Teal", "Mat_LiminalPlaster_Void",
        "Mat_LiminalPlaster_Navy", "Mat_LiminalPlaster_Mustard",
        "Mat_LiminalPlaster_Rose", "Mat_LiminalPlaster_Sage",
    };

    [MenuItem("Echoes of You/Art/Fix School Furniture (Block 1-6)", false, 33)]
    public static void FixSchoolFurniture()
    {
        int shaders = MigrateShaders();

        var log = new StringBuilder();
        int totalFurniture = 0, totalKept = 0, totalTiling = 0;

        for (int level = FirstLevel; level <= LastLevel; level++)
        {
            string levelName = "Level_" + level.ToString("00");
            string path = $"{SceneRoot}/{levelName}.unity";
            if (!File.Exists(path))
            {
                continue;
            }

            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            int furniture = ReassignFurniture(out int kept, out var detail);
            int tiling = ApplyKenneyTiling();

            totalFurniture += furniture;
            totalKept += kept;
            totalTiling += tiling;

            log.AppendLine($"{levelName}: {furniture} reasignados, {kept} props narrativos conservados, {tiling} KenneyTiling");
            foreach (var d in detail.OrderByDescending(kv => kv.Value))
            {
                log.AppendLine($"    {d.Key} x{d.Value}");
            }

            if (furniture > 0 || tiling > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[School Furniture] {shaders} materiales migrados de shader, " +
                  $"{totalFurniture} objetos de mobiliario reasignados, " +
                  $"{totalKept} props narrativos conservados en ambar, " +
                  $"{totalTiling} KenneyTiling anadidos.\n" + log);
    }

    /// <summary>Pasa los materiales de URP/Lit a Echoes/PS1World preservando la
    /// textura. Es necesario hacerlo a mano porque el nombre de la propiedad
    /// cambia: URP/Lit guarda la albedo en _BaseMap y PS1World en _BaseTex, asi
    /// que un cambio de shader a secas la perderia.</summary>
    static int MigrateShaders()
    {
        Shader target = Shader.Find("Echoes/PS1World");
        if (target == null)
        {
            Debug.LogWarning("[School Furniture] No se encuentra el shader Echoes/PS1World. Fase de shaders omitida.");
            return 0;
        }

        int migrated = 0;
        foreach (string name in ShaderMigration)
        {
            Material mat = LoadMaterial(name);
            if (mat == null || mat.shader == target)
            {
                continue;   // idempotente: a la segunda pasada ya esta migrado
            }

            Texture albedo = null;
            foreach (string prop in new[] { "_BaseMap", "_MainTex", "_BaseTex" })
            {
                if (mat.HasProperty(prop) && mat.GetTexture(prop) != null)
                {
                    albedo = mat.GetTexture(prop);
                    break;
                }
            }
            Color baseColor = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : Color.white;
            Color emission = mat.HasProperty("_EmissionColor") ? mat.GetColor("_EmissionColor") : Color.black;

            mat.shader = target;

            if (albedo != null && mat.HasProperty("_BaseTex"))
            {
                mat.SetTexture("_BaseTex", albedo);
            }
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", baseColor);
            }
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", emission);
            }

            EditorUtility.SetDirty(mat);
            migrated++;
        }

        return migrated;
    }

    /// <summary>Rescata el mobiliario del token ambar. Solo actua sobre
    /// materiales que sabemos que estan mal (el token ambar y el Lit por
    /// defecto): si algo ya tiene una superficie de escuela asignada, se
    /// respeta.</summary>
    static int ReassignFurniture(out int kept, out Dictionary<string, int> detail)
    {
        detail = new Dictionary<string, int>();
        kept = 0;
        int changed = 0;

        foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            string ownName = r.gameObject.name.ToLowerInvariant();
            string parentName = r.transform.parent != null
                ? r.transform.parent.name.ToLowerInvariant()
                : string.Empty;

            string rule = MatchFurnitureRule(ownName);
            if (rule == null)
            {
                continue;
            }

            // El ambar narrativo manda sobre la regla de mobiliario.
            if (IsNarrative(ownName) || IsNarrative(parentName))
            {
                kept++;
                continue;
            }

            Material[] mats = r.sharedMaterials;
            bool touched = false;

            for (int i = 0; i < mats.Length; i++)
            {
                if (!ShouldReplace(mats[i]))
                {
                    continue;
                }

                Material replacement = LoadMaterial(rule);
                if (replacement == null)
                {
                    continue;
                }

                mats[i] = replacement;
                touched = true;
                changed++;
                detail.TryGetValue(rule, out int n);
                detail[rule] = n + 1;
            }

            if (touched)
            {
                r.sharedMaterials = mats;
                EditorUtility.SetDirty(r);
            }
        }

        return changed;
    }

    /// <summary>Anade KenneyTiling a suelos y paredes grandes, que es donde el
    /// tiling por cara se nota estirado. En props pequenos no aporta nada.</summary>
    static int ApplyKenneyTiling()
    {
        int added = 0;

        foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (r.GetComponent<KenneyTiling>() != null)
            {
                continue;   // idempotente
            }

            Material mat = r.sharedMaterial;
            if (mat == null)
            {
                continue;
            }

            bool isFloor = FloorMaterials.Contains(mat.name);
            bool isWall = WallMaterials.Contains(mat.name);
            if (!isFloor && !isWall)
            {
                continue;
            }

            Vector3 size = r.bounds.size;
            if (Mathf.Max(size.x, Mathf.Max(size.y, size.z)) < LargeSurfaceMeters)
            {
                continue;
            }

            var tiling = r.gameObject.AddComponent<KenneyTiling>();
            tiling.tilesPerUnit = isFloor ? FloorTilesPerUnit : WallTilesPerUnit;
            tiling.UpdateTiling();
            EditorUtility.SetDirty(r.gameObject);
            added++;
        }

        return added;
    }

    static string MatchFurnitureRule(string objectName)
    {
        foreach (var (token, material) in FurnitureRules)
        {
            if (objectName.Contains(token))
            {
                return material;
            }
        }
        return null;
    }

    static bool IsNarrative(string name)
    {
        return !string.IsNullOrEmpty(name) && NarrativeKeep.Any(name.Contains);
    }

    /// <summary>Solo se reemplaza lo que sabemos que esta mal, nunca una
    /// superficie de escuela ya asignada.</summary>
    static bool ShouldReplace(Material mat)
    {
        if (mat == null)
        {
            return true;
        }
        return mat.name == "Mat_Token_memory-amber"
            || mat.name == "Lit"
            || mat.name == "Universal Render Pipeline/Lit"
            || mat.name == "Default-Material";
    }

    static Material LoadMaterial(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Material>($"{MaterialRoot}/{name}.mat");
    }
}
