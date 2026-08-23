#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Crea y **repara** el AudioMixer de Echoes.
///
/// Historia del bug que arregla este archivo: la versión anterior creaba los
/// grupos con <c>Activator.CreateInstance(AudioMixerGroupController, mixer)</c>.
/// Eso construye el objeto pero **no** le añade el efecto Attenuation ni le
/// reserva los GUID de volumen/pitch, así que <c>GetGUIDForVolume()</c> devolvía
/// un GUID todo a ceros. Resultado: los 11 parámetros expuestos que no eran
/// MasterVolume quedaban apuntando al mismo GUID nulo y
/// <c>AudioMixer.SetFloat("MusicVolume", …)</c> no atenuaba nada.
///
/// Era un no-op silencioso de manual: el código existía, parecía correcto y los
/// sliders de audio del menú no movían el volumen de nada.
///
/// Ahora los grupos se crean con <c>AudioMixerController.CreateNewGroup</c> (el
/// mismo método que usa el botón "+" del editor de mixers), que sí inicializa el
/// Attenuation. Y <see cref="RepairAudioMixer"/> arregla un mixer ya existente
/// **sin recrearlo**, para no romper las referencias por fileID que tengan las
/// escenas y los prefabs.
/// </summary>
[InitializeOnLoad]
public static class EchoesAudioMixerBuilder
{
    public const string MixerAssetPath = "Assets/Resources/EchoesAudioMixer.mixer";
    const string MixerFolderPath = "Assets/Resources";

    // Nombres de parámetro expuesto — los usa EchoesAudioManager en runtime.
    public const string MasterVolumeParam = "MasterVolume";
    public const string MusicVolumeParam = "MusicVolume";
    public const string SFXVolumeParam = "SFXVolume";
    public const string EchoVolumeParam = "EchoVolume";
    public const string AmbienceVolumeParam = "AmbienceVolume";
    public const string VoiceVolumeParam = "VoiceVolume";
    public const string UIVolumeParam = "UIVolume";
    public const string TapeHissVolumeParam = "TapeHissVolume";
    public const string SFXPlayerVolumeParam = "SFXPlayerVolume";
    public const string SFXFoleyVolumeParam = "SFXFoleyVolume";
    public const string SFXEchoVolumeParam = "SFXEchoVolume";
    public const string SFXUIVolumeParam = "SFXUIVolume";

    /// grupo → nombre del parámetro expuesto que controla su volumen.
    static readonly (string group, string param)[] GroupParams =
    {
        ("Master",     MasterVolumeParam),
        ("Music",      MusicVolumeParam),
        ("SFX",        SFXVolumeParam),
        ("Echo",       EchoVolumeParam),
        ("Ambience",   AmbienceVolumeParam),
        ("Voice",      VoiceVolumeParam),
        ("UI",         UIVolumeParam),
        ("TapeHiss",   TapeHissVolumeParam),
        ("SFX_Player", SFXPlayerVolumeParam),
        ("SFX_Foley",  SFXFoleyVolumeParam),
        ("SFX_Echo",   SFXEchoVolumeParam),
        ("SFX_UI",     SFXUIVolumeParam),
    };

    /// jerarquía: grupo → padre (null = hijo directo de Master).
    static readonly (string name, string parent)[] GroupTree =
    {
        ("Music",      null),
        ("SFX",        null),
        ("Echo",       null),
        ("Ambience",   null),
        ("Voice",      null),
        ("UI",         null),
        ("TapeHiss",   null),
        ("SFX_Player", "SFX"),
        ("SFX_Foley",  "SFX"),
        ("SFX_Echo",   "SFX"),
        ("SFX_UI",     "SFX"),
    };

    static Assembly EditorAssembly => typeof(Editor).Assembly;
    static System.Type TController => EditorAssembly.GetType("UnityEditor.Audio.AudioMixerController");
    static System.Type TGroup => EditorAssembly.GetType("UnityEditor.Audio.AudioMixerGroupController");
    static System.Type TEffect => EditorAssembly.GetType("UnityEditor.Audio.AudioMixerEffectController");
    static System.Type TExposedParam => EditorAssembly.GetType("UnityEditor.Audio.ExposedAudioParameter");

    const BindingFlags AnyInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    const BindingFlags AnyStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

    static EchoesAudioMixerBuilder()
    {
        EditorApplication.delayCall += () => EnsureAudioMixer();
    }

    [MenuItem("Echoes of You/Production/Ensure Audio Mixer", false, 160)]
    public static AudioMixer EnsureAudioMixer()
    {
        if (!AssetDatabase.IsValidFolder(MixerFolderPath))
            AssetDatabase.CreateFolder("Assets", "Resources");

        AudioMixer existing = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerAssetPath);
        if (existing != null && existing.FindMatchingGroups("Master").Length > 0)
        {
            // El mixer existe. Repararlo si le faltan atenuaciones o GUID —
            // el early-out anterior dejaba pasar mixers rotos para siempre.
            RepairAudioMixer(existing, verbose: false);
            return existing;
        }

