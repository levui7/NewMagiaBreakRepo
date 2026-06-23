using UnityEngine;
using UnityEngine.SceneManagement;

public class GameModeManager : MonoBehaviour
{
    [Header("Player")]
    public GameObject playerPrefab;
    public Transform[] spawnPoints;

    [Header("Lobby / run settings")]
    public bool resetHealthInThisScene = false;

    private void Start()
    {
        RunSaveSystem.SaveCheckpoint(SceneManager.GetActiveScene().name);

        if (playerPrefab == null)
        {
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0 || spawnPoints[0] == null)
        {
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
        }
    }

    private void SpawnPlayer(int playerID, Transform spawnPoint)
    {
        GameObject playerObject = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        PlayerController player = playerObject.GetComponent<PlayerController>();

        player.playerID = playerID;

        if (PlayerProgressManager.Instance != null)
            PlayerProgressManager.Instance.ApplyUpgradesToPlayer(player);

        // Важно: если сам prefab Player был сохранён выключенным в Project/Prefabs,
        // Unity создаёт его тоже выключенным. Из-за этого игрок виден в иерархии серым.
        if (!playerObject.activeSelf)
            playerObject.SetActive(true);

        // PlayerController player = playerObject.GetComponent<PlayerController>();

        if (player == null)
        {
            return;
        }

        WeaponManager weapon = playerObject.GetComponent<WeaponManager>();

        if (weapon != null && PlayerInventoryManager.Instance != null)
            PlayerInventoryManager.Instance.LoadToWeapon(weapon);

        if (weapon != null && WeaponConfigManager.Instance != null)
        {
            weapon.attackMode = WeaponConfigManager.Instance.selectedAttackMode;
        }

        string hpKey = $"Player{playerID}_HP";
        if (PlayerPrefs.HasKey(hpKey))
            player.SetHealth(PlayerPrefs.GetInt(hpKey));

        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();

        if (cameraFollow != null)
        {
            cameraFollow.SetPlayer(playerID, playerObject.transform);
        }
    }
}
