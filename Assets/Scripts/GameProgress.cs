using UnityEngine;

public static class GameProgress
{
    const string UnlockedCountKey = "Echoes.UnlockedCount";
    const string CompletedPrefix = "Echoes.Completed.";
    const string DeathsPrefix = "Deaths_";
    const string TotalEchoesKey = "Echoes.TotalEchoesCreated";
    const string TotalPlayTimeKey = "Echoes.TotalPlayTimeSeconds";
    const string LastPlayedKey = "Echoes.LastPlayedScene";
    const string SessionsKey = "Echoes.SessionCount";
    const string ProgressVersionKey = "Echoes.ProgressVersion";
    const int CurrentProgressVersion = 2;

    static readonly string[] LevelScenes =
    {
        "Level_01",
        "Level_02",
        "Level_03",
        "Level_04",
        "Level_05",
        "Level_06",
        "Level_07",
        "Level_08",
        "Level_09",
        "Level_10"
    };

    static readonly string[] LevelDisplayNames =
    {
        "Primer eco",
        "Espacio dinámico",
        "Galería del eco",
        "Cruce doble",
        "Caos controlado",
        "Dominio",
        "Sala de enlace",
        "Puente del vacío",
        "Muro de energía",
        "Archivo dual"
    };

    public static int TotalLevels => LevelScenes.Length;

    static bool _initialized;

    public static void EnsureInitialized()
    {
        if (_initialized)
            return;

        _initialized = true;

        int version = PlayerPrefs.GetInt(ProgressVersionKey, 0);
        if (version < CurrentProgressVersion)
        {
            if (version == 0 && !PlayerPrefs.HasKey(UnlockedCountKey))
            {
                PlayerPrefs.SetInt(UnlockedCountKey, 1);
                for (int i = 0; i < LevelScenes.Length; i++)
                    PlayerPrefs.SetInt(CompletedPrefix + LevelScenes[i], 0);
            }

            PlayerPrefs.SetInt(ProgressVersionKey, CurrentProgressVersion);
            PlayerPrefs.Save();
        }

        SanitizeProgress();
    }

    public static int GetUnlockedCount()
    {
        EnsureInitialized();
        return Mathf.Clamp(PlayerPrefs.GetInt(UnlockedCountKey, 1), 1, TotalLevels);
    }

    /// <summary>Corrige desbloqueos inflados o progreso imposible guardado en pruebas.</summary>
    public static void SanitizeProgress()
    {
        int completed = 0;
        for (int i = 0; i < LevelScenes.Length; i++)
        {
            if (PlayerPrefs.GetInt(CompletedPrefix + LevelScenes[i], 0) == 1)
                completed++;
        }

        int unlocked = Mathf.Clamp(PlayerPrefs.GetInt(UnlockedCountKey, 1), 1, TotalLevels);
        int expectedUnlocked = Mathf.Clamp(completed + 1, 1, TotalLevels);

        int totalEchoes = Mathf.Max(0, PlayerPrefs.GetInt(TotalEchoesKey, 0));
        float totalTime = Mathf.Max(0f, PlayerPrefs.GetFloat(TotalPlayTimeKey, 0f));
        bool suspiciousFullCompletion = completed >= TotalLevels
            && totalEchoes <= 0
            && totalTime < 45f;

        if (suspiciousFullCompletion)
        {
            ResetProgress();
            return;
        }

        if (unlocked > expectedUnlocked)
        {
            PlayerPrefs.SetInt(UnlockedCountKey, expectedUnlocked);
            PlayerPrefs.Save();
        }
    }

    public static int GetCompletedCount()
    {
        int completed = 0;
        for (int i = 0; i < LevelScenes.Length; i++)
        {
            if (IsSceneCompleted(LevelScenes[i]))
                completed++;
        }

        return completed;
    }

    public static int GetSceneIndex(string sceneName)
    {
        for (int i = 0; i < LevelScenes.Length; i++)
        {
            if (LevelScenes[i] == sceneName)
                return i;
        }

        return -1;
    }

    public static bool IsSceneUnlocked(string sceneName)
    {
        int index = GetSceneIndex(sceneName);
        if (index < 0)
            return false;

        return index < GetUnlockedCount();
    }

    public static bool IsSceneCompleted(string sceneName)
    {
        return PlayerPrefs.GetInt(CompletedPrefix + sceneName, 0) == 1;
    }

    public static void MarkSceneCompleted(string sceneName)
    {
        int index = GetSceneIndex(sceneName);
        if (index < 0)
            return;

        PlayerPrefs.SetInt(CompletedPrefix + sceneName, 1);

        int unlocked = GetUnlockedCount();
        int nextUnlocked = Mathf.Clamp(Mathf.Max(unlocked, index + 2), 1, TotalLevels);
        PlayerPrefs.SetInt(UnlockedCountKey, nextUnlocked);
        PlayerPrefs.Save();
    }

    public static string GetHighestUnlockedSceneName()
    {
        int unlockedIndex = Mathf.Clamp(GetUnlockedCount() - 1, 0, TotalLevels - 1);
        return LevelScenes[unlockedIndex];
    }

    public static string GetLevelDisplayName(string sceneName)
    {
        int index = GetSceneIndex(sceneName);
        if (index < 0 || index >= LevelDisplayNames.Length)
            return sceneName;

        return LevelDisplayNames[index];
    }

    public static int GetSceneDeathCount(string sceneName)
    {
        return Mathf.Max(0, PlayerPrefs.GetInt(DeathsPrefix + sceneName, 0));
    }

