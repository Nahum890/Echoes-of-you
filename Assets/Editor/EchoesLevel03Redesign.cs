using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Rediseño de Level_03: techo mas alto y puzzle nuevo.
///
/// QUE HABIA ANTES
/// Un puzzle de bifurcacion: el corredor se abria en dos ramas (AulaLyra a la
/// izquierda, AulaEco a la derecha) y una placa de eco en una rama abria la
/// puerta de la otra. El nivel se resolvia eligiendo por donde ir.
///
/// QUE HAY AHORA — "la galeria vertical"
/// El hall de la estatua pasa a doble altura y gana una pasarela elevada. La
/// salida solo se abre con DOS placas pisadas a la vez: una arriba en la
/// pasarela y otra abajo en el suelo. Ningun jugador puede estar en las dos,
/// asi que hay que grabar un eco que suba y se quede arriba mientras el
/// jugador pisa la de abajo.
///
/// Por que este puzzle y no otro: el capitulo I (Persistence) va de aprender
/// que el eco sostiene lo que tu no puedes. N01 lo ensena con una puerta y N02
/// con dos placas al mismo nivel; llevarlo a la vertical es la escalada
/// natural, y es lo que justifica el techo alto en vez de que sea decoracion.
///
/// QUE SE CONSERVA
/// Las dos ramas, las aulas, sus props narrativos y todo el arte siguen donde
/// estaban: dejan de ser el puzzle y pasan a ser espacio explorable. Este pase
/// NO reconstruye la escena — la edita. Por eso no se pierde la iluminacion ni
/// los materiales reparados.
///
/// Es idempotente: se puede ejecutar varias veces y a la segunda no cambia nada.
/// </summary>
public static class EchoesLevel03Redesign
{
    const string ScenePath = "Assets/Scenes/Level_03.unity";
    const string MaterialRoot = "Assets/Materials/Echoes";

    // Alturas nuevas. Las paredes se modelan como cajas centradas, asi que al
    // cambiar la altura hay que recolocar el centro a la mitad: si no, la pared
    // crece hacia abajo y se hunde en el suelo.
    const float HallWallHeight = 8.5f;      // antes 5.0 — doble altura
    const float CorridorWallHeight = 5.5f;  // antes 3.2 / 3.8
    const float CeilingRise = 3.5f;         // cuanto suben techos y luminarias

    // Geometria de la pasarela, en coordenadas LOCALES de Hall_Estatua.
    const float WalkY = 3.2f;               // altura de la pasarela
    const float PlateHighY = WalkY + 0.32f;
    const string Marker = "L03R_";          // prefijo de todo lo que crea el pase

