using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class WeaponManager : MonoBehaviour
{
    [Header("Owner")]
    public PlayerController playerController;

    [Header("Current Element")]
    public Element currentElement = Element.Physical;

    [Header("Ammo")]
    public int magazineSize = 6;
    public int currentAmmo = 6;
    public int fireAmmo = 0;
    public int waterAmmo = 0;

    [SerializeField]
    private float reloadTime = 2f;

    private bool isReloading;
    public bool IsReloading => isReloading;

    [Header("Single Shot Damage")]
    public float basePhysicalSingleDamage = 10f;
    public float baseFireSingleDamage = 8f;
    public float baseWaterSingleDamage = 8f;

    [Header("Area Shot Damage")]
    public float basePhysicalAreaDamage = 7f;
    public float baseFireAreaDamage = 6f;
    public float baseWaterAreaDamage = 6f;

    [Header("Current Damage After Upgrades")]
    public float physicalDamage = 10f;
    public float fireDamage = 8f;
    public float waterDamage = 8f;
    public float areaPhysicalDamage = 7f;
    public float areaFireDamage = 6f;
    public float areaWaterDamage = 6f;

    [Header("Fire Rate")]
    [Tooltip("Базовая пауза между выстрелами. Чем меньше значение, тем быстрее стрельба.")]
    public float baseFireCooldown = 0.35f;
    public float currentFireCooldown = 0.35f;
    public float minFireCooldown = 0.12f;

    private float lastShotTime = -999f;

    [Header("Area Shot")]
    public float areaRadius = 2.5f;

    [Header("Debug")]
    public bool autoFillAmmoOnStart = false;

    [Header("Shooting")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    public Element CurrentElement => currentElement;
    public int MagazineSize => magazineSize;
    public int CurrentAmmo => currentAmmo;
    public int FireAmmo => fireAmmo;
    public int WaterAmmo => waterAmmo;

    private bool loadedFromInventory;
    private TemporaryBuffController2D temporaryBuffs;

    public AttackMode attackMode = AttackMode.Single;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        temporaryBuffs = playerController != null
            ? playerController.GetComponent<TemporaryBuffController2D>()
            : GetComponent<TemporaryBuffController2D>();

        if (magazineSize <= 0)
            magazineSize = 6;

        currentAmmo = Mathf.Clamp(currentAmmo, 0, magazineSize);

        if (baseFireCooldown <= 0f)
            baseFireCooldown = 0.35f;

        currentFireCooldown = Mathf.Max(minFireCooldown, baseFireCooldown);

        if (autoFillAmmoOnStart)
        {
            currentAmmo = magazineSize;
            fireAmmo = Mathf.Max(fireAmmo, 20);
            waterAmmo = Mathf.Max(waterAmmo, 20);
        }
    }

    private void Start()
    {
        LoadInventoryIfPossible();

        if (PlayerProgressManager.Instance != null)
            PlayerProgressManager.Instance.ApplyUpgradesToPlayer(playerController);

        ValidateElementAfterLoad();
        RefreshUI();
    }

    private void Update()
    {
        LoadInventoryIfPossible();
        HandleElementCycle();
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
            if (keyboard.uKey.wasPressedThisFrame)
                CycleElement();
        }
        else
        {
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

    public void EnsureElementAmmo(Element element, int minimumAmount)
    {
        minimumAmount = Mathf.Max(0, minimumAmount);

        if (element == Element.Fire)
            fireAmmo = Mathf.Max(fireAmmo, minimumAmount);
        else if (element == Element.Water)
            waterAmmo = Mathf.Max(waterAmmo, minimumAmount);

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
        if (isReloading)
            return false;

        if (Time.time < lastShotTime + currentFireCooldown)
            return false;

        switch (currentElement)
        {
            case Element.Physical:
                return currentAmmo > 0;

            case Element.Fire:
                return fireAmmo > 0;

            case Element.Water:
                return waterAmmo > 0;

            default:
                return currentAmmo > 0;
        }
    }

    public void ConsumeAmmo()
    {
        switch (currentElement)
        {
            case Element.Physical:
                currentAmmo--;

                if (currentAmmo <= 0)
                {
                    currentAmmo = 0;
                    StartCoroutine(ReloadRoutine());
                }

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
        SaveInventory();
        RefreshUI();
    }

    public float GetCurrentDamage()
    {
        bool areaShot = attackMode == AttackMode.Area;
        float baseDamage;

        switch (currentElement)
        {
            case Element.Fire:
                baseDamage = areaShot ? areaFireDamage : fireDamage;
                break;

            case Element.Water:
                baseDamage = areaShot ? areaWaterDamage : waterDamage;
                break;

            case Element.Physical:
            default:
                baseDamage = areaShot ? areaPhysicalDamage : physicalDamage;
                break;
        }

        TemporaryBuffController2D buffs = playerController != null
            ? playerController.GetComponent<TemporaryBuffController2D>()
            : temporaryBuffs;

        float multiplier = buffs != null ? buffs.GetDamageMultiplier(currentElement) : 1f;
        return baseDamage * multiplier;
    }

    public void ApplyPermanentUpgrades(float singleDamageBonus, float areaDamageBonus, float fireCooldownMultiplier)
    {
        physicalDamage = basePhysicalSingleDamage + singleDamageBonus;
        fireDamage = baseFireSingleDamage + singleDamageBonus;
        waterDamage = baseWaterSingleDamage + singleDamageBonus;

        areaPhysicalDamage = basePhysicalAreaDamage + areaDamageBonus;
        areaFireDamage = baseFireAreaDamage + areaDamageBonus;
        areaWaterDamage = baseWaterAreaDamage + areaDamageBonus;

        fireCooldownMultiplier = Mathf.Clamp(fireCooldownMultiplier, 0.1f, 1f);
        currentFireCooldown = Mathf.Max(minFireCooldown, baseFireCooldown * fireCooldownMultiplier);

        RefreshUI();
    }

    public void AddCartridge(Element element, int amount)
    {
        AddElementAmmo(element, amount);
    }

    public void AddFireAmmo(int amount)
    {
        fireAmmo = magazineSize;
        SaveInventory();
        RefreshUI();
    }

    public void AddWaterAmmo(int amount)
    {
        waterAmmo = magazineSize;
        SaveInventory();
        RefreshUI();
    }

    public void AddElementAmmo(Element element, int amount)
    {
        Debug.Log($"PICKUP {element} amount={amount}");
        amount = Mathf.Max(0, amount);

        switch (element)
        {
            case Element.Fire:
                fireAmmo += amount;
                break;

            case Element.Water:
                waterAmmo += amount;
                break;
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

    public string GetAttackModeNameRu()
    {
        return attackMode == AttackMode.Area ? "по площади" : "одиночный";
    }

    public string GetAmmoText()
    {
        string attackText = attackMode == AttackMode.Area ? "AoE" : "Single";

        if (isReloading)
            return $"Перезарядка... | {attackText} | Огонь:{fireAmmo} | Вода:{waterAmmo}";

        return $"Патроны:{currentAmmo}/{magazineSize} | {attackText} | Огонь:{fireAmmo} | Вода:{waterAmmo}";
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

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        RefreshUI();

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
        isReloading = false;

        SaveInventory();
        RefreshUI();
    }

    public void Shoot(Vector2 direction, GameObject owner)
    {
        if (!CanShoot())
            return;

        if (attackMode == AttackMode.Single)
            ShootSingle(direction, owner);
        else
            ShootArea(direction, owner);

        lastShotTime = Time.time;
        ConsumeAmmo();
    }

    private void ShootSingle(Vector2 direction, GameObject owner)
    {
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        BulletScript bulletScript = bullet.GetComponent<BulletScript>();

        if (bulletScript != null)
        {
            bulletScript.isAoE = false;
            bulletScript.SetDirection(direction);
            bulletScript.SetDamage(GetCurrentDamage());
            bulletScript.SetElement(CurrentElement);
            bulletScript.SetOwner(owner);
        }
    }

    private void ShootArea(Vector2 direction, GameObject owner)
    {
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        BulletScript bulletScript = bullet.GetComponent<BulletScript>();

        if (bulletScript != null)
        {
            bulletScript.isAoE = true;
            bulletScript.aoeRadius = areaRadius;
            bulletScript.SetDirection(direction);
            bulletScript.SetDamage(GetCurrentDamage());
            bulletScript.SetElement(CurrentElement);
            bulletScript.SetOwner(owner);
        }
    }
}
