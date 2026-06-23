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

    private void Awake()
    {
        Debug.Log(
            $"PlayerInventoryManager Awake {GetInstanceID()}"
        );

        if (Instance != null && Instance != this)
        {
            Debug.Log("DESTROY DUPLICATE INVENTORY");
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
            p2FireAmmo = weapon.fireAmmo;
            p2WaterAmmo = weapon.waterAmmo;
            p2CurrentElement = weapon.currentElement;
        }
        else
        {
            p1CurrentAmmo = weapon.currentAmmo;
            p1FireAmmo = weapon.fireAmmo;
            p1WaterAmmo = weapon.waterAmmo;
            p1CurrentElement = weapon.currentElement;
        }
        Debug.Log(
            $"SAVE P{id}: " +
            $"Ammo={weapon.currentAmmo} " +
            $"Fire={weapon.fireAmmo} " +
            $"Water={weapon.waterAmmo} " +
            $"Element={weapon.currentElement}");
    }

    public void LoadToWeapon(WeaponManager weapon)
    {
        Debug.Log(
            $"LOAD REQUEST: PlayerID={weapon.playerController.playerID}");
        if (weapon == null || weapon.playerController == null)
            return;

        int id = weapon.playerController.playerID;

        weapon.magazineSize = magazineSize;

        if (id == 2)
        {
            weapon.currentAmmo = p2CurrentAmmo;
            weapon.fireAmmo = p2FireAmmo;
            weapon.waterAmmo = p2WaterAmmo;
            weapon.currentElement = p2CurrentElement;
        }
        else
        {
            weapon.currentAmmo = p1CurrentAmmo;
            weapon.fireAmmo = p1FireAmmo;
            weapon.waterAmmo = p1WaterAmmo;
            weapon.currentElement = p1CurrentElement;
        }
        weapon.ValidateElementAfterLoad();
        weapon.RefreshUIFromOutside();
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
            SaveFromWeapon(weapon);
        }
    }

    public void ResetInventory()
    {
        Debug.Log("RESET INVENTORY CALLED");
        p1CurrentAmmo = magazineSize;
        p1FireAmmo = 0;
        p1WaterAmmo = 0;
        p1CurrentElement = Element.Physical;

        p2CurrentAmmo = magazineSize;
        p2FireAmmo = 0;
        p2WaterAmmo = 0;
        p2CurrentElement = Element.Physical;
    }
}