        if (existing != null || File.Exists(MixerAssetPath))
        {
            Debug.LogWarning("[Echoes Audio] El mixer existente es inválido. Se regenera.");
            AssetDatabase.DeleteAsset(MixerAssetPath);
        }

        return CreateAudioMixer();
    }

    [MenuItem("Echoes of You/Production/Repair Audio Mixer (volúmenes)", false, 161)]
    public static void RepairAudioMixerMenu()
    {
        AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerAssetPath);
        if (mixer == null)
        {
            EnsureAudioMixer();
            return;
        }
        RepairAudioMixer(mixer, verbose: true);
    }

    // ═══════════════════════════════════════════════════════════
    // REPARACIÓN EN SITIO
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Da a cada grupo su efecto Attenuation y su GUID de volumen, y reescribe la
    /// lista de parámetros expuestos para que cada nombre apunte al GUID correcto.
    /// No borra ni recrea el asset: los fileID de los grupos se conservan, así que
    /// las escenas y prefabs que los referencian siguen enganchados.
    /// </summary>
    public static bool RepairAudioMixer(AudioMixer mixer, bool verbose)
    {
        if (mixer == null) return false;
        if (TController == null || TGroup == null || TEffect == null || TExposedParam == null)
        {
            Debug.LogError("[Echoes Audio] No se pudieron obtener los tipos internos del editor de audio.");
            return false;
        }

        Dictionary<string, Object> groups = CollectGroups(mixer);
        var repaired = new List<string>();

        foreach (var (groupName, _) in GroupParams)
        {
            if (!groups.TryGetValue(groupName, out Object group) || group == null)
                continue;

            if (EnsureAttenuationEffect(mixer, group))
                repaired.Add(groupName);
        }

        // Reconstruir los parámetros expuestos desde cero: los antiguos apuntaban
        // todos al GUID nulo y colisionaban entre sí.
        var exposed = new List<object>();
        var seenNames = new HashSet<string>();
        foreach (var (groupName, paramName) in GroupParams)
        {
            if (!groups.TryGetValue(groupName, out Object group) || group == null)
                continue;

            GUID volumeGuid = GetVolumeGuid(group);
            if (volumeGuid.Empty())
            {
                Debug.LogError($"[Echoes Audio] El grupo '{groupName}' sigue sin GUID de volumen tras la reparación.");
                continue;
            }

            object param = System.Activator.CreateInstance(TExposedParam);
            SetMember(TExposedParam, param, "guid", volumeGuid);
            SetMember(TExposedParam, param, "name", paramName);
            exposed.Add(param);
            seenNames.Add(paramName);
        }

        System.Array newExposed = System.Array.CreateInstance(TExposedParam, exposed.Count);
        for (int i = 0; i < exposed.Count; i++)
            newExposed.SetValue(exposed[i], i);
        SetMember(TController, mixer, "exposedParameters", newExposed);

        EditorUtility.SetDirty(mixer);
        AssetDatabase.SaveAssets();

        if (verbose || repaired.Count > 0)
        {
            Debug.Log($"[Echoes Audio] Mixer reparado. Atenuación añadida a {repaired.Count} grupo(s)" +
                      (repaired.Count > 0 ? $" ({string.Join(", ", repaired)})" : "") +
                      $". Parámetros expuestos válidos: {seenNames.Count}/{GroupParams.Length}.");
        }

        return true;
    }

    /// <summary>
    /// Garantiza que el grupo tenga el efecto Attenuation en su cadena y los GUID
    /// de volumen/pitch reservados. Devuelve true si hubo que arreglar algo.
    /// </summary>
    static bool EnsureAttenuationEffect(AudioMixer mixer, Object group)
    {
        MethodInfo preallocate = TGroup.GetMethod("PreallocateGUIDs", AnyInstance);
        preallocate?.Invoke(group, null);

        System.Array effects = GetMember(TGroup, group, "effects") as System.Array;
        bool hasAttenuation = false;
        if (effects != null)
        {
            MethodInfo isAtt = TEffect.GetMethod("IsAttenuation", AnyInstance);
            foreach (var e in effects)
            {
                if (e == null) continue;
                if (isAtt != null && (bool)isAtt.Invoke(e, null)) { hasAttenuation = true; break; }
            }
        }

        if (hasAttenuation && !GetVolumeGuid(group).Empty())
            return false;

        if (!hasAttenuation)
        {
            Object effect = (Object)System.Activator.CreateInstance(TEffect, new object[] { "Attenuation" });
            TEffect.GetMethod("PreallocateGUIDs", AnyInstance)?.Invoke(effect, null);
            effect.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(effect, mixer);

            MethodInfo insert = TGroup.GetMethod("InsertEffect", AnyInstance, null,
                new[] { TEffect, typeof(int) }, null);
            insert?.Invoke(group, new object[] { effect, 0 });
        }

        preallocate?.Invoke(group, null);
        EditorUtility.SetDirty(group);
        return true;
    }

    static GUID GetVolumeGuid(Object group)
    {
        MethodInfo m = TGroup.GetMethod("GetGUIDForVolume", AnyInstance);
        return m == null ? new GUID() : (GUID)m.Invoke(group, null);
    }

    static Dictionary<string, Object> CollectGroups(AudioMixer mixer)
    {
        var result = new Dictionary<string, Object>();
        string path = AssetDatabase.GetAssetPath(mixer);
        foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            if (o == null || !TGroup.IsInstanceOfType(o)) continue;
            result[o.name] = o;
        }
        return result;
    }

    // ═══════════════════════════════════════════════════════════
    // CREACIÓN DESDE CERO
    // ═══════════════════════════════════════════════════════════

    static AudioMixer CreateAudioMixer()
    {
        MethodInfo createMethod = TController.GetMethod("CreateMixerControllerAtPath", AnyStatic);
        if (createMethod == null)
        {
            Debug.LogError("[Echoes Audio] No se encontró AudioMixerController.CreateMixerControllerAtPath.");
            return null;
        }

        var controller = (Object)createMethod.Invoke(null, new object[] { MixerAssetPath });
        if (controller == null)
        {
            Debug.LogError("[Echoes Audio] CreateMixerControllerAtPath devolvió null.");
            return null;
        }

        var masterGroup = (Object)TController.GetProperty("masterGroup", AnyInstance).GetValue(controller, null);
        if (masterGroup == null)
        {
            Debug.LogError("[Echoes Audio] masterGroup null tras crear el controlador.");
            return null;
        }
        masterGroup.name = "Master";

        var created = new Dictionary<string, Object> { ["Master"] = masterGroup };
        foreach (var (name, parent) in GroupTree)
        {
            Object parentGroup = parent != null && created.TryGetValue(parent, out Object p) ? p : masterGroup;
            Object group = CreateChildGroup(controller, parentGroup, name);
            if (group != null) created[name] = group;
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var mixer = (AudioMixer)controller;
        // CreateNewGroup ya deja la atenuación puesta; esto solo escribe la tabla
        // de parámetros expuestos y verifica que ningún GUID salga vacío.
        RepairAudioMixer(mixer, verbose: true);
        return mixer;
    }

    /// <summary>
    /// Crea un grupo con <c>CreateNewGroup</c> — el mismo camino que usa el editor
    /// de mixers, que sí añade el efecto Attenuation y reserva los GUID.
    /// </summary>
    static Object CreateChildGroup(Object mixer, Object parent, string groupName)
    {
        MethodInfo createGroup = TController.GetMethod("CreateNewGroup", AnyInstance, null,
            new[] { typeof(string), typeof(bool) }, null);
        if (createGroup == null)
        {
            Debug.LogError("[Echoes Audio] No se encontró AudioMixerController.CreateNewGroup.");
            return null;
        }

        var group = (Object)createGroup.Invoke(mixer, new object[] { groupName, false });
        if (group == null) return null;

        MethodInfo addChild = TController.GetMethod("AddChildToParent", AnyInstance, null,
            new[] { TGroup, TGroup }, null);
        addChild?.Invoke(mixer, new object[] { group, parent });

        MethodInfo addToView = TController.GetMethod("AddGroupToCurrentView", AnyInstance, null,
            new[] { TGroup }, null);
        addToView?.Invoke(mixer, new object[] { group });

        return group;
    }

    // ═══════════════════════════════════════════════════════════
    // Reflexión: campo o propiedad, lo que exista
    // ═══════════════════════════════════════════════════════════

    static object GetMember(System.Type type, object target, string name)
    {
        FieldInfo f = type.GetField(name, AnyInstance);
        if (f != null) return f.GetValue(target);
        PropertyInfo p = type.GetProperty(name, AnyInstance);
        return p?.GetValue(target, null);
    }

    static void SetMember(System.Type type, object target, string name, object value)
    {
        FieldInfo f = type.GetField(name, AnyInstance);
        if (f != null) { f.SetValue(target, value); return; }
        PropertyInfo p = type.GetProperty(name, AnyInstance);
        p?.SetValue(target, value, null);
    }

    public static AudioMixer LoadMixer() => AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerAssetPath);
}
#endif