    public static int GetTotalDeathCount()
    {
        int total = 0;
        for (int i = 0; i < LevelScenes.Length; i++)
            total += GetSceneDeathCount(LevelScenes[i]);

        return total;
    }

    public static int GetTotalEchoesCreated()
    {
        return Mathf.Max(0, PlayerPrefs.GetInt(TotalEchoesKey, 0));
    }

    public static float GetTotalPlayTimeSeconds()
    {
        return Mathf.Max(0f, PlayerPrefs.GetFloat(TotalPlayTimeKey, 0f));
    }

    public static string GetLastPlayedSceneName()
    {
        string last = PlayerPrefs.GetString(LastPlayedKey, string.Empty);
        if (!string.IsNullOrEmpty(last) && GetSceneIndex(last) >= 0)
            return last;

        return LevelScenes[0];
    }

    public static int GetSessionCount()
    {
        return Mathf.Max(1, PlayerPrefs.GetInt(SessionsKey, 1));
    }

    static bool _sessionCountedThisRun;

    public static void RecordSessionStarted()
    {
        if (_sessionCountedThisRun)
            return;

        _sessionCountedThisRun = true;
        int sessions = PlayerPrefs.GetInt(SessionsKey, 0);
        PlayerPrefs.SetInt(SessionsKey, Mathf.Max(1, sessions + 1));
        PlayerPrefs.Save();
    }

    public static void SetLastPlayedScene(string sceneName)
    {
        if (GetSceneIndex(sceneName) < 0)
            return;

        PlayerPrefs.SetString(LastPlayedKey, sceneName);
        PlayerPrefs.Save();
    }

    public static void RecordEchoCreated()
    {
        int total = GetTotalEchoesCreated();
        PlayerPrefs.SetInt(TotalEchoesKey, total + 1);
        PlayerPrefs.Save();
    }

    public static void AddPlayTime(float deltaSeconds)
    {
        if (deltaSeconds <= 0f)
            return;

        float total = GetTotalPlayTimeSeconds() + deltaSeconds;
        PlayerPrefs.SetFloat(TotalPlayTimeKey, total);
    }

    public static void SavePlayTime()
    {
        PlayerPrefs.Save();
    }

    /// <summary>Siguiente fragmento a jugar: primero incompleto desbloqueado; si todo está hecho, el último nivel.</summary>
    public static string GetContinueSceneName()
    {
        int unlocked = GetUnlockedCount();
        for (int i = 0; i < unlocked; i++)
        {
            if (!IsSceneCompleted(LevelScenes[i]))
                return LevelScenes[i];
        }

        return GetHighestUnlockedSceneName();
    }

    public static int GetIntegrityPercent()
    {
        int completed = GetCompletedCount();
        float completionRatio = TotalLevels > 0 ? (float)completed / TotalLevels : 0f;
        float baseIntegrity = 38f + completionRatio * 52f;
        float deathPenalty = Mathf.Min(18f, GetTotalDeathCount() * 0.65f);
        float echoBonus = Mathf.Min(6f, GetTotalEchoesCreated() * 0.04f);

        if (completed >= TotalLevels && TotalLevels > 0)
            baseIntegrity += 8f;

        return Mathf.Clamp(Mathf.RoundToInt(baseIntegrity - deathPenalty + echoBonus), 22, 100);
    }

    public static string GetArchivistRank()
    {
        int completed = GetCompletedCount();
        if (completed == 0) return "INICIALIZANDO";
        if (completed < 2) return "OBSERVADOR";
        if (completed < 4) return "SINCRONIZADOR";
        if (completed < TotalLevels) return "RESTAURADOR";
        return "NÚCLEO ESTABLE";
    }

    public static string GetActiveProtocolMessage(int completedLevels, int totalLevels)
    {
        if (completedLevels == 0)
            return "El Archivo aguarda tu primera decisión. Proyecta un eco antes de cruzar lo imposible.";
        if (completedLevels < totalLevels / 2)
            return "Varios fragmentos siguen desalineados. Usa la proyección (F) para actuar sin mover tu cuerpo.";
        if (completedLevels < totalLevels)
            return "La coherencia mejora. Evita ecos en rutas que cierran tu propio camino.";
        return "Todos los nodos de memoria están activos. El Archivo reconoce tu patrón.";
    }

    public static string FormatPlayTime(float seconds)
    {
        int total = Mathf.Max(0, Mathf.FloorToInt(seconds));
        int hours = total / 3600;
        int minutes = (total % 3600) / 60;
        int secs = total % 60;

        if (hours > 0)
            return $"{hours}h {minutes:00}m";

        if (minutes > 0)
            return $"{minutes}m {secs:00}s";

        return $"{secs}s";
    }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(UnlockedCountKey);
        PlayerPrefs.DeleteKey(TotalEchoesKey);
        PlayerPrefs.DeleteKey(TotalPlayTimeKey);
        PlayerPrefs.DeleteKey(LastPlayedKey);
        PlayerPrefs.DeleteKey(SessionsKey);

        for (int i = 0; i < LevelScenes.Length; i++)
        {
            PlayerPrefs.DeleteKey(CompletedPrefix + LevelScenes[i]);
            PlayerPrefs.DeleteKey(DeathsPrefix + LevelScenes[i]);
        }

        PlayerPrefs.SetInt(UnlockedCountKey, 1);
        PlayerPrefs.SetInt(ProgressVersionKey, CurrentProgressVersion);
        PlayerPrefs.SetInt(SessionsKey, 1);
        _sessionCountedThisRun = false;
        _initialized = false;
        PlayerPrefs.Save();
        EnsureInitialized();
    }
}
