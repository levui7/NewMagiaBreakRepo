using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
            return;

        if (spawnPoints == null || spawnPoints.Length == 0 || spawnPoints[0] == null)
            return;

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
        GameObject playerObject = Instantiate(
            playerPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        if (!playerObject.activeSelf)
            playerObject.SetActive(true);

        PlayerController player = playerObject.GetComponent<PlayerController>();

        if (player == null)
            return;

        player.playerID = playerID;

        WeaponManager weapon = playerObject.GetComponent<WeaponManager>();

        if (weapon == null)
            weapon = playerObject.GetComponentInChildren<WeaponManager>();

        if (weapon != null)
        {
            weapon.playerController = player;

            if (PlayerInventoryManager.Instance != null)
            {
                PlayerInventoryManager.Instance.LoadToWeapon(weapon);
            }

            if (WeaponConfigManager.Instance != null)
            {
                WeaponConfigManager.Instance.ApplyAttackModeOnly(weapon);
            }

            weapon.ValidateElementAfterLoad();
            weapon.RefreshUIFromOutside();

            StartCoroutine(InitializePlayerWeapon(player, weapon));
        }

        if (PlayerProgressManager.Instance != null)
            PlayerProgressManager.Instance.ApplyUpgradesToPlayer(player);

        string hpKey = $"Player{playerID}_HP";

        if (PlayerPrefs.HasKey(hpKey))
            player.SetHealth(PlayerPrefs.GetInt(hpKey));

        CameraFollow cameraFollow =
            Camera.main != null
                ? Camera.main.GetComponent<CameraFollow>()
                : null;

        if (cameraFollow != null)
            cameraFollow.SetPlayer(playerID, playerObject.transform);

        Debug.Log($"GameModeManager: Player {playerID} spawned");

        if (UIManager.Instance != null)
        {
            int playersCount = FindObjectsOfType<PlayerController>().Length;
            UIManager.Instance.SetPlayersCount(playersCount);
        }
    }

    private IEnumerator InitializePlayerWeapon(PlayerController player, WeaponManager weapon)
    {
        yield return null;

        if (weapon == null)
            yield break;

        if (PlayerInventoryManager.Instance != null)
            PlayerInventoryManager.Instance.LoadToWeapon(weapon);

        if (WeaponConfigManager.Instance != null)
            WeaponConfigManager.Instance.ApplyAttackModeOnly(weapon);

        weapon.ValidateElementAfterLoad();

        if (PlayerProgressManager.Instance != null)
            PlayerProgressManager.Instance.ApplyUpgradesToPlayer(player);

        weapon.RefreshUIFromOutside();

        Debug.Log(
            $"GameModeManager: weapon initialized after frame. " +
            $"Player={player.playerID}, " +
            $"AttackMode={weapon.attackMode}, " +
            $"Element={weapon.CurrentElement}, " +
            $"Fire={weapon.fireAmmo}, Water={weapon.waterAmmo}");
    }
}