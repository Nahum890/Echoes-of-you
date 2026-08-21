using UnityEditor;
using UnityEngine;

/// <summary>
/// Lanza en orden los pases que dejan el bloque jugable (N01-N06) con aspecto
/// de escuela y con sus props narrativos. Existe para poder ejecutarlo de una
/// sola pasada en batch mode, porque la primera importacion tras un cambio
/// grande tarda bastante y encadenar cuatro invocaciones de Unity multiplica
/// esa espera.
///
/// El orden NO es arbitrario:
///   1. Superficies — repone texturas y tilings en los .mat. Va primero porque
///      los demas pases asignan esos materiales y conviene que ya esten bien.
///   2. Reparacion  — asigna material a lo que esta a NULL o con el Lit por
///      defecto de URP.
///   3. Mobiliario  — rescata del token ambar lo que SI tenia material pero el
///      equivocado (pupitres, cajoneras), migra shaders y anade KenneyTiling.
///   4. Props       — instala el catalogo contextual en las escenas.
///
/// Todos son idempotentes: repetir la tanda no cambia nada la segunda vez.
/// </summary>
public static class EchoesBlockArtRunner
{
    [MenuItem("Echoes of You/Art/Run Block Art + Props (1-6)", false, 34)]
    public static void RunAll()
    {
        Debug.Log("[Block Art Runner] === 1/4 Apply School Surfaces ===");
        EchoesSchoolSurfacePass.ApplySchoolSurfaces();

        Debug.Log("[Block Art Runner] === 2/4 Repair Scene Surfaces ===");
        EchoesSceneRepairPass.RepairAllLevels();

        Debug.Log("[Block Art Runner] === 3/4 Fix School Furniture ===");
        EchoesSchoolFurniturePass.FixSchoolFurniture();

        // InstallAllCore y no InstallAll: el segundo termina en un
        // EditorUtility.DisplayDialog, y en batch mode no hay quien lo cierre.
        Debug.Log("[Block Art Runner] === 4/4 Install Contextual Props ===");
        ContextPropsSceneInstaller.InstallAllCore(out int installed, out int skipped);
        Debug.Log($"[Block Art Runner] Props instalados: {installed}, omitidos por ancla ausente: {skipped}");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Block Art Runner] === TANDA COMPLETA ===");
    }
}
