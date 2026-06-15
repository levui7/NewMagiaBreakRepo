using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Завершение уровня")]
    public bool loadVictoryWhenCompleted = false;

    public string nextSceneName = "Level_02";
    public string victorySceneName = "VictoryScreen";

    [Header("Портал")]
    public GameObject exitPortal;

    [Header("Автопоиск врагов")]
    public bool autoFindEnemiesOnStart = true;

    private bool levelCompleted;
    private RoomManager roomManager;

    private void Start()
    {
        roomManager = FindObjectOfType<RoomManager>();

        if (exitPortal != null)
            exitPortal.SetActive(false);
    }

    private void Update()
    {
        if (levelCompleted)
            return;

        CheckLevelCompletion();
    }

    private void CheckLevelCompletion()
    {
        // Пока волны не закончились — уровень завершить нельзя
        if (roomManager != null && !roomManager.AllWavesCompleted)
            return;

        Enemy[] enemies = FindObjectsOfType<Enemy>();
        BossController[] bosses = FindObjectsOfType<BossController>();

        if (enemies.Length == 0 && bosses.Length == 0)
        {
            CompleteLevel();
        }
    }

    private void CompleteLevel()
    {
        levelCompleted = true;

        Debug.Log("Level completed");

        if (loadVictoryWhenCompleted)
        {
            SavePlayersHealth();
            SceneManager.LoadScene(victorySceneName);
            return;
        }

        if (exitPortal != null)
            exitPortal.SetActive(true);
    }

    private void SavePlayersHealth()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach (PlayerController player in players)
        {
            if (player == null)
                continue;

            if (!player.gameObject.activeInHierarchy)
                continue;

            PlayerPrefs.SetInt(
                $"Player{player.playerID}_HP",
                player.GetCurrentHealth()
            );
        }

        PlayerPrefs.Save();
    }
}