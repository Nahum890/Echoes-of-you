using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Arregla los puzzles de N05 y N06, que no se podian terminar, y da mas
/// margen de tiempo en todo el bloque.
///
/// EL DIAGNOSTICO
/// LevelExit se niega a activarse mientras el objetivo no este listo
/// (if (!_isUnlocked) return). Cada LevelGoal recoge un unico GoalTrigger hijo
/// — Memoria1_Goal — y ese trigger solo se satisface por la fuente que tenga
/// asignada. En los dos niveles esa fuente estaba desconectada:
///
///   N05  Memoria1_Goal escucha la señal Signal_Shield, que es correcto. Pero
///        quien deberia marcarla, Hazard_Curtain [EchoShieldField], tenia
///        completionSignal = NULL. Nadie llamaba nunca a MarkSatisfied().
///        Ademas Boost_Float1 [EchoKineticZone] tiene rol MomentumRelay con
///        momentumRelayTarget = NULL, asi que el impulso que el propio
///        objetivo del nivel promete tampoco se aplicaba.
///
///   N06  Memoria1_Goal tenia usePlatePressedState = true y
///        pressurePlate = NULL: esperaba una placa que no existe en la escena.
///        El blueprint define una (PlacaEco_Puente) que nunca se construyo.
///
/// LA SOLUCION EN N06
/// En vez de recrear la placa a mitad del abismo, se pone en la zona de
/// llegada. Asi el objetivo se cumple al ALCANZAR el otro lado, que es
/// literalmente lo que el nivel pide ("el puente existe solo mientras el eco
/// lo sostiene"), y se apoya en el mecanismo que ya funciona. El TemporalBridge
/// no se toca: ya responde bien al tag Echo.
///
/// MAS TIEMPO
/// EchoRecorder venia a 12 segundos de grabacion. Es muy justo para grabar,
/// volver y cruzar antes de que el eco termine su bucle. Sube a 22 en todo el
/// bloque.
///
/// Idempotente: valores absolutos y objetos con nombre fijo.
/// </summary>
public static class EchoesPuzzleFixPass
{
    const string SceneRoot = "Assets/Scenes";
    const string MaterialRoot = "Assets/Materials/Echoes";

    /// <summary>12 s daba para grabar o para cruzar, pero no para las dos cosas.</summary>
    const float RecordSeconds = 22f;

    [MenuItem("Echoes of You/Level Design/Fix Puzzles N05 + N06", false, 61)]
    public static void FixPuzzles()
    {
        var log = new StringBuilder();

        int recorders = 0;
        for (int level = 1; level <= 6; level++)
        {
            string path = $"{SceneRoot}/Level_{level:00}.unity";
            if (!File.Exists(path)) continue;

            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            int r = RaiseRecordTime();
            recorders += r;

            bool changed = r > 0;
            if (level == 5) changed |= FixLevel05(log);
            if (level == 6) changed |= FixLevel06(log);

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[Puzzle Fix] {recorders} grabadores a {RecordSeconds}s.\n" + log);
    }

    // ---------------------------------------------------------------
    // Tiempo de grabacion
    // ---------------------------------------------------------------
    static int RaiseRecordTime()
    {
        int n = 0;
        foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (mb == null || mb.GetType().Name != "EchoRecorder") continue;

            var so = new SerializedObject(mb);
            var p = so.FindProperty("maxRecordSeconds");
            if (p == null || Mathf.Approximately(p.floatValue, RecordSeconds)) continue;

            p.floatValue = RecordSeconds;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(mb);
            n++;
        }
        return n;
    }

