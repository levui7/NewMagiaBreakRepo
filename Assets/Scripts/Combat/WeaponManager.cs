using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    public enum Element
    {
        Physical,
        Fire,
        Water,
        Steam,
        Smoldering
    }

    [Header("Owner")]
    public PlayerController playerController;

    [Header("Current Element")]
    public Element currentElement = Element.Physical;

    [Header("Ammo")]
    public int magazineSize = 6;
    public int currentAmmo = 6;
    public int reserveAmmo = 24;
    public int fireAmmo = 0;
    public int waterAmmo = 0;

    [Header("Damage")]
    public float physicalDamage = 10f;
    public float fireDamage = 8f;
    public float waterDamage = 8f;

    [Header("Auto Ammo")]
    public bool autoRefillMagazine = true;

    [Header("Debug")]
    public bool autoFillAmmoOnStart = false;

    public Element CurrentElement => currentElement;
    public int MagazineSize => magazineSize;
    public int CurrentAmmo => currentAmmo;
    public int ReserveAmmo => reserveAmmo;
    public int FireAmmo => fireAmmo;
    public int WaterAmmo => waterAmmo;
    public bool IsReloading => false;

    private bool loadedFromInventory;

    private TemporaryBuffController2D temporaryBuffs;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        temporaryBuffs = playerController != null ? playerController.GetComponent<TemporaryBuffController2D>() : GetComponent<TemporaryBuffController2D>();

        if (magazineSize <= 0)
            magazineSize = 6;

        currentAmmo = Mathf.Clamp(currentAmmo, 0, magazineSize);

        if (autoFillAmmoOnStart)
        {
            currentAmmo = magazineSize;
            reserveAmmo = Mathf.Max(reserveAmmo, 60);
            fireAmmo = Mathf.Max(fireAmmo, 20);
            waterAmmo = Mathf.Max(waterAmmo, 20);
        }
    }

    private void Start()
    {
        LoadInventoryIfPossible();

        AutoRefillFromReserve();
        ValidateElementAfterLoad();
        RefreshUI();
    }

    private void Update()
    {
        LoadInventoryIfPossible();
        HandleElementCycle();

        if (autoRefillMagazine)
            AutoRefillFromReserve();
    }

    private void LoadInventoryIfPossible()
    {
        if (loadedFromInventory)
            return;

        if (PlayerInventoryManager.Instance == null)
            return;

        PlayerInventoryManager.Instance.LoadToWeapon(this);
        loadedFromInventory = true;
    }

    private void HandleElementCycle()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        int id = playerController != null ? playerController.playerID : 1;

        if (id == 2)
        {
            // Игрок 2: U — переключение типа снаряда
            if (keyboard.uKey.wasPressedThisFrame)
                CycleElement();
        }
        else
        {
            // Игрок 1: Q — переключение типа снаряда
            if (keyboard.qKey.wasPressedThisFrame)
                CycleElement();
        }
    }

    public void CycleElement()
    {
        if (currentElement == Element.Physical)
        {
            if (fireAmmo > 0)
            {
                SetElement(Element.Fire);
                return;
            }

            if (waterAmmo > 0)
            {
                SetElement(Element.Water);
                return;
            }

            SetElement(Element.Physical);
            return;
        }

        if (currentElement == Element.Fire)
        {
            if (waterAmmo > 0)
            {
                SetElement(Element.Water);
                return;
            }

            SetElement(Element.Physical);
            return;
        }

        SetElement(Element.Physical);
    }

    public void SetElement(Element newElement)
    {
        if (newElement == Element.Fire && fireAmmo <= 0)
        {
            currentElement = Element.Physical;
            SaveInventory();
            RefreshUI();
            return;
        }

        if (newElement == Element.Water && waterAmmo <= 0)
        {
            currentElement = Element.Physical;
            SaveInventory();
            RefreshUI();
            return;
        }

        currentElement = newElement;
        SaveInventory();
        RefreshUI();
    }

    public void ValidateElementAfterLoad()
    {
        if (currentElement == Element.Fire && fireAmmo <= 0)
            currentElement = Element.Physical;

        if (currentElement == Element.Water && waterAmmo <= 0)
            currentElement = Element.Physical;
    }

    public bool CanShoot()
    {
        switch (currentElement)
        {
            case Element.Physical:
                if (currentAmmo <= 0 && autoRefillMagazine)
                    AutoRefillFromReserve();

                return currentAmmo > 0;

            case Element.Fire:
                return fireAmmo > 0;

            case Element.Water:
                return waterAmmo > 0;

            default:
                if (currentAmmo <= 0 && autoRefillMagazine)
                    AutoRefillFromReserve();

                return currentAmmo > 0;
        }
    }

    public void ConsumeAmmo()
    {
        switch (currentElement)
        {
            case Element.Physical:
                currentAmmo = Mathf.Max(0, currentAmmo - 1);

                if (autoRefillMagazine)
                    AutoRefillFromReserve();

                break;

            case Element.Fire:
                fireAmmo = Mathf.Max(0, fireAmmo - 1);

                if (fireAmmo <= 0)
                    currentElement = Element.Physical;

                break;

            case Element.Water:
                waterAmmo = Mathf.Max(0, waterAmmo - 1);

                if (waterAmmo <= 0)
                    currentElement = Element.Physical;

                break;
        }

        SaveInventory();
        RefreshUI();
    }

    public void Reload()
    {
        AutoRefillFromReserve();
        SaveInventory();
        RefreshUI();
    }

    private void AutoRefillFromReserve()
    {
        if (currentElement != Element.Physical)
            return;

        if (currentAmmo >= magazineSize)
            return;

        if (reserveAmmo <= 0)
            return;

        int needAmmo = magazineSize - currentAmmo;
        int ammoToLoad = Mathf.Min(needAmmo, reserveAmmo);

        currentAmmo += ammoToLoad;
        reserveAmmo -= ammoToLoad;
    }

    public float GetCurrentDamage()
    {
        float baseDamage;

        switch (currentElement)
        {
            case Element.Fire:
                baseDamage = fireDamage;
                break;

            case Element.Water:
                baseDamage = waterDamage;
                break;

            case Element.Physical:
            default:
                baseDamage = physicalDamage;
                break;
        }

        TemporaryBuffController2D buffs = playerController != null ? playerController.GetComponent<TemporaryBuffController2D>() : null;
        float multiplier = buffs != null ? buffs.GetDamageMultiplier(currentElement) : 1f;

        return baseDamage * multiplier;
    }

    public void AddAmmo(int amount)
    {
        amount = Mathf.Max(0, amount);

        if (amount <= 0)
            return;

        if (currentElement == Element.Fire && fireAmmo <= 0)
            currentElement = Element.Physical;

        if (currentElement == Element.Water && waterAmmo <= 0)
            currentElement = Element.Physical;

        reserveAmmo += amount;
        AutoRefillFromReserve();

        SaveInventory();
        RefreshUI();
    }

    public void AddReserveAmmo(int amount)
    {
        AddAmmo(amount);
    }

    public void AddCartridge(Element element, int amount)
    {
        AddElementAmmo(element, amount);
    }

    public void AddCartridge(int amount)
    {
        AddAmmo(amount);
    }

    public void AddFireAmmo(int amount)
    {
        fireAmmo += Mathf.Max(0, amount);

        SaveInventory();
        RefreshUI();
    }

    public void AddWaterAmmo(int amount)
    {
        waterAmmo += Mathf.Max(0, amount);

        SaveInventory();
        RefreshUI();
    }

    public void AddElementAmmo(Element element, int amount)
    {
        amount = Mathf.Max(0, amount);

        switch (element)
        {
            case Element.Fire:
                fireAmmo += amount;
                break;

            case Element.Water:
                waterAmmo += amount;
                break;

            case Element.Physical:
            default:
                AddAmmo(amount);
                return;
        }

        SaveInventory();
        RefreshUI();
    }

    public string GetElementNameRu()
    {
        switch (currentElement)
        {
            case Element.Fire:
                return "Огонь";

            case Element.Water:
                return "Вода";

            case Element.Steam:
                return "Пар";

            case Element.Smoldering:
                return "Тление";

            case Element.Physical:
            default:
                return "Обычный";
        }
    }

    public string GetAmmoText()
    {
        return $"Патроны: {currentAmmo}/{magazineSize} | Запас: {reserveAmmo} | Огонь: {fireAmmo} | Вода: {waterAmmo} | Перекл.: {(playerController != null && playerController.playerID == 2 ? "RShift" : "Q")}";
    }

    private void SaveInventory()
    {
        if (PlayerInventoryManager.Instance != null)
            PlayerInventoryManager.Instance.SaveFromWeapon(this);
    }

    public void RefreshUIFromOutside()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.UpdatePlayerHUD(playerController, this);
    }
}