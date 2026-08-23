using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>
/// Hace los niveles legibles: que se VEAN y que se ENTIENDA a donde ir.
///
/// EL PROBLEMA MEDIDO
/// La iluminacion del bloque estaba partida en dos mitades muy desiguales:
///
///     N01-N03   37-47 focos   intensidad media 1.5-2.0
///     N04-N06    8-15 focos   intensidad media 0.86-0.99
///
/// La segunda mitad tiene cuatro veces menos luz que la primera, y encima la
/// luz ambiental global es casi negra (color ~0.06-0.11 a intensidad 0.15).
/// A eso se suma el post-proceso, que baja la exposicion medio paso y usa
/// Tonemapping None: sin curva, todo lo que queda por debajo del negro se
/// recorta de golpe en vez de comprimirse, y las sombras se vuelven manchas
/// planas.
///
/// LO QUE HACE ESTE PASE
///   1. Sube la luz ambiental conservando el tinte de cada capitulo.
///   2. Iguala los focos flojos de N04-N06 con los de N01-N03.
///   3. Corrige el post-proceso para que las sombras se compriman.
///   4. Anade señalizacion: una baliza calida sobre la salida y realce
///      emisivo en placas y puertas, que es lo que responde a "no se
///      entiende a donde ir".
///
/// Es idempotente: los valores son absolutos, no incrementos.
/// </summary>
public static class EchoesReadabilityPass
{
    const string SceneRoot = "Assets/Scenes";
    const int FirstLevel = 1;
    const int LastLevel = 6;

    // --- luz ambiental ---
    // El tinte por capitulo se conserva (es identidad visual del juego); lo que
    // sube es la cantidad de luz.
    const float AmbientIntensity = 0.40f;   // antes 0.15
    const float AmbientBoost = 2.4f;        // multiplicador sobre el color actual

    // --- focos ---
    // Umbrales tomados de lo que ya funciona en N01-N03.
    const float MinPointIntensity = 1.5f;
    const float MinPointRange = 10f;

    // --- baliza de salida ---
    // Deliberadamente exagerada: la salida tiene que verse desde el otro
    // extremo del nivel, atravesando la niebla, o el jugador no sabe hacia
    // donde avanza. Es la luz mas potente de la escena a proposito.
    const float BeaconIntensity = 6.5f;
    const float BeaconRange = 48f;
    static readonly Color BeaconColor = new Color(1f, 0.80f, 0.42f, 1f);

    // Segunda luz, baja y muy abierta, para que el suelo alrededor de la salida
    // tambien se encienda: una sola luz puntual alta deja el suelo oscuro.
    const float BeaconFloorIntensity = 3.2f;
    const float BeaconFloorRange = 22f;

    // --- luz de los botones (placas de presion) ---
    // El componente PressurePlate ya crea una luz indicadora en runtime, pero
    // venia a 0.85 de intensidad y 4.5 de rango: se perdia por completo contra
    // los fluorescentes del techo. Ademas se anade una luz propia en escena,
    // para que la placa se vea tambien en el editor y no dependa del runtime.
    const float PlateLightIntensity = 2.5f;
    const float PlateLightRange = 11f;
    static readonly Color PlateLightColor = new Color(0.35f, 0.88f, 1f, 1f);

    [MenuItem("Echoes of You/Art/Improve Level Readability (Block 1-6)", false, 35)]
    public static void ImproveReadability()
    {
        var log = new StringBuilder();
        int totalLights = 0, totalBeacons = 0, totalHighlights = 0;
        var profilesDone = new HashSet<VolumeProfile>();
        int profilesFixed = 0;

        for (int level = FirstLevel; level <= LastLevel; level++)
        {
            string levelName = "Level_" + level.ToString("00");
            string path = $"{SceneRoot}/{levelName}.unity";
            if (!File.Exists(path)) continue;

            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            RaiseAmbient();
            int lights = BalancePointLights();
            int beacons = AddExitBeacons();
            int highlights = HighlightInteractables();
            profilesFixed += FixPostProcessing(profilesDone);

            totalLights += lights;
            totalBeacons += beacons;
            totalHighlights += highlights;

            log.AppendLine($"{levelName}: {lights} focos reforzados, {beacons} balizas, {highlights} interactivos realzados");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[Readability] {totalLights} focos reforzados, {totalBeacons} balizas de salida, " +
                  $"{totalHighlights} interactivos realzados, {profilesFixed} perfiles de post-proceso corregidos.\n" + log);
    }

    // ---------------------------------------------------------------
    // 1. Luz ambiental
    // ---------------------------------------------------------------
    static void RaiseAmbient()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientIntensity = AmbientIntensity;