    [MenuItem("Echoes of You/Level Design/Redesign N03 (galeria vertical)", false, 60)]
    public static void Redesign()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        int raised = RaiseCeilings();
        int fog = FixFogVolumeScales();
        DisableOldPuzzle(out int disabled);
        BuildVerticalGallery(out int created);
        RetargetObjective();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[N03 Redesign] {raised} elementos elevados, {fog} fog volumes corregidos, " +
                  $"{disabled} piezas del puzzle viejo desactivadas, {created} objetos nuevos.");
    }

    // ---------------------------------------------------------------
    // 1. Techo mas alto
    // ---------------------------------------------------------------
    static int RaiseCeilings()
    {
        int n = 0;
        n += RaiseRoom("Hall_Estatua", HallWallHeight);
        n += RaiseRoom("Hall_Salida", HallWallHeight);
        n += RaiseRoom("CorridorCentral", CorridorWallHeight);
        n += RaiseRoom("RamaIzquierda", CorridorWallHeight);
        n += RaiseRoom("RamaDerecha", CorridorWallHeight);
        return n;
    }

    static int RaiseRoom(string roomName, float wallHeight)
    {
        GameObject room = GameObject.Find(roomName);
        if (room == null)
        {
            Debug.LogWarning($"[N03 Redesign] No existe '{roomName}'; se omite.");
            return 0;
        }

        // Idempotencia de toda la sala en un solo sitio: si una pared ya esta a
        // la altura objetivo, el pase ya corrio aqui y no hay que volver a subir
        // techo ni luminarias (que, al moverse en delta, se irian acumulando).
        foreach (Transform probe in room.transform)
        {
            if (!probe.name.StartsWith("Wall_")) continue;
            if (Mathf.Approximately(probe.localScale.y, wallHeight))
            {
                return 0;
            }
            break;
        }

        int touched = 0;
        foreach (Transform c in room.transform)
        {
            string n = c.name;

            // Paredes y columnas: se estiran en Y y se recentran.
            bool isWall = n.StartsWith("Wall_");
            bool isColumn = n.StartsWith("Column");
            if (isWall || isColumn)
            {
                Vector3 s = c.localScale;
                if (Mathf.Approximately(s.y, wallHeight))
                {
                    continue;   // ya elevado: idempotencia
                }
                s.y = wallHeight;
                c.localScale = s;

                Vector3 p = c.localPosition;
                p.y = wallHeight * 0.5f;
                c.localPosition = p;
                EditorUtility.SetDirty(c);
                touched++;
                continue;
            }

            // Techo y luminarias: suben en bloque, sin cambiar de tamano.
            if (n == "Techo" || n.StartsWith("ROW_Light"))
            {
                c.localPosition += new Vector3(0f, CeilingRise, 0f);
                EditorUtility.SetDirty(c);
                touched++;
            }
        }

        return touched;
    }

    // ---------------------------------------------------------------
    // 2. Fog volumes con la escala x1000
    // ---------------------------------------------------------------
    /// <summary>Mismo bug de separador decimal que ya tenian las POSICIONES de
    /// los niveles 1-3, pero en la escala: 160008 en vez de 160.008. El pase de
    /// reparacion corrige posiciones, no escalas, asi que estos sobrevivieron.</summary>
    static int FixFogVolumeScales()
    {
        GameObject root = GameObject.Find("--- FOG VOLUMES ---");
        if (root == null) return 0;

        int fixedCount = 0;
        foreach (Transform c in root.transform)
        {
            Vector3 s = c.localScale;
            bool bad = false;
            if (s.x > 1000f) { s.x /= 1000f; bad = true; }
            if (s.y > 1000f) { s.y /= 1000f; bad = true; }
            if (s.z > 1000f) { s.z /= 1000f; bad = true; }
            if (!bad) continue;

            c.localScale = s;
            EditorUtility.SetDirty(c);
            fixedCount++;
        }
        return fixedCount;
    }

    // ---------------------------------------------------------------
    // 3. Retirar el puzzle de bifurcacion
    // ---------------------------------------------------------------
    /// <summary>No se borra nada: se desactiva. Asi el rediseno es reversible a
    /// mano y no se pierde el arte de las piezas.</summary>
    static void DisableOldPuzzle(out int disabled)
    {
        disabled = 0;
        string[] oldPieces =
        {
            "Cable_PlacaEco_AulaLyra_a_PuertaRamaDerecha",
            "PlacaEco_AulaLyra",
            "PuertaRamaDerecha",
            "Bloqueo_Taquillas",
        };

        foreach (string name in oldPieces)
        {
            GameObject go = GameObject.Find(name);
            if (go == null || !go.activeSelf) continue;
            go.SetActive(false);
            EditorUtility.SetDirty(go);
            disabled++;
        }
    }

    // ---------------------------------------------------------------
    // 4. La galeria vertical
    // ---------------------------------------------------------------
    static void BuildVerticalGallery(out int created)
    {
        created = 0;
        GameObject hall = GameObject.Find("Hall_Estatua");
        if (hall == null)
        {
            Debug.LogError("[N03 Redesign] No existe Hall_Estatua: no se puede montar el puzzle.");
            return;
        }

        Material metal = LoadMat("Mat_Arch_Metal");
        Material stairs = LoadMat("Mat_Arch_Stairs");
        Material plateMat = LoadMat("Mat_Plate");

        // --- pasarela elevada, cruzando el hall a lo ancho ---
        var walkway = EnsureBox(hall.transform, Marker + "Pasarela",
            new Vector3(0f, WalkY, 2.2f), new Vector3(13f, 0.3f, 3.2f), metal, ref created);

        // Barandilla, para que se lea como pasarela y no como losa flotante.
        EnsureBox(hall.transform, Marker + "Barandilla_N",
            new Vector3(0f, WalkY + 0.55f, 3.7f), new Vector3(13f, 0.8f, 0.12f), metal, ref created);
        EnsureBox(hall.transform, Marker + "Barandilla_S",
            new Vector3(0f, WalkY + 0.55f, 0.7f), new Vector3(9.5f, 0.8f, 0.12f), metal, ref created);

        // --- escalera de acceso, pegada a la pared izquierda ---
        // Cuatro peldanos en vez de una rampa rotada: el CharacterController
        // sube escalones sin problema y evita el collider inclinado.
        for (int i = 0; i < 4; i++)
        {
            float h = (WalkY / 4f) * (i + 1);
            EnsureBox(hall.transform, Marker + "Peldano_" + i,
                new Vector3(-5.4f, h * 0.5f, -1.4f + i * 0.9f),
                new Vector3(2.6f, h, 0.9f), stairs, ref created);
        }

        // --- las dos placas ---
        // Arriba: solo el eco. Es la que obliga a grabar.
        var high = EnsurePlate(hall.transform, Marker + "Placa_Alta",
            new Vector3(4.2f, PlateHighY, 2.2f), plateMat, acceptPlayer: true, ref created);

        // Abajo: la que pisa el jugador mientras el eco sostiene la de arriba.
        var low = EnsurePlate(hall.transform, Marker + "Placa_Baja",
            new Vector3(-3.6f, 0.16f, -2.4f), plateMat, acceptPlayer: true, ref created);

        // --- puerta de salida, al fondo del hall ---
        GameObject door = EnsureBox(hall.transform, Marker + "PuertaSalida",
            new Vector3(0f, 1.9f, 4.9f), new Vector3(5.5f, 3.8f, 0.3f), metal, ref created);

        var ctl = door.GetComponent<DoorController>();
        if (ctl == null) ctl = door.AddComponent<DoorController>();

        // AND implicito: DoorController exige TODAS las placas de su array.
        ctl.plates = new[] { high, low };
        ctl.latchOpen = false;   // si el eco se va, la puerta vuelve a cerrarse
        EditorUtility.SetDirty(ctl);
        EditorUtility.SetDirty(door);
    }

    // ---------------------------------------------------------------
    // 5. Texto del objetivo
    // ---------------------------------------------------------------
    static void RetargetObjective()
    {
        foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (mb == null || mb.GetType().Name != "LevelRuntimeController") continue;

            var so = new SerializedObject(mb);
            var obj = so.FindProperty("objectiveText");
            var intro = so.FindProperty("introLine");
            if (obj != null) obj.stringValue = "Dos placas, dos alturas. Tu no llegas a las dos.";
            if (intro != null && string.IsNullOrEmpty(intro.stringValue))
                intro.stringValue = "La galeria se abre hacia arriba. El eco puede quedarse donde tu no.";
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(mb);
        }
    }

    // ---------------------------------------------------------------
    // helpers
    // ---------------------------------------------------------------
    static GameObject EnsureBox(Transform parent, string name, Vector3 localPos, Vector3 localScale,
                                Material mat, ref int created)
    {
        Transform existing = parent.Find(name);
        GameObject go;
        if (existing != null)
        {
            go = existing.gameObject;
        }
        else
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            created++;
        }

        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        go.transform.localRotation = Quaternion.identity;

        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null && mat != null) mr.sharedMaterial = mat;
        EditorUtility.SetDirty(go);
        return go;
    }

    static PressurePlate EnsurePlate(Transform parent, string name, Vector3 localPos,
                                     Material mat, bool acceptPlayer, ref int created)
    {
        GameObject go = EnsureBox(parent, name, localPos, new Vector3(2.4f, 0.16f, 2.4f), mat, ref created);

        var plate = go.GetComponent<PressurePlate>();
        if (plate == null) plate = go.AddComponent<PressurePlate>();
        plate.acceptPlayer = acceptPlayer;
        plate.acceptEcho = true;
        plate.acceptEchoProjection = true;

        // El collider del cubo debe ser trigger para que la placa detecte, pero
        // hace falta ademas una superficie solida: si no, el eco cae a traves.
        var col = go.GetComponent<BoxCollider>();
        if (col != null) col.isTrigger = true;

        Transform baseT = parent.Find(name + "_Base");
        if (baseT == null)
        {
            var baseGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseGo.name = name + "_Base";
            baseGo.transform.SetParent(parent, false);
            created++;
            baseT = baseGo.transform;
        }
        baseT.localPosition = localPos + new Vector3(0f, -0.09f, 0f);
        baseT.localScale = new Vector3(2.5f, 0.1f, 2.5f);
        var baseMr = baseT.GetComponent<MeshRenderer>();
        if (baseMr != null && mat != null) baseMr.sharedMaterial = mat;

        EditorUtility.SetDirty(plate);
        return plate;
    }

    static Material LoadMat(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Material>($"{MaterialRoot}/{name}.mat");
    }
}
