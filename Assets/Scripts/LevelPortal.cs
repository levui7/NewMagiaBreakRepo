using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPortal : MonoBehaviour
{
    [Header("Переход")]
    public string nextSceneName = "Level_01";

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
            player.Heal(healBeforeNextLevel);
        }

        RunSaveSystem.SaveRunState(nextSceneName);

        SceneManager.LoadScene(nextSceneName);
    }
}