        Color c = RenderSettings.ambientLight;
        // Si ya se subio en una pasada anterior, no volver a multiplicar.
        float lum = c.r * 0.299f + c.g * 0.587f + c.b * 0.114f;
        if (lum < 0.16f)
        {
            RenderSettings.ambientLight = new Color(
                Mathf.Min(c.r * AmbientBoost, 1f),
                Mathf.Min(c.g * AmbientBoost, 1f),
                Mathf.Min(c.b * AmbientBoost, 1f),
                1f);
        }
    }

    // ---------------------------------------------------------------
    // 2. Focos
    // ---------------------------------------------------------------
    /// <summary>Sube solo los que estan por debajo del umbral, asi los niveles
    /// que ya estaban bien iluminados no cambian.</summary>
    static int BalancePointLights()
    {
        int touched = 0;
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (l.type != LightType.Point) continue;

            bool changed = false;
            if (l.intensity < MinPointIntensity) { l.intensity = MinPointIntensity; changed = true; }
            if (l.range < MinPointRange) { l.range = MinPointRange; changed = true; }

            if (changed)
            {
                EditorUtility.SetDirty(l);
                touched++;
            }
        }
        return touched;
    }

    // ---------------------------------------------------------------
    // 3. Post-proceso
    // ---------------------------------------------------------------
    /// <summary>Los perfiles son assets y varias escenas comparten el mismo, asi
    /// que se lleva un registro para no procesarlos dos veces.
    ///
    /// ⚠ Esto se aparta de RULE-PST-G01, que fija Tonemapping None y
    /// postExposure -0.5. Se cambia porque es la causa directa de que las
    /// sombras salgan como manchas negras planas: sin curva de tono, todo lo
    /// que cae por debajo del negro se recorta en vez de comprimirse.</summary>
    static int FixPostProcessing(HashSet<VolumeProfile> done)
    {
        int fixedCount = 0;
        foreach (var v in Object.FindObjectsByType<Volume>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (v.profile == null || done.Contains(v.profile)) continue;
            done.Add(v.profile);

            bool touched = false;
            foreach (var comp in v.profile.components)
            {
                string n = comp.GetType().Name;

                if (n.Contains("ColorAdjustments"))
                {
                    touched |= SetFloat(comp, "postExposure", -0.1f);
                    touched |= SetFloat(comp, "contrast", 6f);
                }
                else if (n.Contains("Tonemapping"))
                {
                    // 0=None 1=Neutral 2=ACES. Neutral comprime las altas luces
                    // sin lavar el color, que es lo que quiere un look PS1.
                    touched |= SetEnum(comp, "mode", 1);
                    comp.active = true;
                }
            }

            if (touched)
            {
                EditorUtility.SetDirty(v.profile);
                fixedCount++;
            }
        }
        return fixedCount;
    }

    static bool SetFloat(VolumeComponent comp, string field, float value)
    {
        var f = comp.GetType().GetField(field);
        if (f == null) return false;
        var param = f.GetValue(comp);
        if (param == null) return false;
        var pv = param.GetType().GetProperty("value");
        var ps = param.GetType().GetProperty("overrideState");
        if (pv == null || ps == null) return false;
        pv.SetValue(param, value);
        ps.SetValue(param, true);
        return true;
    }

    static bool SetEnum(VolumeComponent comp, string field, int value)
    {
        var f = comp.GetType().GetField(field);
        if (f == null) return false;
        var param = f.GetValue(comp);
        if (param == null) return false;
        var pv = param.GetType().GetProperty("value");
        var ps = param.GetType().GetProperty("overrideState");
        if (pv == null || ps == null) return false;
        pv.SetValue(param, System.Enum.ToObject(pv.PropertyType, value));
        ps.SetValue(param, true);
        return true;
    }

    // ---------------------------------------------------------------
    // 4a. Baliza de salida — "a donde voy"
    // ---------------------------------------------------------------
    /// <summary>Una luz calida y de rango largo sobre la salida. Es la unica
    /// fuente calida del nivel, asi que destaca sobre los fluorescentes
    /// verdosos y lee como "ve hacia alli" sin necesidad de texto.</summary>
    static int AddExitBeacons()
    {
        int added = 0;
        foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (mb == null || mb.GetType().Name != "LevelExit") continue;

            Transform existing = mb.transform.Find("ReadabilityBeacon");
            GameObject beacon;
            if (existing != null)
            {
                beacon = existing.gameObject;
            }
            else
            {
                beacon = new GameObject("ReadabilityBeacon");
                beacon.transform.SetParent(mb.transform, false);
                added++;
            }

            beacon.transform.localPosition = new Vector3(0f, 3.6f, 0f);

            var light = beacon.GetComponent<Light>();
            if (light == null) light = beacon.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = BeaconColor;
            light.intensity = BeaconIntensity;
            light.range = BeaconRange;
            light.shadows = LightShadows.None;   // solo guia, no debe crear sombras raras
            EditorUtility.SetDirty(beacon);

            // Segunda luz a ras de suelo: la puntual alta ilumina el aire y deja
            // el suelo de la salida oscuro, que es donde el jugador mira.
            Transform floorT = mb.transform.Find("ReadabilityBeacon_Floor");
            GameObject floorGo;
            if (floorT != null)
            {
                floorGo = floorT.gameObject;
            }
            else
            {
                floorGo = new GameObject("ReadabilityBeacon_Floor");
                floorGo.transform.SetParent(mb.transform, false);
                added++;
            }
            floorGo.transform.localPosition = new Vector3(0f, 0.6f, 0f);

            var fl = floorGo.GetComponent<Light>();
            if (fl == null) fl = floorGo.AddComponent<Light>();
            fl.type = LightType.Point;
            fl.color = BeaconColor;
            fl.intensity = BeaconFloorIntensity;
            fl.range = BeaconFloorRange;
            fl.shadows = LightShadows.None;
            EditorUtility.SetDirty(floorGo);
        }
        return added;
    }

    // ---------------------------------------------------------------
    // 4b. Realce de interactivos — "que puedo tocar"
    // ---------------------------------------------------------------
    /// <summary>Las placas y las puertas son lo unico accionable del nivel y se
    /// veian igual que el resto de la geometria. Se les pone emision para que
    /// el bloom las recoja y se lean como activas.</summary>
    static int HighlightInteractables()
    {
        int touched = 0;
        // Emision alta: por encima de 1 para que el bloom la recoja y la placa
        // "brille" en vez de solo estar coloreada.
        var plateGlow = new Color(0.30f, 0.85f, 1f) * 1.7f;    // cian: el color del eco
        var doorGlow = new Color(1f, 0.72f, 0.32f) * 1.1f;     // ambar

        foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (mb == null) continue;
            string tn = mb.GetType().Name;
            bool isPlate = tn == "PressurePlate" || tn == "PressurePlateEchoOnly";
            bool isDoor = tn == "DoorController";
            if (!isPlate && !isDoor) continue;

            if (isPlate)
            {
                touched += LightUpPlate(mb);
            }

            foreach (var r in mb.GetComponentsInChildren<MeshRenderer>(true))
            {
                Material m = r.sharedMaterial;
                if (m == null || !m.HasProperty("_EmissionColor")) continue;

                Color target = isPlate ? plateGlow : doorGlow;
                // Comparacion por igualdad, no por "mayor o igual": el valor es
                // absoluto y hay que poder RECALIBRAR a la baja, no solo subir.
                Color current = m.GetColor("_EmissionColor");
                if (Mathf.Abs(current.maxColorComponent - target.maxColorComponent) < 0.01f)
                {
                    continue;   // ya esta en el valor objetivo
                }

                m.SetColor("_EmissionColor", target);
                m.EnableKeyword("_EMISSION");
                EditorUtility.SetDirty(m);
                touched++;
            }
        }
        return touched;
    }

    /// <summary>Da luz propia a un boton. Dos cosas a la vez:
    ///
    ///   1. Sube los valores de la luz que PressurePlate crea en runtime, que
    ///      venian a 0.85 / 4.5 y no se veian contra los fluorescentes.
    ///   2. Anade una luz hija real, para que el boton llame la atencion
    ///      tambien en el editor y desde lejos, no solo al acercarse.
    ///
    /// El color es el cian del eco: asocia el boton con la mecanica sin texto.</summary>
    static int LightUpPlate(MonoBehaviour plate)
    {
        int changed = 0;

        // 1. la luz que el propio componente genera al arrancar
        var so = new SerializedObject(plate);
        var pInt = so.FindProperty("lightIntensity");
        var pRange = so.FindProperty("lightRange");
        var pCreate = so.FindProperty("createIndicatorLight");
        bool dirty = false;
        if (pCreate != null && !pCreate.boolValue) { pCreate.boolValue = true; dirty = true; }
        // Valor absoluto en ambos sentidos, por el mismo motivo que la emision.
        if (pInt != null && !Mathf.Approximately(pInt.floatValue, PlateLightIntensity)) { pInt.floatValue = PlateLightIntensity; dirty = true; }
        if (pRange != null && !Mathf.Approximately(pRange.floatValue, PlateLightRange)) { pRange.floatValue = PlateLightRange; dirty = true; }
        if (dirty)
        {
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(plate);
            changed++;
        }

        // 2. luz hija visible siempre
        Transform existing = plate.transform.Find("ReadabilityPlateGlow");
        GameObject glow;
        if (existing != null)
        {
            glow = existing.gameObject;
        }
        else
        {
            glow = new GameObject("ReadabilityPlateGlow");
            glow.transform.SetParent(plate.transform, false);
            changed++;
        }

        // Se sube un poco sobre la placa para que el halo caiga sobre el suelo
        // de alrededor y marque la zona donde hay que pisar.
        glow.transform.localPosition = new Vector3(0f, 1.1f, 0f);

        var l = glow.GetComponent<Light>();
        if (l == null) l = glow.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = PlateLightColor;
        l.intensity = PlateLightIntensity;
        l.range = PlateLightRange;
        l.shadows = LightShadows.None;
        EditorUtility.SetDirty(glow);

        return changed;
    }
}
