using UnityEngine;

public static class GameProgress
{
    const string UnlockedCountKey = "Echoes.UnlockedCount";
    const string CompletedPrefix = "Echoes.Completed.";

    static readonly string[] LevelScenes =
    {
        "Level_01",
        "Level_02",
        "Level_03",
        "Level_04",
        "Level_05",
        "Level_06"
    };

    public static int TotalLevels => LevelScenes.Length;

    public static int GetUnlockedCount()
    {
        return Mathf.Clamp(PlayerPrefs.GetInt(UnlockedCountKey, 1), 1, TotalLevels);
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

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(UnlockedCountKey);

        for (int i = 0; i < LevelScenes.Length; i++)
            PlayerPrefs.DeleteKey(CompletedPrefix + LevelScenes[i]);

        PlayerPrefs.Save();
    }
}
