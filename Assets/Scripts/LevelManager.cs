using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Завершение уровня")]
    public bool loadVictoryWhenCompleted = false;
    public string nextSceneName = "Level_02";
    public string victorySceneName = "VictoryScreen";

    [Header("Портал выхода")]
    public GameObject exitPortal;

    [Header("Обязательные враги")]
    public Enemy[] requiredEnemies;

    [Header("Обязательные боссы")]
    public BossController[] requiredBosses;

    private bool levelCompleted = false;

    private void Start()
    {
        if (exitPortal != null)
            exitPortal.SetActive(false);
    }

    private void Update()
    {
        if (levelCompleted) return;
        CheckLevelCompletion();
    }

    private void CheckLevelCompletion()
    {
        bool allEnemiesDead = true;
        foreach (Enemy enemy in requiredEnemies)
        {
            if (enemy != null)
            {
                allEnemiesDead = false;
                break;
            }
        }

        bool allBossesDead = true;
        foreach (BossController boss in requiredBosses)
        {
            if (boss != null)
            {
                allBossesDead = false;
                break;
            }
        }

        if (allEnemiesDead && allBossesDead)
            CompleteLevel();
    }

    private void CompleteLevel()
    {
        levelCompleted = true;

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
        foreach (PlayerController p in players)
        {
            if (p != null && p.gameObject.activeSelf)
                PlayerPrefs.SetInt($"Player{p.playerID}_HP", p.GetCurrentHealth());
        }

        PlayerPrefs.Save();
    }
}
