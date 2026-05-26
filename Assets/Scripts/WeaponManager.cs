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

    // Оставлено для совместимости со старым UI.
    public bool IsReloading => false;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();

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

        AutoRefillFromReserve();
        RefreshUI();
    }

    private void Update()
    {
        HandleElementSwitch();

        if (autoRefillMagazine)
            AutoRefillFromReserve();
    }

    private void HandleElementSwitch()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        int id = playerController != null ? playerController.playerID : 1;

        if (id == 2)
        {
            if (keyboard.numpad1Key.wasPressedThisFrame)
                SetElement(Element.Physical);

            if (keyboard.numpad2Key.wasPressedThisFrame)
                SetElement(Element.Fire);

            if (keyboard.numpad3Key.wasPressedThisFrame)
                SetElement(Element.Water);
        }
        else
        {
            if (keyboard.digit1Key.wasPressedThisFrame)
                SetElement(Element.Physical);

            if (keyboard.digit2Key.wasPressedThisFrame)
                SetElement(Element.Fire);

            if (keyboard.digit3Key.wasPressedThisFrame)
                SetElement(Element.Water);
        }
    }

    public void SetElement(Element newElement)
    {
        if (newElement == Element.Fire && fireAmmo <= 0)
        {
            currentElement = Element.Physical;
            RefreshUI();
            return;
        }

        if (newElement == Element.Water && waterAmmo <= 0)
        {
            currentElement = Element.Physical;
            RefreshUI();
            return;
        }

        currentElement = newElement;
        RefreshUI();
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

        RefreshUI();
    }

    /// <summary>
    /// Полностью убираем ручную перезарядку.
    /// Метод оставлен, чтобы старые скрипты не ломались,
    /// но теперь он просто автоматически пополняет магазин из запаса.
    /// </summary>
    public void Reload()
    {
        AutoRefillFromReserve();
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
        switch (currentElement)
        {
            case Element.Fire:
                return fireDamage;

            case Element.Water:
                return waterDamage;

            case Element.Physical:
            default:
                return physicalDamage;
        }
    }

    public void AddAmmo(int amount)
    {
        amount = Mathf.Max(0, amount);

        if (amount <= 0)
            return;

        // Если игрок был на элементе без патронов, возвращаем обычный режим.
        if (currentElement == Element.Fire && fireAmmo <= 0)
            currentElement = Element.Physical;

        if (currentElement == Element.Water && waterAmmo <= 0)
            currentElement = Element.Physical;

        // Сначала добавляем в запас.
        reserveAmmo += amount;

        // Потом сразу автоматически пополняем магазин.
        AutoRefillFromReserve();

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
        RefreshUI();
    }

    public void AddWaterAmmo(int amount)
    {
        waterAmmo += Mathf.Max(0, amount);
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
        return $"Патроны: {currentAmmo}/{magazineSize} | Запас: {reserveAmmo} | Огонь: {fireAmmo} | Вода: {waterAmmo}";
    }

    private void RefreshUI()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.UpdatePlayerHUD(playerController, this);
    }
}