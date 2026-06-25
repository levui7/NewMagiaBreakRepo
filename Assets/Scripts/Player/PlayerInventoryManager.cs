using UnityEngine;

public class PlayerInventoryManager : MonoBehaviour
{
    public static PlayerInventoryManager Instance { get; private set; }

    [Header("Player 1")]
    public PlayerWeaponData player1 = new PlayerWeaponData();

    [Header("Player 2")]
    public PlayerWeaponData player2 = new PlayerWeaponData();

    [Header("Magazine")]
    public int magazineSize = 6;

    [Header("Debug")]
    public bool logDebug = true;

    private const string HasInventoryKey = "Run_HasInventory";
    private const string MagazineSizeKey = "Run_MagazineSize";

    private const string P1CurrentAmmoKey = "Run_P1_CurrentAmmo";
    private const string P1FireAmmoKey = "Run_P1_FireAmmo";
    private const string P1WaterAmmoKey = "Run_P1_WaterAmmo";
    private const string P1CurrentElementKey = "Run_P1_CurrentElement";

    private const string P2CurrentAmmoKey = "Run_P2_CurrentAmmo";
    private const string P2FireAmmoKey = "Run_P2_FireAmmo";
    private const string P2WaterAmmoKey = "Run_P2_WaterAmmo";
    private const string P2CurrentElementKey = "Run_P2_CurrentElement";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (logDebug)
                Debug.Log("PlayerInventoryManager: duplicate destroyed");

            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadInventoryFromPrefs();