    // ---------------------------------------------------------------
    // N05
    // ---------------------------------------------------------------
    static bool FixLevel05(StringBuilder log)
    {
        bool changed = false;

        MonoBehaviour signal = FindByTypeName("PuzzleSignal", "Signal_Shield");
        MonoBehaviour shield = FindByTypeName("EchoShieldField", null);
        MonoBehaviour kinetic = FindByTypeName("EchoKineticZone", null);

        // 1. la cortina marca la señal al ser escudada por el eco
        if (shield != null && signal != null)
        {
            var so = new SerializedObject(shield);
            var p = so.FindProperty("completionSignal");
            if (p != null && p.objectReferenceValue == null)
            {
                p.objectReferenceValue = signal;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(shield);
                changed = true;
                log.AppendLine("N05: Hazard_Curtain -> completionSignal = " + signal.gameObject.name);
            }
        }
        else
        {
            log.AppendLine("N05: ⚠ no se encontro EchoShieldField o Signal_Shield.");
        }

        // 2. el impulso necesita un destino hacia el que empujar
        if (kinetic != null)
        {
            var so = new SerializedObject(kinetic);
            var p = so.FindProperty("momentumRelayTarget");
            if (p != null && p.objectReferenceValue == null)
            {
                GameObject target = GameObject.Find("ExitPlatform") ?? GameObject.Find("Float_2");
                if (target != null)
                {
                    p.objectReferenceValue = target.transform;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(kinetic);
                    changed = true;
                    log.AppendLine("N05: Boost_Float1 -> momentumRelayTarget = " + target.name);
                }
                else
                {
                    log.AppendLine("N05: ⚠ no hay ExitPlatform ni Float_2 al que empujar.");
                }
            }
        }

        return changed;
    }

    // ---------------------------------------------------------------
    // N06
    // ---------------------------------------------------------------
    static bool FixLevel06(StringBuilder log)
    {
        MonoBehaviour goalTrigger = FindByTypeName("GoalTrigger", "Memoria1_Goal");
        if (goalTrigger == null)
        {
            log.AppendLine("N06: ⚠ no se encontro Memoria1_Goal.");
            return false;
        }

        // La placa va en la zona de llegada: el objetivo se cumple al alcanzar
        // el otro lado del abismo.
        GameObject anchor = GameObject.Find("ZonaD_Llegada");
        Vector3 pos = anchor != null
            ? anchor.transform.position + new Vector3(0f, 0.12f, -1.5f)
            : new Vector3(0f, 0.12f, 26.5f);

        GameObject plateGo = GameObject.Find("PlacaEco_Puente");
        bool created = false;
        if (plateGo == null)
        {
            plateGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plateGo.name = "PlacaEco_Puente";
            created = true;
        }
        plateGo.transform.position = pos;
        plateGo.transform.localScale = new Vector3(3f, 0.18f, 3f);

        var mat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialRoot}/Mat_Plate.mat");
        var mr = plateGo.GetComponent<MeshRenderer>();
        if (mr != null && mat != null) mr.sharedMaterial = mat;

        // Trigger para que detecte, y una base solida debajo para no caer.
        var col = plateGo.GetComponent<BoxCollider>();
        if (col != null) col.isTrigger = true;

        GameObject baseGo = GameObject.Find("PlacaEco_Puente_Base");
        if (baseGo == null)
        {
            baseGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseGo.name = "PlacaEco_Puente_Base";
        }
        baseGo.transform.position = pos + new Vector3(0f, -0.11f, 0f);
        baseGo.transform.localScale = new Vector3(3.1f, 0.12f, 3.1f);
        var bmr = baseGo.GetComponent<MeshRenderer>();
        if (bmr != null && mat != null) bmr.sharedMaterial = mat;

        var plate = plateGo.GetComponent<PressurePlate>();
        if (plate == null) plate = plateGo.AddComponent<PressurePlate>();
        plate.acceptPlayer = true;
        plate.acceptEcho = true;
        plate.acceptEchoProjection = true;
        EditorUtility.SetDirty(plate);
        EditorUtility.SetDirty(plateGo);

        // enlazar el objetivo a la placa
        var so2 = new SerializedObject(goalTrigger);
        var pPlate = so2.FindProperty("pressurePlate");
        var pUse = so2.FindProperty("usePlatePressedState");
        bool linked = false;
        if (pPlate != null && pPlate.objectReferenceValue != plate)
        {
            pPlate.objectReferenceValue = plate;
            linked = true;
        }
        if (pUse != null && !pUse.boolValue) { pUse.boolValue = true; linked = true; }
        if (linked)
        {
            so2.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(goalTrigger);
        }

        log.AppendLine($"N06: PlacaEco_Puente {(created ? "creada" : "reubicada")} en {pos.ToString("F1")}, enlazada al objetivo.");
        return true;
    }

    // ---------------------------------------------------------------
    static MonoBehaviour FindByTypeName(string typeName, string objectName)
    {
        foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (mb == null || mb.GetType().Name != typeName) continue;
            if (objectName != null && mb.gameObject.name != objectName) continue;
            return mb;
        }
        return null;
    }
}
