using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Pase idempotente que lleva un nivel a lo que exigen las specs de iluminacion,
/// escala e identidad escolar, y que valida las reglas que se pueden comprobar
/// sin ejecutar el juego.
///
/// Especificaciones aplicadas:
///   SPEC-105  LIGHTING_GRAMMAR      ambiente Flat 0.15, sombras duras, tope 48 luces
///   SPEC-143  GLOBAL_LIGHTING_SPEC  sol canonico, niebla por capitulo, point/spot
///                                   en Realtime con sombras None
///   SPEC-002  ANTI_PATTERNS         intensidad <= 5 lux, rango <= 25 m,
///                                   placas a >= 1.5 m de pared, cable visible
///   SPEC-106  SCALE_GUIDE           holgura de placa 0.5 m
///
/// Ojo con la escala: LevelEnvironmentBootstrap multiplica x2 la posicion XZ y la
/// escala completa de los hijos directos de los roots de nivel en runtime. Todo lo
/// que este pase crea bajo esos roots se autoria en metros de EDITOR, que son la
/// mitad de los metros de juego.
/// </summary>
public static class EchoesLevelRedesignPass
{
    // ---- Capitulo I (niveles 1-3), de lighting_profiles.yaml ----
    static readonly Color FogChapterI = new Color(0.109f, 0.141f, 0.188f, 1f);   // #1C2430
    static readonly Color AmbientChapterI = new Color(0.059f, 0.078f, 0.102f, 1f); // #0F141A
    const float FogDensityChapterI = 0.008f;

    // ---- Sol canonico (RULE-LGT-003 / RULE-LGT-G02) ----
    static readonly Color SunColor = new Color(0.949f, 0.949f, 1f, 1f);           // #F2F2FF
    static readonly Vector3 SunRotation = new Vector3(50f, -30f, 0f);
    const float SunIntensity = 0.85f;

    // ---- Topes de ANTI_PATTERNS ----
    const int MaxLightsPerScene = 48;     // RULE-LGT-002
    const float MaxLightIntensity = 5f;   // RULE-ANTI-008
    const float MaxLightRange = 25f;      // RULE-ANTI-008
    const float MinWallClearance = 1.5f;  // RULE-ANTI-002

    // ---- Paleta de la jerarquia de iluminacion ----
    static readonly Color KeyColor = new Color(0.839f, 0.902f, 1f);        // #D6E6FF fluorescente frio
    static readonly Color FillColor = new Color(0.62f, 0.66f, 0.72f);      // rebote neutro
    static readonly Color AccentColor = new Color(1f, 0.749f, 0f);         // #FFBF00 ambar: donde se resuelve
    static readonly Color TransitionColor = new Color(1f, 0.878f, 0.6f);   // #FFE099 tenue: por donde se va

    const string LightingRoot = "--- LIGHTING ---";
    const string CableRoot = "--- CABLES ---";

    [MenuItem("Echoes of You/Design/Rediseñar N02 y N03 (spec)", false, 300)]
    public static void RedesignN02N03()
    {
        var log = new StringBuilder();
        foreach (int level in new[] { 2, 3 })
            ApplyToLevel(level, log);
        Debug.Log("[Rediseño N02-N03]\n" + log);
    }

    [MenuItem("Echoes of You/Design/Preparar bloque para presentar (N01-N06)", false, 299)]
    public static void PrepareBlockForShowcase()
    {
        var log = new StringBuilder();
        for (int level = 1; level <= 6; level++)
            ApplyToLevel(level, log);
        Debug.Log("[Bloque listo]" + System.Environment.NewLine + log);
    }

    [MenuItem("Echoes of You/Design/Botones: respuesta inmediata (N01-N06)", false, 303)]
    public static void TightenAllPlates()
    {
        var log = new StringBuilder();
        for (int level = 1; level <= 6; level++)
        {
            string path = ScenePath(level);
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) continue;
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            log.AppendLine("======== " + scene.name + " ========");
            TightenPlates(scene, log);
            EnforceDoorTiming(scene, log);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        Debug.Log("[Botones inmediatos]" + System.Environment.NewLine + log);
    }

    /// <summary>
    /// Deja todas las placas con respuesta inmediata y aceptando al jugador.
    ///
    /// Dos cosas hacian que pareciera que se quedaban encendidas:
    ///  - autoReleaseTimer > 0 mantiene la placa pisada despues de bajarte.
    ///  - la caja de deteccion era mas grande que la placa visible, asi que
    ///    salias de ella y seguias dentro de la zona.
    /// </summary>
    static void TightenPlates(Scene scene, StringBuilder log)
    {
        int n = 0, temporizadores = 0, aceptan = 0, soloEco = 0;

        foreach (PressurePlate pp in Object.FindObjectsByType<PressurePlate>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            n++;
            var so = new SerializedObject(pp);

            SerializedProperty timer = so.FindProperty("autoReleaseTimer");
            if (timer != null && timer.floatValue > 0f)
            {
                log.AppendLine("  " + pp.gameObject.name + ": autoReleaseTimer "
                    + timer.floatValue.ToString("0.00") + " -> 0 (se apagaba tarde)");
                timer.floatValue = 0f;
                temporizadores++;
            }

            if (!so.FindProperty("acceptPlayer").boolValue) aceptan++;
            so.FindProperty("acceptPlayer").boolValue = true;
            so.FindProperty("acceptEcho").boolValue = true;
            so.FindProperty("acceptEchoProjection").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();

            var comp = pp.GetComponent<PressurePlateEchoOnly>();
            if (comp != null) { Object.DestroyImmediate(comp); soloEco++; }
            Transform barrera = pp.transform.Find("EchoOnly_PlayerBarrier");
            if (barrera != null) Object.DestroyImmediate(barrera.gameObject);
        }

        log.AppendLine("  placas=" + n + " | temporizadores a 0: " + temporizadores
            + " | pasan a aceptar al jugador: " + aceptan + " | componentes solo-Eco retirados: " + soloEco);
    }

