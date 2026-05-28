using UnityEngine;

public class PlayerInventoryManager : MonoBehaviour
{
    public static PlayerInventoryManager Instance { get; private set; }

    [Header("Player 1 Ammo")]
    public int p1CurrentAmmo = 6;
    public int p1ReserveAmmo = 24;
    public int p1FireAmmo = 0;
    public int p1WaterAmmo = 0;
    public WeaponManager.Element p1CurrentElement = WeaponManager.Element.Physical;

    [Header("Player 2 Ammo")]
    public int p2CurrentAmmo = 6;
    public int p2ReserveAmmo = 24;
    public int p2FireAmmo = 0;
    public int p2WaterAmmo = 0;
    public WeaponManager.Element p2CurrentElement = WeaponManager.Element.Physical;

    [Header("Magazine")]
    public int magazineSize = 6;

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

    public void SaveFromWeapon(WeaponManager weapon)
    {
        if (weapon == null || weapon.playerController == null)
            return;

        int id = weapon.playerController.playerID;

        if (id == 2)
        {
            p2CurrentAmmo = weapon.currentAmmo;
            p2ReserveAmmo = weapon.reserveAmmo;
            p2FireAmmo = weapon.fireAmmo;
            p2WaterAmmo = weapon.waterAmmo;
            p2CurrentElement = weapon.currentElement;
        }
        else
        {
            p1CurrentAmmo = weapon.currentAmmo;
            p1ReserveAmmo = weapon.reserveAmmo;
            p1FireAmmo = weapon.fireAmmo;
            p1WaterAmmo = weapon.waterAmmo;
            p1CurrentElement = weapon.currentElement;
        }
    }

    public void LoadToWeapon(WeaponManager weapon)
    {
        if (weapon == null || weapon.playerController == null)
            return;

        int id = weapon.playerController.playerID;

        weapon.magazineSize = magazineSize;

        if (id == 2)
        {
            weapon.currentAmmo = p2CurrentAmmo;
            weapon.reserveAmmo = p2ReserveAmmo;
            weapon.fireAmmo = p2FireAmmo;
            weapon.waterAmmo = p2WaterAmmo;
            weapon.currentElement = p2CurrentElement;
        }
        else
        {
            weapon.currentAmmo = p1CurrentAmmo;
            weapon.reserveAmmo = p1ReserveAmmo;
            weapon.fireAmmo = p1FireAmmo;
            weapon.waterAmmo = p1WaterAmmo;
            weapon.currentElement = p1CurrentElement;
        }

        weapon.ValidateElementAfterLoad();
        weapon.RefreshUIFromOutside();
    }

    public void SaveAllWeaponsInScene()
    {
        WeaponManager[] weapons = FindObjectsOfType<WeaponManager>();

        foreach (WeaponManager weapon in weapons)
        {
            SaveFromWeapon(weapon);
        }
    }

    public void ResetInventory()
    {
        p1CurrentAmmo = magazineSize;
        p1ReserveAmmo = 24;
        p1FireAmmo = 0;
        p1WaterAmmo = 0;
        p1CurrentElement = WeaponManager.Element.Physical;

        p2CurrentAmmo = magazineSize;
        p2ReserveAmmo = 24;
        p2FireAmmo = 0;
        p2WaterAmmo = 0;
        p2CurrentElement = WeaponManager.Element.Physical;
    }
}