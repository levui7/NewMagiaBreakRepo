using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance { get; private set; }

    [Header("Game Over")]
    public string gameOverSceneName = "GameOverScreen";
    public float delayBeforeGameOver = 1f;

    [Header("Players")]
    public bool waitUntilAllPlayersDead = false;

    private bool gameOverStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void OnPlayerDied(PlayerController deadPlayer)
    {
        if (gameOverStarted)
            return;

        if (waitUntilAllPlayersDead)
        {
            PlayerController[] players = FindObjectsOfType<PlayerController>();

            foreach (PlayerController player in players)
            {
                if (player != null && player.gameObject.activeInHierarchy && player.GetCurrentHealth() > 0)
                    return;
            }
        }

        StartCoroutine(LoadGameOverAfterDelay());
    }

    private IEnumerator LoadGameOverAfterDelay()
    {
        gameOverStarted = true;

        yield return new WaitForSeconds(delayBeforeGameOver);

        if (!string.IsNullOrEmpty(gameOverSceneName))
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
        else
        {
            Debug.LogError("GameSessionManager: gameOverSceneName не задан.");
        }
    }

    public void ResetSessionState()
    {
        gameOverStarted = false;
    }
}