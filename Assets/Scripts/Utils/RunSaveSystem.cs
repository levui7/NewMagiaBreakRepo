using UnityEngine;

public static class RunSaveSystem
{
    private const string HasCheckpointKey = "Run_HasCheckpoint";
    private const string LastSceneKey = "Run_LastScene";

    private const string CoinsKey = "Progress_Coins";
    private const string CrystalsKey = "Progress_Crystals";
    private const string DamageLevelKey = "Progress_DamageLevel";
    private const string HealthLevelKey = "Progress_HealthLevel";
    private const string SpeedLevelKey = "Progress_SpeedLevel";

    public static void StartNewGame(int playerMode, string lobbySceneName)
    {
        PlayerPrefs.SetInt("PlayerMode", Mathf.Clamp(playerMode, 1, 2));

        ResetPermanentProgress();
        ClearRunState();
        RunLevelRouteManager.ClearSavedRoute();

        GameSessionManager session = GameSessionManager.Instance;
        if (session != null)
            session.ResetSessionState();

        if (RunLevelRouteManager.Instance != null)
            RunLevelRouteManager.Instance.StartNewRoute();

        SaveCheckpoint(lobbySceneName);
    }

    public static void SaveCheckpoint(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        PlayerPrefs.SetInt(HasCheckpointKey, 1);
        PlayerPrefs.SetString(LastSceneKey, sceneName);
        PlayerPrefs.Save();
    }

    public static bool CanContinue()
    {
        return PlayerPrefs.GetInt(HasCheckpointKey, 0) == 1 &&
               !string.IsNullOrWhiteSpace(PlayerPrefs.GetString(LastSceneKey, string.Empty));
    }

    public static string GetCheckpointSceneName(string fallbackSceneName = "Lobby")
    {
        string sceneName = PlayerPrefs.GetString(LastSceneKey, string.Empty);
        return string.IsNullOrWhiteSpace(sceneName) ? fallbackSceneName : sceneName;
    }

    public static void SaveRunState(string nextSceneName)
    {
        SavePlayerHealth();

        if (PlayerInventoryManager.Instance != null)
            PlayerInventoryManager.Instance.SaveAllWeaponsInScene();

        SaveCheckpoint(nextSceneName);
    }

    public static void SavePlayerHealth()
    {
        PlayerController[] players = Object.FindObjectsOfType<PlayerController>();

        foreach (PlayerController player in players)
        {
            if (player == null || !player.gameObject.activeInHierarchy)
                continue;

            PlayerPrefs.SetInt($"Player{player.playerID}_HP", player.GetCurrentHealth());
        }

        PlayerPrefs.Save();
    }

    public static void ClearRunState(bool clearInventory = true)
    {
        PlayerPrefs.DeleteKey(HasCheckpointKey);
        PlayerPrefs.DeleteKey(LastSceneKey);

        PlayerPrefs.DeleteKey("Player1_HP");
        PlayerPrefs.DeleteKey("Player2_HP");

        RunLevelRouteManager.ClearSavedRoute();

        PlayerPrefs.Save();

        if (clearInventory && PlayerInventoryManager.Instance != null)
            PlayerInventoryManager.Instance.ResetInventory();

        GameSessionManager session = GameSessionManager.Instance;
        if (session != null)
            session.ResetSessionState();
    }

    public static void ResetPermanentProgress()
    {
        if (PlayerProgressManager.Instance != null)
        {
            PlayerProgressManager.Instance.ResetProgress();
        }
        else
        {
            PlayerPrefs.DeleteKey(CoinsKey);
            PlayerPrefs.DeleteKey(CrystalsKey);
            PlayerPrefs.DeleteKey(DamageLevelKey);
            PlayerPrefs.DeleteKey(HealthLevelKey);
            PlayerPrefs.DeleteKey(SpeedLevelKey);
            PlayerPrefs.Save();
        }

        if (PlayerInventoryManager.Instance != null)
            PlayerInventoryManager.Instance.ResetInventory();
    }
}