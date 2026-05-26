using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    [Header("Player")]
    public GameObject playerPrefab;
    public Transform[] spawnPoints;

    [Header("Lobby / run settings")]
    public bool resetHealthInThisScene = false;

    private void Start()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("GameModeManager: Player Prefab не назначен!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0 || spawnPoints[0] == null)
        {
            Debug.LogError("GameModeManager: не назначен хотя бы PlayerSpawn_1!");
            return;
        }

        if (resetHealthInThisScene)
        {
            PlayerPrefs.DeleteKey("Player1_HP");
            PlayerPrefs.DeleteKey("Player2_HP");
            PlayerPrefs.Save();
        }

        int mode = PlayerPrefs.GetInt("PlayerMode", 1);
        mode = Mathf.Clamp(mode, 1, 2);

        SpawnPlayer(1, spawnPoints[0]);

        if (mode >= 2)
        {
            if (spawnPoints.Length > 1 && spawnPoints[1] != null)
                SpawnPlayer(2, spawnPoints[1]);
            else
                Debug.LogWarning("GameModeManager: выбран кооператив, но PlayerSpawn_2 не назначен. Второй игрок не создан.");
        }
    }

    private void SpawnPlayer(int playerID, Transform spawnPoint)
    {
        GameObject playerObject = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        // Важно: если сам prefab Player был сохранён выключенным в Project/Prefabs,
        // Unity создаёт его тоже выключенным. Из-за этого игрок виден в иерархии серым.
        if (!playerObject.activeSelf)
            playerObject.SetActive(true);

        PlayerController player = playerObject.GetComponent<PlayerController>();

        if (player == null)
        {
            Debug.LogError($"GameModeManager: на префабе игрока нет PlayerController. Объект: {playerObject.name}");
            return;
        }

        player.playerID = playerID;

        string hpKey = $"Player{playerID}_HP";
        if (PlayerPrefs.HasKey(hpKey))
            player.SetHealth(PlayerPrefs.GetInt(hpKey));

        Debug.Log($"GameModeManager: Player {playerID} spawned at {spawnPoint.name}. Active = {playerObject.activeSelf}");
    }
}