        if (logDebug)
        {
            Debug.Log(
                $"PlayerInventoryManager Awake. " +
                $"P1 Element={player1.currentElement}, Fire={player1.fireAmmo}, Water={player1.waterAmmo}");
        }
    }

    public PlayerWeaponData GetDataForPlayer(int playerID)
    {
        return playerID == 2 ? player2 : player1;
    }

    public void SaveFromWeapon(WeaponManager weapon)
    {
        if (weapon == null || weapon.playerController == null)
            return;

        int id = weapon.playerController.playerID;
        PlayerWeaponData data = GetDataForPlayer(id);

        data.CopyFromWeapon(weapon);
        magazineSize = Mathf.Max(1, weapon.magazineSize);

        SaveInventoryToPrefs();

        if (logDebug)
        {
            Debug.Log(
                $"PlayerInventoryManager SAVE FROM WEAPON P{id}: " +
                $"PhysicalAmmo={data.currentAmmo}, " +
                $"FireAmmo={data.fireAmmo}, " +
                $"WaterAmmo={data.waterAmmo}, " +
                $"Element={data.currentElement}");
        }
    }

    public void LoadToWeapon(WeaponManager weapon)
    {
        if (weapon == null || weapon.playerController == null)
            return;

        LoadInventoryFromPrefs();

        int id = weapon.playerController.playerID;
        PlayerWeaponData data = GetDataForPlayer(id);

        weapon.magazineSize = Mathf.Max(1, magazineSize);
        weapon.currentAmmo = Mathf.Clamp(data.currentAmmo, 0, weapon.magazineSize);
        weapon.fireAmmo = Mathf.Max(0, data.fireAmmo);
        weapon.waterAmmo = Mathf.Max(0, data.waterAmmo);

        weapon.ForceSetElementAfterLoad(data.currentElement);
        weapon.RefreshUIFromOutside();

        if (logDebug)
        {
            Debug.Log(
                $"PlayerInventoryManager LOAD TO WEAPON P{id}: " +
                $"PhysicalAmmo={weapon.currentAmmo}, " +
                $"FireAmmo={weapon.fireAmmo}, " +
                $"WaterAmmo={weapon.waterAmmo}, " +
                $"LoadedElement={data.currentElement}, " +
                $"FinalElement={weapon.CurrentElement}");
        }
    }

    public void SaveAllWeaponsInScene()
    {
        WeaponManager[] weapons = FindObjectsOfType<WeaponManager>(true);

        foreach (WeaponManager weapon in weapons)
        {
            if (weapon == null)
                continue;

            if (!weapon.gameObject.activeInHierarchy)
                continue;

            SaveFromWeapon(weapon);
        }

        SaveInventoryToPrefs();
    }

    public void ResetInventory(bool saveToPrefs = true)
    {
        magazineSize = Mathf.Max(1, magazineSize);

        player1.Reset(magazineSize);
        player2.Reset(magazineSize);

        if (saveToPrefs)
            SaveInventoryToPrefs();

        if (logDebug)
            Debug.Log("PlayerInventoryManager: inventory reset");
    }

    public void SaveInventoryToPrefs()
    {
        magazineSize = Mathf.Max(1, magazineSize);

        PlayerPrefs.SetInt(HasInventoryKey, 1);
        PlayerPrefs.SetInt(MagazineSizeKey, magazineSize);

        SavePlayerDataToPrefs(1, player1);
        SavePlayerDataToPrefs(2, player2);

        PlayerPrefs.Save();
    }

    private void SavePlayerDataToPrefs(int playerID, PlayerWeaponData data)
    {
        if (playerID == 2)
        {
            PlayerPrefs.SetInt(P2CurrentAmmoKey, Mathf.Max(0, data.currentAmmo));
            PlayerPrefs.SetInt(P2FireAmmoKey, Mathf.Max(0, data.fireAmmo));
            PlayerPrefs.SetInt(P2WaterAmmoKey, Mathf.Max(0, data.waterAmmo));
            PlayerPrefs.SetInt(P2CurrentElementKey, (int)data.currentElement);
        }
        else
        {
            PlayerPrefs.SetInt(P1CurrentAmmoKey, Mathf.Max(0, data.currentAmmo));
            PlayerPrefs.SetInt(P1FireAmmoKey, Mathf.Max(0, data.fireAmmo));
            PlayerPrefs.SetInt(P1WaterAmmoKey, Mathf.Max(0, data.waterAmmo));
            PlayerPrefs.SetInt(P1CurrentElementKey, (int)data.currentElement);
        }
    }

    public void LoadInventoryFromPrefs()
    {
        if (PlayerPrefs.GetInt(HasInventoryKey, 0) != 1)
            return;

        magazineSize = Mathf.Max(1, PlayerPrefs.GetInt(MagazineSizeKey, magazineSize));

        LoadPlayerDataFromPrefs(1, player1);
        LoadPlayerDataFromPrefs(2, player2);
    }

    private void LoadPlayerDataFromPrefs(int playerID, PlayerWeaponData data)
    {
        if (playerID == 2)
        {
            data.currentAmmo = Mathf.Clamp(PlayerPrefs.GetInt(P2CurrentAmmoKey, magazineSize), 0, magazineSize);
            data.fireAmmo = Mathf.Max(0, PlayerPrefs.GetInt(P2FireAmmoKey, 0));
            data.waterAmmo = Mathf.Max(0, PlayerPrefs.GetInt(P2WaterAmmoKey, 0));
            data.currentElement = IntToElement(PlayerPrefs.GetInt(P2CurrentElementKey, (int)Element.Physical));
        }
        else
        {
            data.currentAmmo = Mathf.Clamp(PlayerPrefs.GetInt(P1CurrentAmmoKey, magazineSize), 0, magazineSize);
            data.fireAmmo = Mathf.Max(0, PlayerPrefs.GetInt(P1FireAmmoKey, 0));
            data.waterAmmo = Mathf.Max(0, PlayerPrefs.GetInt(P1WaterAmmoKey, 0));
            data.currentElement = IntToElement(PlayerPrefs.GetInt(P1CurrentElementKey, (int)Element.Physical));
        }
    }

    private Element IntToElement(int value)
    {
        if (System.Enum.IsDefined(typeof(Element), value))
            return (Element)value;

        return Element.Physical;
    }

    public static void ClearSavedInventoryPrefs()
    {
        PlayerPrefs.DeleteKey(HasInventoryKey);
        PlayerPrefs.DeleteKey(MagazineSizeKey);

        PlayerPrefs.DeleteKey(P1CurrentAmmoKey);
        PlayerPrefs.DeleteKey(P1FireAmmoKey);
        PlayerPrefs.DeleteKey(P1WaterAmmoKey);
        PlayerPrefs.DeleteKey(P1CurrentElementKey);

        PlayerPrefs.DeleteKey(P2CurrentAmmoKey);
        PlayerPrefs.DeleteKey(P2FireAmmoKey);
        PlayerPrefs.DeleteKey(P2WaterAmmoKey);
        PlayerPrefs.DeleteKey(P2CurrentElementKey);

        PlayerPrefs.Save();
    }
}