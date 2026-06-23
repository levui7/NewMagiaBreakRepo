using UnityEngine;

public class PlayerInventoryManager : MonoBehaviour
{
    public static PlayerInventoryManager Instance { get; private set; }

    [Header("Player 1 Ammo")]
    public int p1CurrentAmmo = 6;
    public int p1FireAmmo = 0;
    public int p1WaterAmmo = 0;
    public Element p1CurrentElement = Element.Physical;

    [Header("Player 2 Ammo")]
    public int p2CurrentAmmo = 6;
    public int p2FireAmmo = 0;
    public int p2WaterAmmo = 0;
    public Element p2CurrentElement = Element.Physical;

    [Header("Magazine")]
    public int magazineSize = 6;

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
        Debug.Log($"PlayerInventoryManager Awake {GetInstanceID()}");

        if (Instance != null && Instance != this)
        {
            Debug.Log("DESTROY DUPLICATE INVENTORY");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadInventoryFromPrefs();
    }

    public void SaveFromWeapon(WeaponManager weapon)
    {
        if (weapon == null || weapon.playerController == null)
            return;

        int id = weapon.playerController.playerID;

        if (id == 2)
        {
            p2CurrentAmmo = Mathf.Clamp(weapon.currentAmmo, 0, weapon.magazineSize);
            p2FireAmmo = Mathf.Max(0, weapon.fireAmmo);
            p2WaterAmmo = Mathf.Max(0, weapon.waterAmmo);
            p2CurrentElement = weapon.currentElement;
        }
        else
        {
            p1CurrentAmmo = Mathf.Clamp(weapon.currentAmmo, 0, weapon.magazineSize);
            p1FireAmmo = Mathf.Max(0, weapon.fireAmmo);
            p1WaterAmmo = Mathf.Max(0, weapon.waterAmmo);
            p1CurrentElement = weapon.currentElement;
        }

        magazineSize = Mathf.Max(1, weapon.magazineSize);
        SaveInventoryToPrefs();

        Debug.Log(
            $"SAVE P{id}: " +
            $"Ammo={weapon.currentAmmo} " +
            $"Fire={weapon.fireAmmo} " +
            $"Water={weapon.waterAmmo} " +
            $"Element={weapon.currentElement}");
    }

    public void LoadToWeapon(WeaponManager weapon)
    {
        if (weapon == null || weapon.playerController == null)
            return;

        LoadInventoryFromPrefs();

        int id = weapon.playerController.playerID;

        weapon.magazineSize = Mathf.Max(1, magazineSize);

        if (id == 2)
        {
            weapon.currentAmmo = Mathf.Clamp(p2CurrentAmmo, 0, weapon.magazineSize);
            weapon.fireAmmo = Mathf.Max(0, p2FireAmmo);
            weapon.waterAmmo = Mathf.Max(0, p2WaterAmmo);
            weapon.currentElement = p2CurrentElement;
        }
        else
        {
            weapon.currentAmmo = Mathf.Clamp(p1CurrentAmmo, 0, weapon.magazineSize);
            weapon.fireAmmo = Mathf.Max(0, p1FireAmmo);
            weapon.waterAmmo = Mathf.Max(0, p1WaterAmmo);
            weapon.currentElement = p1CurrentElement;
        }

        weapon.ValidateElementAfterLoad();
        weapon.RefreshUIFromOutside();

        SaveFromWeapon(weapon);

        Debug.Log(
            $"LOAD P{id}: " +
            $"Ammo={weapon.currentAmmo} " +
            $"Fire={weapon.fireAmmo} " +
            $"Water={weapon.waterAmmo} " +
            $"Element={weapon.currentElement}");
    }

    public void SaveAllWeaponsInScene()
    {
        WeaponManager[] weapons = FindObjectsOfType<WeaponManager>();

        foreach (WeaponManager weapon in weapons)
        {
            if (weapon != null && weapon.gameObject.activeInHierarchy)
                SaveFromWeapon(weapon);
        }
    }

    public void ResetInventory(bool saveToPrefs = true)
    {
        Debug.Log("RESET INVENTORY CALLED");

        magazineSize = Mathf.Max(1, magazineSize);

        p1CurrentAmmo = magazineSize;
        p1FireAmmo = 0;
        p1WaterAmmo = 0;
        p1CurrentElement = Element.Physical;

        p2CurrentAmmo = magazineSize;
        p2FireAmmo = 0;
        p2WaterAmmo = 0;
        p2CurrentElement = Element.Physical;

        if (saveToPrefs)
            SaveInventoryToPrefs();
    }

    public void SaveInventoryToPrefs()
    {
        PlayerPrefs.SetInt(HasInventoryKey, 1);
        PlayerPrefs.SetInt(MagazineSizeKey, Mathf.Max(1, magazineSize));

        PlayerPrefs.SetInt(P1CurrentAmmoKey, Mathf.Max(0, p1CurrentAmmo));
        PlayerPrefs.SetInt(P1FireAmmoKey, Mathf.Max(0, p1FireAmmo));
        PlayerPrefs.SetInt(P1WaterAmmoKey, Mathf.Max(0, p1WaterAmmo));
        PlayerPrefs.SetInt(P1CurrentElementKey, (int)p1CurrentElement);

        PlayerPrefs.SetInt(P2CurrentAmmoKey, Mathf.Max(0, p2CurrentAmmo));
        PlayerPrefs.SetInt(P2FireAmmoKey, Mathf.Max(0, p2FireAmmo));
        PlayerPrefs.SetInt(P2WaterAmmoKey, Mathf.Max(0, p2WaterAmmo));
        PlayerPrefs.SetInt(P2CurrentElementKey, (int)p2CurrentElement);

        PlayerPrefs.Save();
    }

    public void LoadInventoryFromPrefs()
    {
        if (PlayerPrefs.GetInt(HasInventoryKey, 0) != 1)
            return;

        magazineSize = Mathf.Max(1, PlayerPrefs.GetInt(MagazineSizeKey, magazineSize));

        p1CurrentAmmo = Mathf.Clamp(PlayerPrefs.GetInt(P1CurrentAmmoKey, p1CurrentAmmo), 0, magazineSize);
        p1FireAmmo = Mathf.Max(0, PlayerPrefs.GetInt(P1FireAmmoKey, p1FireAmmo));
        p1WaterAmmo = Mathf.Max(0, PlayerPrefs.GetInt(P1WaterAmmoKey, p1WaterAmmo));
        p1CurrentElement = IntToElement(PlayerPrefs.GetInt(P1CurrentElementKey, (int)p1CurrentElement));

        p2CurrentAmmo = Mathf.Clamp(PlayerPrefs.GetInt(P2CurrentAmmoKey, p2CurrentAmmo), 0, magazineSize);
        p2FireAmmo = Mathf.Max(0, PlayerPrefs.GetInt(P2FireAmmoKey, p2FireAmmo));
        p2WaterAmmo = Mathf.Max(0, PlayerPrefs.GetInt(P2WaterAmmoKey, p2WaterAmmo));
        p2CurrentElement = IntToElement(PlayerPrefs.GetInt(P2CurrentElementKey, (int)p2CurrentElement));
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