    [MenuItem("Echoes of You/Design/Puertas sin enganche (N01-N06)", false, 302)]
    public static void UnlatchAllDoors()
    {
        var log = new StringBuilder();
        for (int level = 1; level <= 6; level++)
        {
            string path = ScenePath(level);
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) continue;
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            log.AppendLine("======== " + scene.name + " ========");
            if (EnforceDoorTiming(scene, log))
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }
        Debug.Log("[Puertas sin enganche]" + System.Environment.NewLine + log);
    }

    /// <summary>
    /// Una puerta con latchOpen se queda abierta para siempre en cuanto se cumple
    /// la condicion una sola vez. Eso anula la mecanica: la gracia es que la placa
    /// se suelta al bajarte y la puerta se cierra, asi que hay que cruzar MIENTRAS
    /// el Eco la sostiene. PressurePlate ya libera al instante (autoReleaseTimer=0);
    /// el enganche estaba en la puerta.
    /// </summary>
    static bool EnforceDoorTiming(Scene scene, StringBuilder log)
    {
        bool cambiada = false;

        foreach (DoorController d in Object.FindObjectsByType<DoorController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (d.latchOpen)
            {
                d.latchOpen = false;
                EditorUtility.SetDirty(d);
                cambiada = true;
                log.AppendLine("  " + d.gameObject.name + ": latchOpen SI -> no (ahora se cierra al soltar la placa)");
            }

            int placas = d.plates == null ? 0 : d.plates.Length;
            int validas = 0;
            if (d.plates != null) foreach (PressurePlate pp in d.plates) if (pp != null) validas++;
            if (validas == 0)
                log.AppendLine("  AVISO " + d.gameObject.name + ": " + placas + " placas asignadas, " + validas
                    + " validas -> DoorController la deja CERRADA para siempre, el nivel no se puede terminar");
        }

        // Lo mismo en las conexiones de PuzzleWire.
        foreach (PuzzleWire w in Object.FindObjectsByType<PuzzleWire>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (w.connections == null) continue;
            foreach (var c in w.connections)
            {
                if (!c.latchOpen) continue;
                c.latchOpen = false;
                EditorUtility.SetDirty(w);
                cambiada = true;
                log.AppendLine("  PuzzleWire -> " + (c.door == null ? "?" : c.door.gameObject.name) + ": latchOpen SI -> no");
            }
        }

        if (!cambiada) log.AppendLine("  ninguna puerta con enganche");
        return cambiada;
    }

    [MenuItem("Echoes of You/Design/Validar reglas de spec (N01-N06)", false, 301)]
    public static void ValidateBlock()
    {
        var log = new StringBuilder();
        for (int level = 1; level <= 6; level++)
        {
            string path = ScenePath(level);
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) continue;
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            log.AppendLine("======== " + scene.name + " ========");
            Validate(scene, log);
        }
        Debug.Log("[Validacion de spec]\n" + log);
    }

    static string ScenePath(int level) => "Assets/Scenes/Level_" + level.ToString("00") + ".unity";

    static void ApplyToLevel(int level, StringBuilder log)
    {
        string path = ScenePath(level);
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
        {
            log.AppendLine("Escena no encontrada: " + path);
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        if (scene.name != "Level_" + level.ToString("00"))
        {
            log.AppendLine("ABORTADO: se abrio " + scene.name + " en vez de Level_" + level.ToString("00"));
            return;
        }

        log.AppendLine("======== " + scene.name + " ========");

        ApplyCanonicalLighting(scene, log);
        SplitLargeFloors(scene, log);       // sin esto, 8 luces para 1600 m2 de suelo
        FixOversizedProps(scene, log);
        RepaintFurniture(scene, log);       // el ambar vuelve a significar algo
        if (level == 2) LayOutTwinPlates(scene, log);   // LEVEL_SPEC_02: las dos placas a la vista
        RaiseRooms(scene, log);             // antes del techo: sube los muros
        BuildCeilings(scene, log);          // antes de la luz: las luminarias cuelgan del techo
        TuneCameraForInteriors(scene, log); // despues del techo: la camara tiene que caber debajo
        FixPlateClearance(scene, log);      // antes de la luz: las ACCENT siguen a las placas
        BuildLightingHierarchy(scene, log);
        TightenPlates(scene, log);       // se apaga al bajarte
        EnforceDoorTiming(scene, log);   // el timing es la mecanica
        BuildPuzzleCables(scene, log);
        EnforceLightBudget(scene, log);
        Validate(scene, log);

        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene);
        log.AppendLine("  guardada = " + saved);
        log.AppendLine();
    }

    // ------------------------------------------------------------------
    // 1. Iluminacion canonica
    // ------------------------------------------------------------------
    static void ApplyCanonicalLighting(Scene scene, StringBuilder log)
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = AmbientChapterI;
        RenderSettings.ambientIntensity = 0.15f;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = FogChapterI;
        RenderSettings.fogDensity = FogDensityChapterI;

        int sol = 0, convertidas = 0, recortadas = 0;
        foreach (Light l in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (l.type == LightType.Directional)
            {
                l.intensity = SunIntensity;
                l.color = SunColor;
                l.transform.rotation = Quaternion.Euler(SunRotation);
                l.shadows = LightShadows.Hard;                 // CONS-LGT-003: nunca Soft
                l.lightmapBakeType = LightmapBakeType.Mixed;
                sol++;
                continue;
            }

            // RULE-LGT-G05: sin lightmaps horneados, una luz Baked no ilumina nada.
            // El proyecto no tiene lightmaps, asi que todas pasan a Realtime.
            if (l.lightmapBakeType != LightmapBakeType.Realtime)
            {
                l.lightmapBakeType = LightmapBakeType.Realtime;
                convertidas++;
            }

            if (l.shadows != LightShadows.None)
                l.shadows = LightShadows.None;                 // look PS1 plano

            if (l.intensity > MaxLightIntensity) { l.intensity = MaxLightIntensity; recortadas++; }
            if (l.range > MaxLightRange) { l.range = MaxLightRange; recortadas++; }
        }

        log.AppendLine("  iluminacion canonica: soles=" + sol
            + "  Baked/Mixed -> Realtime=" + convertidas
            + "  valores recortados=" + recortadas);
        log.AppendLine("  ambiente Flat #0F141A 0.15 | niebla ExpSq #1C2430 d=0.008");
    }

    // ------------------------------------------------------------------
    // 1.-2 Las dos placas de N02, a la vista
    // ------------------------------------------------------------------
    /// <summary>
    /// LEVEL_SPEC_02 coloca las dos placas en [-3,0,25] y [3,0,25]: separadas 6 m,
    /// en la misma sala, visibles a la vez. La escena las tenia repartidas: la del
    /// jugador en mitad del pasillo (se pisa sola al pasar, parece averiada) y la
    /// del Eco 24 m a la izquierda dentro de un aula lateral, sin linea de vision,
    /// asi que el jugador no llegaba a saber que existia y el puzzle se leia
    /// incompleto.
    ///
    /// Se recoloca la del Eco al lado de la del jugador y ambas se apartan de la
    /// linea de marcha, para que se lean como un par y no se pisen sin querer.
    /// </summary>
    static void LayOutTwinPlates(Scene scene, StringBuilder log)
    {
        DoorController puerta = Object.FindAnyObjectByType<DoorController>();
        if (puerta == null) { log.AppendLine("  placas gemelas: no hay puerta de referencia"); return; }

        PressurePlate jugador = null, eco = null;
        foreach (PressurePlate pp in Object.FindObjectsByType<PressurePlate>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var so = new SerializedObject(pp);
            bool aceptaJugador = so.FindProperty("acceptPlayer").boolValue;
            if (aceptaJugador && jugador == null) jugador = pp;
            else if (!aceptaJugador && eco == null) eco = pp;
        }
        if (jugador == null || eco == null) { log.AppendLine("  placas gemelas: no encontre la pareja"); return; }

        // El ancla es la PUERTA, no la posicion actual de las placas: si se toma
        // la placa como referencia, cada pase la desplaza otra vez y el par se va
        // arrastrando por el pasillo.
        Vector3 pd = puerta.transform.position;
        const float Separacion = 3f;    // 3 m de editor = 6 m de juego (LEVEL_SPEC_02)
        const float DistanciaAntesDeLaPuerta = 11f;

        Vector3 centro = new Vector3(pd.x, pd.y, pd.z - DistanciaAntesDeLaPuerta);
        Vector3 posJugador = SobreElSuelo(new Vector3(centro.x - Separacion * 0.5f, centro.y, centro.z), jugador.transform.position);
        Vector3 posEco = SobreElSuelo(new Vector3(centro.x + Separacion * 0.5f, centro.y, centro.z), eco.transform.position);

        Vector3 antesJ = jugador.transform.position;
        Vector3 antesE = eco.transform.position;
        jugador.transform.position = posJugador;
        eco.transform.position = posEco;

        // Ambas placas aceptan jugador Y Eco. El puzzle sigue exigiendo dos
        // actores — nadie puede estar en dos sitios a la vez — pero deja de
        // castigar al jugador por pisar "la que no era", que solo confundia.
        foreach (PressurePlate pp in new[] { jugador, eco })
        {
            var so = new SerializedObject(pp);
            so.FindProperty("acceptPlayer").boolValue = true;
            so.FindProperty("acceptEcho").boolValue = true;
            so.FindProperty("acceptEchoProjection").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();

            // La barrera fisica de las placas solo-Eco ya no pinta nada aqui.
            var soloEco = pp.GetComponent<PressurePlateEchoOnly>();
            if (soloEco != null) Object.DestroyImmediate(soloEco);
            Transform barrera = pp.transform.Find("EchoOnly_PlayerBarrier");
            if (barrera != null) Object.DestroyImmediate(barrera.gameObject);
        }
        Physics.SyncTransforms();

        log.AppendLine("  placas gemelas (LEVEL_SPEC_02, ancladas a la puerta):");
        log.AppendLine("     jugador " + antesJ.ToString("0.0") + " -> " + posJugador.ToString("0.0"));
        log.AppendLine("     eco     " + antesE.ToString("0.0") + " -> " + posEco.ToString("0.0"));
        log.AppendLine("     separacion " + (Vector3.Distance(posJugador, posEco) * 2f).ToString("0.0")
            + " m de juego, a " + (DistanciaAntesDeLaPuerta * 2f).ToString("0") + " m de la puerta");
    }

    static Vector3 SobreElSuelo(Vector3 deseada, Vector3 respaldo)
    {
        RaycastHit h;
        if (Physics.Raycast(new Vector3(deseada.x, deseada.y + 5f, deseada.z), Vector3.down, out h, 20f, ~0, QueryTriggerInteraction.Ignore))
            return new Vector3(deseada.x, h.point.y + 0.05f, deseada.z);
        return respaldo;
    }

    // ------------------------------------------------------------------
    // 1.-1 Techos mas altos
    // ------------------------------------------------------------------
    /// <summary>
    /// Sube los muros de cada sala para que el techo quede mas alto, manteniendo
    /// el suelo donde esta. BuildCeilings mide los bounds despues, asi que la losa
    /// sigue a los muros sin tocar nada mas.
    /// </summary>
    static void RaiseRooms(Scene scene, StringBuilder log)
    {
        // Altura OBJETIVO en metros de juego, no un incremento: asi el pase se
        // puede reejecutar sin que las salas crezcan sin parar.
        const float AlturaPasillo = 14f;
        const float AlturaSalaGrande = 20f;

        GameObject env = FindRoot(scene, "--- ENVIRONMENT ---");
        if (env == null) { log.AppendLine("  altura: sin root ENVIRONMENT"); return; }

        int muros = 0, salas = 0;
        float masBaja = float.MaxValue, masAlta = 0f;

        foreach (Transform sala in env.transform)
        {
            // Tamaño de la sala para decidir cuanto techo merece.
            Bounds b = new Bounds();
            bool tiene = false;
            foreach (Collider c in sala.GetComponentsInChildren<Collider>())
            {
                if (c.isTrigger) continue;
                Bounds cb = c.bounds;
                if (cb.size.magnitude > 300f) continue;
                if (!tiene) { b = cb; tiene = true; } else b.Encapsulate(cb);
            }
            if (!tiene) continue;

            float anchoJuego = b.size.x * 2f;
            float fondoJuego = b.size.z * 2f;
            if (anchoJuego < 4f || fondoJuego < 4f) continue;

            float objetivoJuego = Mathf.Min(anchoJuego, fondoJuego) >= 14f ? AlturaSalaGrande : AlturaPasillo;

            float escalaSala = Mathf.Abs(sala.lossyScale.y);
            if (escalaSala < 0.001f) continue;
            // metros de juego -> unidades locales del muro
            float objetivoLocal = objetivoJuego / (escalaSala * 2f);

            bool tocada = false;
            foreach (Transform hijo in sala.GetComponentsInChildren<Transform>())
            {
                string n = hijo.name.ToLower();
                if (!n.Contains("wall") && !n.Contains("muro") && !n.Contains("pared")) continue;
                if (hijo.GetComponent<Renderer>() == null && hijo.GetComponent<Collider>() == null) continue;

                Vector3 sc = hijo.localScale;
                if (sc.y < 0.5f) continue;   // no es un muro vertical

                float baseY = hijo.localPosition.y - sc.y * 0.5f;   // el suelo no se toca
                hijo.localScale = new Vector3(sc.x, objetivoLocal, sc.z);
                Vector3 pos = hijo.localPosition;
                hijo.localPosition = new Vector3(pos.x, baseY + objetivoLocal * 0.5f, pos.z);
                muros++;
                tocada = true;
            }
            if (tocada)
            {
                salas++;
                masBaja = Mathf.Min(masBaja, objetivoJuego);
                masAlta = Mathf.Max(masAlta, objetivoJuego);
            }
        }

        // Sin esto, Collider.bounds sigue devolviendo la caja ANTERIOR al cambio de
        // transform, y BuildCeilings coloca la losa a la altura vieja.
        Physics.SyncTransforms();

        log.AppendLine("  altura (objetivo absoluto): " + muros + " muros en " + salas + " salas -> "
            + (masBaja == float.MaxValue ? "-" : masBaja.ToString("0")) + " a " + masAlta.ToString("0")
            + " m de juego, el suelo no se mueve");
    }

    // ------------------------------------------------------------------
    // 1.0 Trocear los suelos gigantes
    // ------------------------------------------------------------------
    /// <summary>
    /// El suelo del pasillo es UNA sola malla de 32 x 52 m. URP asigna como mucho
    /// 8 luces adicionales POR OBJETO, asi que esos 1600 m2 reciben 8 luces para
    /// todo: da igual cuanto se suba la intensidad, el suelo se queda a oscuras a
    /// tramos. Trocearlo en baldosas hace que cada tramo reciba sus propias 8.
    ///
    /// Las baldosas se solapan 5 cm para que el CharacterController no encuentre
    /// juntas donde engancharse.
    /// </summary>
    static void SplitLargeFloors(Scene scene, StringBuilder log)
    {
        const float AreaMinima = 200f;   // m2 a partir de los cuales trocear
        const float LadoBaldosa = 10f;

        var candidatos = new List<Renderer>();
        foreach (Renderer r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (r.transform.name.StartsWith("Baldosa_")) continue;
            string n = r.transform.name.ToLower();
            if (!n.Contains("floor") && !n.Contains("suelo")) continue;
            Bounds b = r.bounds;
            if (b.size.y > 2f) continue;                 // no es una losa horizontal
            if (b.size.x * b.size.z < AreaMinima) continue;
            candidatos.Add(r);
        }

        int troceados = 0, baldosas = 0;
        foreach (Renderer r in candidatos)
        {
            Bounds b = r.bounds;
            Transform padre = r.transform.parent;
            if (padre == null) continue;
            Vector3 ls = padre.lossyScale;
            if (Mathf.Abs(ls.x) < 0.001f || Mathf.Abs(ls.y) < 0.001f || Mathf.Abs(ls.z) < 0.001f) continue;

            Material mat = r.sharedMaterial;
            int capa = r.gameObject.layer;
            string baseName = r.transform.name;

            int nx = Mathf.Max(1, Mathf.CeilToInt(b.size.x / LadoBaldosa));
            int nz = Mathf.Max(1, Mathf.CeilToInt(b.size.z / LadoBaldosa));
            float pasoX = b.size.x / nx;
            float pasoZ = b.size.z / nz;

            Object.DestroyImmediate(r.gameObject);

            for (int ix = 0; ix < nx; ix++)
                for (int iz = 0; iz < nz; iz++)
                {
                    var t = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    t.name = "Baldosa_" + baseName + "_" + ix + "_" + iz;
                    t.transform.SetParent(padre, true);
                    t.transform.rotation = Quaternion.identity;
                    t.transform.position = new Vector3(
                        b.min.x + pasoX * (ix + 0.5f),
                        b.center.y,
                        b.min.z + pasoZ * (iz + 0.5f));
                    t.transform.localScale = new Vector3(
                        (pasoX + 0.05f) / ls.x,
                        b.size.y / ls.y,
                        (pasoZ + 0.05f) / ls.z);
                    if (mat != null) t.GetComponent<Renderer>().sharedMaterial = mat;
                    t.layer = capa;
                    t.isStatic = true;
                    baldosas++;
                }
            troceados++;
        }

        Physics.SyncTransforms();
        if (troceados == 0) log.AppendLine("  suelos: nada que trocear");
        else log.AppendLine("  suelos troceados: " + troceados + " mallas gigantes -> " + baldosas
            + " baldosas de ~10 m (cada una con su propio cupo de 8 luces)");
    }

    // ------------------------------------------------------------------
    // 1.0b Props fuera de escala
    // ------------------------------------------------------------------
    /// <summary>
    /// Misma familia de fallo que la manzana de 87 m de N04-N06: props importados
    /// sin normalizar. Aqui la mochila mide 2.2 m — del tamaño de un armario — y
    /// como ademas es ambar emisivo, se come el plano entero.
    /// </summary>
    static void FixOversizedProps(Scene scene, StringBuilder log)
    {
        // nombre -> altura real en metros de JUEGO
        var esperado = new Dictionary<string, float>
        {
            { "Bag", 0.45f }, { "Mochila", 0.45f },
            { "Notebook", 0.30f }, { "Cuaderno", 0.30f },
            { "Letter", 0.30f }, { "Carta", 0.30f },
        };

        int ajustados = 0;
        var yaHechos = new HashSet<Transform>();

        foreach (Renderer r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Transform raiz = RaizDelProp(r.transform, esperado);
            if (raiz == null || !yaHechos.Add(raiz)) continue;

            float objetivo = 0f;
            foreach (var kv in esperado)
                if (raiz.name.IndexOf(kv.Key, System.StringComparison.OrdinalIgnoreCase) >= 0) objetivo = kv.Value;
            if (objetivo <= 0f) continue;

            Bounds b = new Bounds();
            bool tiene = false;
            foreach (Renderer rr in raiz.GetComponentsInChildren<Renderer>())
            {
                if (!tiene) { b = rr.bounds; tiene = true; } else b.Encapsulate(rr.bounds);
            }
            if (!tiene || b.size.y < 0.001f) continue;

            // bounds de editor -> metros de juego
            float alturaJuego = b.size.y * 2f;
            if (alturaJuego <= objetivo * 1.6f) continue;   // ya esta bien

            float factor = objetivo / alturaJuego;
            Vector3 antes = raiz.localScale;
            raiz.localScale = antes * factor;
            log.AppendLine("  prop reescalado: " + raiz.name + " " + alturaJuego.ToString("0.00")
                + " m -> " + objetivo.ToString("0.00") + " m");
            ajustados++;
        }

        Physics.SyncTransforms();
        if (ajustados == 0) log.AppendLine("  props: ninguno fuera de escala");
    }

    static Transform RaizDelProp(Transform t, Dictionary<string, float> claves)
    {
        Transform cursor = t;
        int guardia = 0;
        while (cursor != null && guardia++ < 8)
        {
            foreach (var kv in claves)
                if (cursor.name.IndexOf(kv.Key, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return cursor;
            cursor = cursor.parent;
        }
        return null;
    }

    // ------------------------------------------------------------------
    // 1.c La camara tiene que caber bajo el techo
    // ------------------------------------------------------------------
    /// <summary>
    /// Al cerrar los techos salio a la luz un conflicto que antes no se veia
    /// porque el nivel estaba abierto al cielo: el pasillo tiene el techo a
    /// 3.15 m (escala escolar correcta segun SCALE_GUIDE) y la camara orbitaba
    /// a 3.94 m, o sea POR ENCIMA del techo. El jugador desaparecia detras de
    /// la losa.
    ///
    /// Se baja el rig a altura humana. Ademas es mejor plano: una camara baja
    /// en un pasillo con techo visible lee como colegio; una camara alta lee
    /// como maqueta vista desde fuera.
    /// </summary>
    static void TuneCameraForInteriors(Scene scene, StringBuilder log)
    {
        // Techo mas bajo del nivel, para no pasarnos.
        float techoMin = float.MaxValue;
        foreach (Renderer r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (r.transform.name == "Losa")
                techoMin = Mathf.Min(techoMin, r.bounds.min.y * 2f);   // editor -> runtime

        if (techoMin == float.MaxValue) { log.AppendLine("  camara: sin techos medidos, sin cambios"); return; }

        float margen = 0.45f;
        float alturaMax = Mathf.Max(1.6f, techoMin - margen);

        SimpleFollowCamera cam = Object.FindAnyObjectByType<SimpleFollowCamera>();
        if (cam == null) { log.AppendLine("  camara: no hay SimpleFollowCamera"); return; }

        // altura sobre los pies = targetOffset.y + distancia * sin(pitch)
        float offsetY = 1.1f;
        float pitch = 12f;
        float distancia = 4.5f;
        float altura = offsetY + distancia * Mathf.Sin(pitch * Mathf.Deg2Rad);
        while (altura > alturaMax && distancia > 2.5f)
        {
            distancia -= 0.25f;
            altura = offsetY + distancia * Mathf.Sin(pitch * Mathf.Deg2Rad);
        }

        var so = new SerializedObject(cam);
        so.FindProperty("targetOffset").vector3Value = new Vector3(0f, offsetY, 0f);
        so.FindProperty("pitch").floatValue = pitch;
        so.FindProperty("distance").floatValue = distancia;
        so.FindProperty("maxDistance").floatValue = Mathf.Max(distancia + 2f, 7f);
        so.FindProperty("minDistance").floatValue = 2f;
        // El retroceso automatico al soltar un Eco tampoco puede subir del techo.
        SerializedProperty framing = so.FindProperty("framingMaxDistance");
        if (framing != null) framing.floatValue = Mathf.Min(9f, (alturaMax - offsetY) / Mathf.Sin(pitch * Mathf.Deg2Rad));
        so.ApplyModifiedPropertiesWithoutUndo();

        log.AppendLine("  camara ajustada a interior: techo mas bajo " + techoMin.ToString("0.00")
            + " m -> altura de camara " + altura.ToString("0.00") + " m (pitch " + pitch
            + ", dist " + distancia.ToString("0.0") + ", encuadre max "
            + (framing != null ? framing.floatValue.ToString("0.0") : "-") + ")");
    }

    // ------------------------------------------------------------------
    // 1.a El ambar tiene que volver a significar algo
    // ------------------------------------------------------------------
    /// <summary>
    /// EchoesModuleFactory pinta TODOS los pupitres de aula con MemoryMat
    /// (Mat_Token_memory-amber, #FFBF00 con emision 0.9). El ambar es el token
    /// narrativo de la memoria: si brilla el mobiliario entero, no destaca nada
    /// y el aula se convierte en un mar amarillo.
    ///
    /// Se repinta el mobiliario corriente con madera escolar y el ambar se
    /// reserva a los objetos que de verdad cuentan algo.
    /// </summary>
    static readonly string[] PropsNarrativos =
    {
        "Lyra", "Prop_", "Notebook", "Cuaderno", "Letter", "Carta", "Bag", "Mochila",
        "Memory", "Memoria", "Foto", "Cinta", "Coat", "Registros"
    };

    static void RepaintFurniture(Scene scene, StringBuilder log)
    {
        Material ambar = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Echoes/Mat_Token_memory-amber.mat");
        Material madera = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Echoes/Mat_Arch_Seating.mat");
        if (ambar == null || madera == null)
        {
            log.AppendLine("  repintado: falta Mat_Token_memory-amber o Mat_Arch_Seating");
            return;
        }

        int repintados = 0, conservados = 0;
        foreach (Renderer r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Material[] mats = r.sharedMaterials;
            bool tocaAmbar = false;
            for (int i = 0; i < mats.Length; i++)
                if (mats[i] == ambar) tocaAmbar = true;
            if (!tocaAmbar) continue;

            if (EsNarrativo(r.transform)) { conservados++; continue; }

            for (int i = 0; i < mats.Length; i++)
                if (mats[i] == ambar) mats[i] = madera;
            r.sharedMaterials = mats;
            repintados++;
        }

        log.AppendLine("  ambar: " + repintados + " piezas de mobiliario repintadas a madera escolar, "
            + conservados + " props narrativos conservados en #FFBF00");
    }

    static bool EsNarrativo(Transform t)
    {
        Transform cursor = t;
        int guardia = 0;
        while (cursor != null && guardia++ < 8)
        {
            foreach (string clave in PropsNarrativos)
                if (cursor.name.IndexOf(clave, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            cursor = cursor.parent;
        }
        return false;
    }

    // ------------------------------------------------------------------
    // 1.b Techos y luminarias
    // ------------------------------------------------------------------
    /// <summary>
    /// Ninguna sala del bloque tenia techo: un rayo vertical desde la cabeza del
    /// jugador salia al vacio en todos los puntos. Sin techo, un pasillo escolar
    /// se lee como una zanja a cielo abierto y el skybox se cuela por los lados,
    /// que es la razon principal de que los niveles no parezcan un interior.
    ///
    /// Se cierra cada sala y se le cuelgan luminarias visibles, para que la luz
    /// tenga una fuente que se vea y no salga "de la nada".
    /// </summary>
    static void BuildCeilings(Scene scene, StringBuilder log)
    {
        // Los bounds de collider se leen aqui: hay que asegurarse de que reflejan
        // el estado actual y no el previo a los cambios de transform de este pase.
        Physics.SyncTransforms();

        Material matTecho = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Echoes/Mat_LiminalCeiling.mat");
        Material matTubo = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Echoes/Mat_Fluorescent.mat");
        GameObject env = FindRoot(scene, "--- ENVIRONMENT ---");
        if (env == null) { log.AppendLine("  techos: sin root ENVIRONMENT"); return; }

        int techos = 0, tubos = 0;

        foreach (Transform sala in env.transform)
        {
            // idempotente
            Transform previo = sala.Find("Techo");
            if (previo != null) Object.DestroyImmediate(previo.gameObject);

            Bounds b = new Bounds();
            bool tiene = false;
            foreach (Collider c in sala.GetComponentsInChildren<Collider>())
            {
                if (c.isTrigger) continue;
                Bounds cb = c.bounds;
                if (cb.size.magnitude > 300f) continue;
                if (!tiene) { b = cb; tiene = true; } else b.Encapsulate(cb);
            }
            if (!tiene) continue;
            if (b.size.x < 3f || b.size.z < 3f || b.size.y < 1.5f) continue;   // no es una sala

            Vector3 ls = sala.lossyScale;
            if (Mathf.Abs(ls.x) < 0.001f || Mathf.Abs(ls.y) < 0.001f || Mathf.Abs(ls.z) < 0.001f) continue;

            var techo = new GameObject("Techo");
            techo.transform.SetParent(sala, true);
            techo.transform.rotation = Quaternion.identity;

            var losa = GameObject.CreatePrimitive(PrimitiveType.Cube);
            losa.name = "Losa";
            losa.transform.SetParent(techo.transform, true);
            losa.transform.rotation = Quaternion.identity;
            losa.transform.position = new Vector3(b.center.x, b.max.y + 0.05f, b.center.z);
            losa.transform.localScale = new Vector3((b.size.x + 0.4f) / ls.x, 0.2f / ls.y, (b.size.z + 0.4f) / ls.z);
            if (matTecho != null) losa.GetComponent<Renderer>().sharedMaterial = matTecho;
            // Sin collider: el techo es decorativo. Con el, el SphereCast
            // anticolision de SimpleFollowCamera empuja la camara contra la losa
            // y el jugador desaparece detras de ella. El jugador no puede llegar
            // ahi de todos modos (salta 0.84 m y el techo esta a 6-8 m).
            Object.DestroyImmediate(losa.GetComponent<Collider>());
            losa.isStatic = true;
            techos++;

            // Fila de fluorescentes a lo largo del eje mayor de la sala.
            bool largoEnZ = b.size.z >= b.size.x;
            float largo = largoEnZ ? b.size.z : b.size.x;
            int cuantos = Mathf.Clamp(Mathf.RoundToInt(largo / 4f), 1, 6);
            for (int i = 0; i < cuantos; i++)
            {
                float t = cuantos == 1 ? 0.5f : (i + 0.5f) / cuantos;
                Vector3 pos = largoEnZ
                    ? new Vector3(b.center.x, b.max.y - 0.12f, Mathf.Lerp(b.min.z, b.max.z, t))
                    : new Vector3(Mathf.Lerp(b.min.x, b.max.x, t), b.max.y - 0.12f, b.center.z);

                var tubo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tubo.name = "Fluorescente_" + i;
                tubo.transform.SetParent(techo.transform, true);
                tubo.transform.rotation = Quaternion.identity;
                tubo.transform.position = pos;
                tubo.transform.localScale = largoEnZ
                    ? new Vector3(1.1f / ls.x, 0.08f / ls.y, 0.22f / ls.z)
                    : new Vector3(0.22f / ls.x, 0.08f / ls.y, 1.1f / ls.z);
                if (matTubo != null) tubo.GetComponent<Renderer>().sharedMaterial = matTubo;
                Object.DestroyImmediate(tubo.GetComponent<Collider>());   // no estorba al jugador
                tubo.isStatic = true;
                tubos++;
            }
        }

        log.AppendLine("  techos cerrados: " + techos + " salas, " + tubos + " luminarias visibles"
            + "  (antes: 0 techos, cielo abierto en todo el nivel)");
    }

    // ------------------------------------------------------------------
    // 2. Jerarquia KEY / FILL / ACCENT / TRANSITION
    // ------------------------------------------------------------------
    /// <summary>
    /// Asigna un papel a cada luz que YA existe en la escena, en vez de añadir un
    /// juego paralelo. El problema real no es que falten luces: es que las 30-38
    /// que hay estan todas a intensidad 1.5, asi que no hay contraste y el nivel
    /// se lee plano. Aqui se les da jerarquia.
    ///
    /// Nota de escala: Light.range NO se multiplica por el transform, asi que un
    /// rango de 12 en una sala que en runtime mide 16 m se queda corto. Los rangos
    /// se calculan contra las dimensiones de juego (bounds de editor x2).
    /// </summary>
    static void BuildLightingHierarchy(Scene scene, StringBuilder log)
    {
        // El root solo agrupa las que este pase tenga que crear de cero.
        GameObject root = FindOrCreateRoot(scene, LightingRoot);
        for (int i = root.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(root.transform.GetChild(i).gameObject);

        List<Bounds> salas = RoomBounds(scene);

        // Puntos que exigen ACCENT: donde se resuelve el puzzle.
        var focos = new List<Vector3>();
        foreach (PressurePlate p in Object.FindObjectsByType<PressurePlate>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            focos.Add(p.transform.position);
        foreach (DoorController d in Object.FindObjectsByType<DoorController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            focos.Add(d.transform.position);

        // Puntos que exigen TRANSITION: por donde se sale.
        var salidas = new List<Vector3>();
        foreach (LevelExit e in Object.FindObjectsByType<LevelExit>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            salidas.Add(e.transform.position);

        int nKey = 0, nRow = 0, nFill = 0, nAccent = 0, nTrans = 0, nIntacta = 0;

        foreach (Light l in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (l.type == LightType.Directional) continue;

            // La luz de recorte del jugador no participa en la jerarquia del nivel.
            if (l.gameObject.name.Contains("PlayerRim")) { nIntacta++; continue; }

            Vector3 pos = l.transform.position;

            // 1) ACCENT — pegada a una placa o a una puerta.
            if (MasCercaQue(pos, focos, 4.5f))
            {
                Tune(l, AccentColor, 2.1f, 7f);
                l.gameObject.name = Rebautizar(l.gameObject.name, "ACCENT");
                nAccent++;
                continue;
            }

            // 2) TRANSITION — salida y pistas de camino.
            if (MasCercaQue(pos, salidas, 8f) || l.gameObject.name.Contains("PathHint") || l.gameObject.name.Contains("Exit"))
            {
                Tune(l, TransitionColor, 2.6f, 18f);
                l.gameObject.name = Rebautizar(l.gameObject.name, "TRANSITION");
                nTrans++;
                continue;
            }

            // 3) KEY / FILL segun la altura dentro de su sala.
            Bounds sala;
            bool dentro = SalaDe(pos, salas, out sala);
            float rango = dentro
                ? Mathf.Clamp(new Vector2(sala.size.x, sala.size.z).magnitude * 2f * 0.8f, 16f, MaxLightRange)
                : 20f;

            bool esTecho = !dentro || pos.y >= sala.min.y + sala.size.y * 0.62f;
            if (esTecho)
            {
                // Una sola KEY dominante por sala: la fluorescente mas centrada.
                // Las demas del techo bajan a fila secundaria — si todas van a
                // 3.2 lux vuelve a no haber jerarquia, solo mas brillo plano.
                bool esDominante = dentro && EsLaMasCentrada(l, sala);
                if (esDominante)
                {
                    Tune(l, KeyColor, 4.2f, rango);
                    l.gameObject.name = Rebautizar(l.gameObject.name, "KEY");
                    nKey++;
                }
                else
                {
                    Tune(l, KeyColor * 0.92f, 2.4f, rango * 0.9f);
                    l.gameObject.name = Rebautizar(l.gameObject.name, "ROW");
                    nRow++;
                }
            }
            else
            {
                // Relleno bajo: levanta las esquinas para que el espacio se lea.
                // "No usar oscuridad como sustituto de atmosfera."
                Tune(l, FillColor, 1.5f, rango * 0.9f);
                l.gameObject.name = Rebautizar(l.gameObject.name, "FILL");
                nFill++;
            }
        }

        // Si algun foco de puzzle se quedo sin ACCENT, se crea el que falte.
        Transform accentGrp = NewGroup(root.transform, "ACCENT");
        foreach (PressurePlate p in Object.FindObjectsByType<PressurePlate>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (HayLuzCerca(p.transform.position, 4.5f)) continue;
            MakeLight(accentGrp, "ACCENT_Placa_" + p.gameObject.name,
                p.transform.position + Vector3.up * 1.8f, AccentColor, 2.1f, 7f);
            nAccent++;
        }

        // FILL no se puede reclasificar: los niveles solo traen luces de techo.
        // Sin relleno, subir las KEY a 3.2 solo hace las sombras mas duras y las
        // esquinas mas ilegibles. Se crea un rebote bajo y ancho por sala.
        Transform fillGrp = NewGroup(root.transform, "FILL");
        int creadas = 0;
        foreach (Bounds b in salas)
        {
            // Salas pequeñas (pasos, huecos) no necesitan relleno propio.
            if (b.size.x < 4f || b.size.z < 4f) continue;
            float rango = Mathf.Clamp(new Vector2(b.size.x, b.size.z).magnitude * 2f * 0.9f, 18f, MaxLightRange);
            MakeLight(fillGrp, "FILL_Rebote_" + creadas,
                new Vector3(b.center.x, b.min.y + Mathf.Max(0.9f, b.size.y * 0.28f), b.center.z),
                FillColor, 1.5f, rango);
            creadas++;
            nFill++;
        }

        log.AppendLine("  escalera de luz: KEY=" + nKey + " (3.2) ROW=" + nRow + " (1.55) FILL="
            + nFill + " (0.75, " + creadas + " creadas) ACCENT=" + nAccent + " (2.6 ambar) TRANSITION="
            + nTrans + " (2.2) | sin tocar=" + nIntacta);
        log.AppendLine("  escalera 4.2 KEY : 2.6 TRANS : 2.4 ROW : 2.1 ACCENT : 1.5 FILL   (antes: todas planas a 1.5)");
    }

    /// <summary>
    /// RULE-ANTI-002: una placa a menos de 1.5 m de una pared queda incomoda de
    /// pisar y visualmente escondida. Se separa hacia el centro de su sala.
    /// </summary>
    static void FixPlateClearance(Scene scene, StringBuilder log)
    {
        List<Bounds> salas = RoomBounds(scene);
        int movidas = 0;

        foreach (PressurePlate p in Object.FindObjectsByType<PressurePlate>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Vector3 pos = p.transform.position;
            Vector3 origen = pos + Vector3.up * 0.6f;
            Vector3 empuje = Vector3.zero;

            foreach (Vector3 dir in new[] { Vector3.forward, Vector3.back, Vector3.left, Vector3.right })
            {
                RaycastHit hit;
                if (!Physics.Raycast(origen, dir, out hit, MinWallClearance, ~0, QueryTriggerInteraction.Ignore)) continue;
                if (hit.collider.transform.IsChildOf(p.transform)) continue;
                empuje -= dir * (MinWallClearance - hit.distance + 0.15f);
            }

            if (empuje.sqrMagnitude < 0.0001f) continue;

            Vector3 destino = pos + empuje;
            // Solo se mueve si el destino sigue teniendo suelo debajo.
            RaycastHit suelo;
            if (!Physics.Raycast(destino + Vector3.up * 4f, Vector3.down, out suelo, 12f, ~0, QueryTriggerInteraction.Ignore))
                continue;

            // Solo se AVISA, no se mueve: estas placas son decisiones de diseño y
            // el nivel puede estar abierto en el editor mientras corre el pase.
            // Mover geometria de otro por debajo es peor que dejar el aviso.
            destino.y = suelo.point.y + 0.05f;
            movidas++;
            log.AppendLine("  AVISO holgura: " + p.gameObject.name + " en " + pos.ToString("0.00")
                + " esta a menos de 1.5 m de una pared; sugerido " + destino.ToString("0.00"));
        }

        if (movidas == 0) log.AppendLine("  holgura de placas: todas con espacio suficiente");
    }


    /// <summary>True si esta luz es la mas proxima al centro de su sala en XZ.</summary>
    static bool EsLaMasCentrada(Light candidata, Bounds sala)
    {
        Vector2 centro = new Vector2(sala.center.x, sala.center.z);
        float mia = Vector2.Distance(new Vector2(candidata.transform.position.x, candidata.transform.position.z), centro);

        Bounds inflada = sala;
        inflada.Expand(1.5f);

        foreach (Light otra in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (otra == candidata || otra.type == LightType.Directional) continue;
            if (otra.gameObject.name.Contains("PlayerRim")) continue;
            Vector3 p = otra.transform.position;
            if (!inflada.Contains(p)) continue;
            if (p.y < sala.min.y + sala.size.y * 0.62f) continue;   // solo compite el techo
            float suya = Vector2.Distance(new Vector2(p.x, p.z), centro);
            if (suya < mia) return false;
        }
        return true;
    }

    static string Rebautizar(string original, string papel)
    {
        foreach (string p in new[] { "KEY_", "ROW_", "FILL_", "ACCENT_", "TRANSITION_" })
            if (original.StartsWith(p)) original = original.Substring(p.Length);
        return papel + "_" + original;
    }

    static void Tune(Light l, Color color, float intensidad, float rango)
    {
        l.color = color;
        l.intensity = Mathf.Min(intensidad, MaxLightIntensity);
        l.range = Mathf.Min(rango, MaxLightRange);
        l.shadows = LightShadows.None;
        l.lightmapBakeType = LightmapBakeType.Realtime;
    }

    static bool MasCercaQue(Vector3 p, List<Vector3> puntos, float d)
    {
        foreach (Vector3 q in puntos)
            if (Vector3.Distance(p, q) <= d) return true;
        return false;
    }

    static bool HayLuzCerca(Vector3 p, float d)
    {
        foreach (Light l in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (l.type != LightType.Directional && Vector3.Distance(l.transform.position, p) <= d) return true;
        return false;
    }

    static bool SalaDe(Vector3 p, List<Bounds> salas, out Bounds sala)
    {
        foreach (Bounds b in salas)
        {
            Bounds inflada = b;
            inflada.Expand(1.5f);
            if (inflada.Contains(p)) { sala = b; return true; }
        }
        sala = default(Bounds);
        return false;
    }

    static Transform NewGroup(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    static void MakeLight(Transform parent, string name, Vector3 worldPos, Color color, float intensity, float range)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, true);
        go.transform.position = worldPos;
        Light l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = color;
        l.intensity = Mathf.Min(intensity, MaxLightIntensity);
        l.range = Mathf.Min(range, MaxLightRange);
        l.shadows = LightShadows.None;                    // CONS-LGT-003
        l.lightmapBakeType = LightmapBakeType.Realtime;   // RULE-LGT-G05
    }

    /// <summary>Salas reales de la escena, medidas por sus colliders solidos.</summary>
    static List<Bounds> RoomBounds(Scene scene)
    {
        var salas = new List<Bounds>();
        GameObject env = FindRoot(scene, "--- ENVIRONMENT ---");
        if (env == null) return salas;

        foreach (Transform ch in env.transform)
        {
            Bounds b = new Bounds();
            bool tiene = false;
            foreach (Collider c in ch.GetComponentsInChildren<Collider>())
            {
                if (c.isTrigger) continue;
                Bounds cb = c.bounds;
                // descartar bounds corruptos (hay renderers con extension astronomica)
                if (cb.size.magnitude > 300f) continue;
                if (!tiene) { b = cb; tiene = true; } else b.Encapsulate(cb);
            }
            // solo cuentan volumenes con tamaño de sala, no props sueltos
            if (tiene && b.size.x > 3f && b.size.z > 3f && b.size.y > 1.5f)
                salas.Add(b);
        }
        return salas;
    }

    // ------------------------------------------------------------------
    // 3. Cables visibles placa -> puerta (RULE-ANTI-007)
    // ------------------------------------------------------------------
    static void BuildPuzzleCables(Scene scene, StringBuilder log)
    {
        GameObject root = FindOrCreateRoot(scene, CableRoot);
        for (int i = root.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(root.transform.GetChild(i).gameObject);

        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Echoes/Mat_Token_echo-cyan.mat");
        int n = 0;

        foreach (DoorController door in Object.FindObjectsByType<DoorController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (door.plates == null) continue;
            foreach (PressurePlate plate in door.plates)
            {
                if (plate == null) continue;

                var go = new GameObject("Cable_" + plate.gameObject.name + "_a_" + door.gameObject.name);
                go.transform.SetParent(root.transform, true);

                LineRenderer lr = go.AddComponent<LineRenderer>();
                lr.useWorldSpace = true;
                lr.widthMultiplier = 0.06f;
                lr.numCapVertices = 2;
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lr.receiveShadows = false;
                if (mat != null) lr.sharedMaterial = mat;

                // Trazado de instalacion: sube por la placa, cruza a media altura,
                // baja al marco de la puerta. Nunca una recta flotando en el aire.
                Vector3 a = plate.transform.position;
                Vector3 b = door.transform.position;
                float altura = Mathf.Max(a.y, b.y) + 2.6f;
                lr.positionCount = 4;
                lr.SetPosition(0, a + Vector3.up * 0.1f);
                lr.SetPosition(1, new Vector3(a.x, altura, a.z));
                lr.SetPosition(2, new Vector3(b.x, altura, b.z));
                lr.SetPosition(3, b + Vector3.up * 0.6f);

                PuzzleCable cable = go.AddComponent<PuzzleCable>();
                cable.Configure(plate, door, AccentColor);
                n++;
            }
        }
        log.AppendLine("  cables visibles placa->puerta: " + n + "  (RULE-ANTI-007)");
    }

    // ------------------------------------------------------------------
    // 4. Tope de luces
    // ------------------------------------------------------------------
    static void EnforceLightBudget(Scene scene, StringBuilder log)
    {
        var luces = new List<Light>(Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        if (luces.Count <= MaxLightsPerScene)
        {
            log.AppendLine("  presupuesto de luz: " + luces.Count + "/" + MaxLightsPerScene + " OK");
            return;
        }

        // Se van las mas debiles primero, y nunca el sol ni la jerarquia nueva.
        luces.RemoveAll(l => l == null || l.type == LightType.Directional
            || (l.transform.parent != null && l.transform.parent.parent != null
                && l.transform.parent.parent.name == LightingRoot));
        luces.Sort((x, y) => (x.intensity * x.range).CompareTo(y.intensity * y.range));

        int total = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
        int quitadas = 0;
        foreach (Light l in luces)
        {
            if (total - quitadas <= MaxLightsPerScene) break;
            Object.DestroyImmediate(l.gameObject);
            quitadas++;
        }
        log.AppendLine("  presupuesto de luz: eliminadas " + quitadas + " luces debiles -> "
            + (total - quitadas) + "/" + MaxLightsPerScene);
    }

    // ------------------------------------------------------------------
    // 5. Validacion
    // ------------------------------------------------------------------
    static void Validate(Scene scene, StringBuilder log)
    {
        var fallos = new List<string>();

        Light[] luces = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (luces.Length > MaxLightsPerScene)
            fallos.Add("FAIL-LGT-01: " + luces.Length + " luces (tope " + MaxLightsPerScene + ")");

        int soft = 0, bakedSinLightmap = 0, fuera = 0;
        foreach (Light l in luces)
        {
            if (l.shadows == LightShadows.Soft) soft++;
            if (l.type != LightType.Directional && l.lightmapBakeType != LightmapBakeType.Realtime
                && LightmapSettings.lightmaps.Length == 0) bakedSinLightmap++;
            if (l.type != LightType.Directional && (l.intensity > MaxLightIntensity || l.range > MaxLightRange)) fuera++;
        }
        if (soft > 0) fallos.Add("FAIL-LGT-02: " + soft + " luces con sombras Soft");
        if (bakedSinLightmap > 0) fallos.Add("FAIL-LGT-G02: " + bakedSinLightmap + " luces Baked sin lightmaps (no iluminan)");
        if (fuera > 0) fallos.Add("RULE-ANTI-008: " + fuera + " luces fuera de 5 lux / 25 m");

        if (RenderSettings.ambientMode != UnityEngine.Rendering.AmbientMode.Flat)
            fallos.Add("FAIL-LGT-G01: ambiente " + RenderSettings.ambientMode + " en vez de Flat");
        if (!Mathf.Approximately(RenderSettings.fogDensity, FogDensityChapterI))
            fallos.Add("RULE-LGT-005: niebla " + RenderSettings.fogDensity.ToString("0.000") + " en vez de 0.008");

        // RULE-ANTI-002: placas lejos de las paredes
        foreach (PressurePlate p in Object.FindObjectsByType<PressurePlate>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Vector3 o = p.transform.position + Vector3.up * 0.6f;
            foreach (Vector3 dir in new[] { Vector3.forward, Vector3.back, Vector3.left, Vector3.right })
            {
                RaycastHit hit;
                if (Physics.Raycast(o, dir, out hit, MinWallClearance, ~0, QueryTriggerInteraction.Ignore)
                    && !hit.collider.transform.IsChildOf(p.transform))
                {
                    fallos.Add("RULE-ANTI-002: " + p.gameObject.name + " a "
                        + hit.distance.ToString("0.00") + " m de " + hit.collider.transform.name
                        + " (minimo 1.5 m)");
                    break;
                }
            }
        }

        // RULE-ANTI-007: toda pareja placa-puerta necesita cable
        int esperados = 0;
        foreach (DoorController d in Object.FindObjectsByType<DoorController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (d.plates != null)
                foreach (PressurePlate p in d.plates)
                    if (p != null) esperados++;
        int cables = Object.FindObjectsByType<PuzzleCable>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
        if (cables < esperados)
            fallos.Add("RULE-ANTI-007: " + cables + " cables para " + esperados + " enlaces placa-puerta");

        if (fallos.Count == 0)
            log.AppendLine("  VALIDACION: sin incumplimientos (" + luces.Length + " luces)");
        else
        {
            log.AppendLine("  VALIDACION: " + fallos.Count + " incumplimientos");
            foreach (string f in fallos) log.AppendLine("     - " + f);
        }
    }

    // ------------------------------------------------------------------
    static GameObject FindRoot(Scene scene, string name)
    {
        foreach (GameObject go in scene.GetRootGameObjects())
            if (go.name == name) return go;
        return null;
    }

    static GameObject FindOrCreateRoot(Scene scene, string name)
    {
        GameObject existing = FindRoot(scene, name);
        if (existing != null) return existing;
        var go = new GameObject(name);
        SceneManager.MoveGameObjectToScene(go, scene);
        return go;
    }
}
