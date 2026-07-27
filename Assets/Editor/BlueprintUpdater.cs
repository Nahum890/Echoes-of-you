using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class BlueprintUpdater
{
    [MenuItem("Echoes of You/Tools/Update All Blueprints")]
    public static void UpdateAll()
    {
        var camProfiles = LoadCameraProfiles();
        var lightProfiles = LoadLightingProfiles();
        var ecoRecordings = LoadEchoRecordings();

        string[] guids = AssetDatabase.FindAssets("t:LevelBlueprint", new[] { "Assets/Data/Levels" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var bp = AssetDatabase.LoadAssetAtPath<LevelBlueprint>(path);
            if (bp == null) continue;

            bool dirty = false;
            string name = bp.levelName;

            // Apply level configuration
            if (ApplyLevelConfig(name, bp, camProfiles, lightProfiles, ecoRecordings)) dirty = true;

            if (dirty)
            {
                EditorUtility.SetDirty(bp);
                Debug.Log($"[BlueprintUpdater] Actualizado: {name}");
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static bool ApplyLevelConfig(string name, LevelBlueprint bp,
        Dictionary<string, CameraProfile> cams, Dictionary<string, LightingProfile> lights,
        Dictionary<string, EchoRecordingData> ecos)
    {
        var config = GetConfigForLevel(name);
        if (config == null) return false;

        bp.echoMode = config.echoMode;
        bp.recordFuture = config.recordFuture;
        bp.degradationPerReplay = config.degradation;
        bp.lockEchoSlots = config.lockSlots;
        bp.lockedSlotIndices = config.lockedIndices;

        if (!string.IsNullOrEmpty(config.cameraProfile) && cams.TryGetValue(config.cameraProfile, out var cam))
            bp.cameraProfile = cam;

        if (!string.IsNullOrEmpty(config.lightProfile) && lights.TryGetValue(config.lightProfile, out var light))
            bp.lightingProfile = light;

        if (!string.IsNullOrEmpty(config.imposedEcho) && ecos.TryGetValue(config.imposedEcho, out var impEco))
            bp.imposedEchoData = impEco;

        if (!string.IsNullOrEmpty(config.ambientEcho) && ecos.TryGetValue(config.ambientEcho, out var ambEco))
            bp.ambientEchoData = ambEco;

        bp.maxEchoes = config.maxEchoes;
        bp.maxRecordSeconds = config.maxRecordSeconds;
        return true;
    }

    static Dictionary<string, CameraProfile> LoadCameraProfiles()
    {
        var dict = new Dictionary<string, CameraProfile>();
        string[] guids = AssetDatabase.FindAssets("t:CameraProfile");
        foreach (string g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            var asset = AssetDatabase.LoadAssetAtPath<CameraProfile>(path);
            if (asset != null && !dict.ContainsKey(asset.name))
            {
                dict.Add(asset.name, asset);
            }
        }
        return dict;
    }

    static Dictionary<string, LightingProfile> LoadLightingProfiles()
    {
        var dict = new Dictionary<string, LightingProfile>();
        string[] guids = AssetDatabase.FindAssets("t:LightingProfile");
        foreach (string g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            var asset = AssetDatabase.LoadAssetAtPath<LightingProfile>(path);
            if (asset != null && !dict.ContainsKey(asset.name))
            {
                dict.Add(asset.name, asset);
            }
        }
        return dict;
    }

    static Dictionary<string, EchoRecordingData> LoadEchoRecordings()
    {
        var dict = new Dictionary<string, EchoRecordingData>();
        string[] guids = AssetDatabase.FindAssets("t:EchoRecordingData");
        foreach (string g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            var asset = AssetDatabase.LoadAssetAtPath<EchoRecordingData>(path);
            if (asset != null && !dict.ContainsKey(asset.name))
            {
                dict.Add(asset.name, asset);
            }
        }
        return dict;
    }

    class LevelConfig
    {
        public EchoPlaybackMode echoMode = EchoPlaybackMode.Standard;
        public bool recordFuture = false;
        public float degradation = 0f;
        public bool lockSlots = false;
        public int[] lockedIndices = null;
        public string cameraProfile;
        public string lightProfile;
        public string imposedEcho;
        public string ambientEcho;
        public int maxEchoes = 1;
        public float maxRecordSeconds = 12f;
    }

    static LevelConfig GetConfigForLevel(string name)
    {
        var configs = new Dictionary<string, LevelConfig>
        {
            ["Level_01"] = new LevelConfig { echoMode = EchoPlaybackMode.Standard, cameraProfile = "Discovery", lightProfile = "FluorescentStandard", maxEchoes = 0, maxRecordSeconds = 0 },
            ["Level_02"] = new LevelConfig { echoMode = EchoPlaybackMode.Standard, cameraProfile = "Learning", lightProfile = "FluorescentStandard", maxEchoes = 1, maxRecordSeconds = 12 },
            ["Level_03"] = new LevelConfig { echoMode = EchoPlaybackMode.Standard, cameraProfile = "Discovery", lightProfile = "FluorescentStandard", maxEchoes = 1, maxRecordSeconds = 12 },
            ["Level_04"] = new LevelConfig { echoMode = EchoPlaybackMode.Standard, degradation = 0.02f, cameraProfile = "Learning", lightProfile = "FluorescentStandard", maxEchoes = 1, maxRecordSeconds = 10 },
            ["Level_05"] = new LevelConfig { echoMode = EchoPlaybackMode.Standard, cameraProfile = "Puzzle", lightProfile = "FluorescentDying", maxEchoes = 1, maxRecordSeconds = 12 },
            ["Level_06"] = new LevelConfig { echoMode = EchoPlaybackMode.Standard, degradation = 0.02f, cameraProfile = "LeapOfFaith", lightProfile = "FluorescentStandard", maxEchoes = 1, maxRecordSeconds = 12 },
            ["Level_07"] = new LevelConfig { echoMode = EchoPlaybackMode.Standard, recordFuture = true, cameraProfile = "Learning", lightProfile = "FluorescentStandard", maxEchoes = 1, maxRecordSeconds = 8 },
            ["Level_08"] = new LevelConfig { echoMode = EchoPlaybackMode.Standard, lockSlots = true, lockedIndices = new[] { 0, 1 }, cameraProfile = "Puzzle", lightProfile = "FluorescentStandard", maxEchoes = 2, maxRecordSeconds = 12 },
            ["Level_09"] = new LevelConfig { echoMode = EchoPlaybackMode.Standard, cameraProfile = "Acceptance", lightProfile = "WindowNatural", maxEchoes = 1, maxRecordSeconds = 12 },
            ["Level_10"] = new LevelConfig { echoMode = EchoPlaybackMode.Ambient, ambientEcho = "Eco_Lyra_N10", cameraProfile = "Emotional", lightProfile = "WarmMemory", maxEchoes = 1, maxRecordSeconds = 12 },
            ["Level_11"] = new LevelConfig { echoMode = EchoPlaybackMode.Standard, maxRecordSeconds = 6, cameraProfile = "Learning", lightProfile = "FluorescentStandard", maxEchoes = 1 },
            ["Level_12"] = new LevelConfig { echoMode = EchoPlaybackMode.Standard, lockSlots = true, lockedIndices = new[] { 0, 1 }, cameraProfile = "Puzzle", lightProfile = "EmergencyRed", maxEchoes = 2, maxRecordSeconds = 12 },
            ["Level_13"] = new LevelConfig { echoMode = EchoPlaybackMode.Imposed, imposedEcho = "Eco_Impuesto_N13", lockSlots = true, lockedIndices = new[] { 0 }, cameraProfile = "Emotional", lightProfile = "EmergencyRed", maxEchoes = 1, maxRecordSeconds = 0 },
            ["Level_14"] = new LevelConfig { echoMode = EchoPlaybackMode.Inversion, imposedEcho = "Eco_Inversion_N14", cameraProfile = "Inversion", lightProfile = "VoidNone", maxEchoes = 0, maxRecordSeconds = 0 },
            ["Level_15"] = new LevelConfig { echoMode = EchoPlaybackMode.Standard, cameraProfile = "Acceptance", lightProfile = "FluorescentStandard", maxEchoes = 1, maxRecordSeconds = 8 },
        };

        configs.TryGetValue(name, out var config);
        return config;
    }
}
