using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPortal : MonoBehaviour
{
    [Header("Переход")]
    [Tooltip("Запасная сцена. Используется, если случайный маршрут не настроен.")]
    public string nextSceneName = "Level_01";

    [Tooltip("Если включено, портал берёт следующую сцену из случайного маршрута забега.")]
    public bool useRandomRunRoute = true;

    [Header("Лечение перед переходом")]
    public int healBeforeNextLevel = 30;
    public bool healAllPlayers = true;

    private bool used = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used)
            return;

        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null)
            return;

        used = true;

        HealBeforeTransition(player);

        string targetSceneName = GetTargetSceneName();

        RunSaveSystem.SaveRunState(targetSceneName);
        SceneManager.LoadScene(targetSceneName);
    }

    private void HealBeforeTransition(PlayerController triggeringPlayer)
    {
        if (healAllPlayers)
        {
            PlayerController[] players = FindObjectsOfType<PlayerController>();

            foreach (PlayerController p in players)
            {
                if (p != null && p.gameObject.activeSelf)
                    p.Heal(healBeforeNextLevel);
            }
        }
        else
        {
            triggeringPlayer.Heal(healBeforeNextLevel);
        }
    }

    private string GetTargetSceneName()
    {
        if (useRandomRunRoute && RunLevelRouteManager.Instance != null)
            return RunLevelRouteManager.Instance.GetNextScene(nextSceneName);

        return nextSceneName;
    }
}