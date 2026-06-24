using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class WeaponManager : MonoBehaviour
{
    [Header("Owner")]
    public PlayerController playerController;

    [Header("Shooting")]
    public GameObject projectilePrefab;
    public GameObject areaProjectilePrefab;
    public Transform firePoint;

    [Header("Attack Mode")]
    public AttackMode attackMode = AttackMode.Single;

    [Header("Current Element")]
    [SerializeField] private Element currentElement = Element.Physical;

    [Header("Ammo")]
    public int magazineSize = 6;
    public int currentAmmo = 6;
    public int fireAmmo = 0;
    public int waterAmmo = 0;

    [Header("Reload")]
    [SerializeField] private float reloadTime = 2f;
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
    public float baseFireCooldown = 0.35f;
    public float currentFireCooldown = 0.35f;
    public float minFireCooldown = 0.12f;

    [Header("Area Shot")]
    public float areaRadius = 2.5f;
    public LayerMask areaDamageMask;

    [Header("Debug")]
    public bool autoFillAmmoOnStart = false;
    public bool logDebug = true;

    private float lastShotTime = -999f;
    private bool loadedFromInventory = false;
    private TemporaryBuffController2D temporaryBuffs;

    public Element CurrentElement => currentElement;
    public int MagazineSize => magazineSize;
    public int CurrentAmmo => currentAmmo;
    public int FireAmmo => fireAmmo;
    public int WaterAmmo => waterAmmo;

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

        if (WeaponConfigManager.Instance != null)
            WeaponConfigManager.Instance.ApplyAttackModeOnly(this);

        ValidateElementAfterLoad();

        if (PlayerProgressManager.Instance != null && playerController != null)
            PlayerProgressManager.Instance.ApplyUpgradesToPlayer(playerController);

        RefreshUI();

        if (logDebug)
        {
            Debug.Log(
                $"WeaponManager {name}: START FINAL STATE. " +
                $"AttackMode={attackMode}, " +
                $"Element={currentElement}, " +
                $"PhysicalAmmo={currentAmmo}, " +
                $"FireAmmo={fireAmmo}, " +
                $"WaterAmmo={waterAmmo}");
        }
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

        if (currentElement == Element.Water)
        {
            SetElement(Element.Physical);
            return;
        }

        SetElement(Element.Physical);
    }

    public void SetElement(Element newElement)
    {
        if (newElement == Element.Fire)
        {
            if (fireAmmo <= 0)
            {
                Debug.LogWarning(
                    $"WeaponManager {name}: нельзя переключиться на Fire, FireAmmo={fireAmmo}");

                ValidateElementAfterLoad();
                SaveInventory();
                RefreshUI();
                return;
            }

            currentElement = Element.Fire;
            SaveInventory();
            RefreshUI();

            if (logDebug)
                Debug.Log($"WeaponManager {name}: активная стихия установлена Fire");

            return;
        }

        if (newElement == Element.Water)
        {
            if (waterAmmo <= 0)
            {
                Debug.LogWarning(
                    $"WeaponManager {name}: нельзя переключиться на Water, WaterAmmo={waterAmmo}");

                ValidateElementAfterLoad();
                SaveInventory();
                RefreshUI();
                return;
            }

            currentElement = Element.Water;
            SaveInventory();
            RefreshUI();

            if (logDebug)
                Debug.Log($"WeaponManager {name}: активная стихия установлена Water");

            return;
        }

        currentElement = Element.Physical;
        SaveInventory();
        RefreshUI();

        if (logDebug)
            Debug.Log($"WeaponManager {name}: активная стихия установлена Physical");
    }

    public void ForceSetElementAfterLoad(Element loadedElement)
    {
        currentElement = loadedElement;

        if (logDebug)
        {
            Debug.Log(
                $"WeaponManager {name}: ForceSetElementAfterLoad = {currentElement}. " +
                $"FireAmmo={fireAmmo}, WaterAmmo={waterAmmo}");
        }

        ValidateElementAfterLoad();
    }

    public void ValidateElementAfterLoad()
    {
        if (logDebug)
        {
            Debug.Log(
                $"WeaponManager {name}: ValidateElementAfterLoad BEFORE. " +
                $"CurrentElement={currentElement}, " +
                $"PhysicalAmmo={currentAmmo}, " +
                $"FireAmmo={fireAmmo}, " +
                $"WaterAmmo={waterAmmo}");
        }

        if (currentElement == Element.Fire && fireAmmo > 0)
        {
            RefreshUI();
            return;
        }

        if (currentElement == Element.Water && waterAmmo > 0)
        {
            RefreshUI();
            return;
        }

        if (fireAmmo > 0)
        {
            currentElement = Element.Fire;

            if (logDebug)
                Debug.Log($"WeaponManager {name}: после загрузки выбираю Fire, потому что FireAmmo > 0");

            RefreshUI();
            return;
        }

        if (waterAmmo > 0)
        {
            currentElement = Element.Water;

            if (logDebug)
                Debug.Log($"WeaponManager {name}: после загрузки выбираю Water, потому что WaterAmmo > 0");

            RefreshUI();
            return;
        }

        currentElement = Element.Physical;

        if (logDebug)
            Debug.Log($"WeaponManager {name}: стихийных патронов нет, выбираю Physical");

        RefreshUI();
    }

    public bool HasAmmoForElement(Element element)
    {
        switch (element)
        {
            case Element.Fire:
                return fireAmmo > 0;

            case Element.Water:
                return waterAmmo > 0;

            case Element.Physical:
                return currentAmmo > 0;

            default:
                return false;
        }
    }

    public void EnsureElementAmmo(Element element, int minimumAmount)
    {
        minimumAmount = Mathf.Max(0, minimumAmount);

        if (element == Element.Fire)
        {
            fireAmmo = Mathf.Max(fireAmmo, minimumAmount);

            if (currentElement == Element.Physical)
                currentElement = Element.Fire;
        }
        else if (element == Element.Water)
        {
            waterAmmo = Mathf.Max(waterAmmo, minimumAmount);

            if (currentElement == Element.Physical)
                currentElement = Element.Water;
        }

        ValidateElementAfterLoad();
        SaveInventory();
        RefreshUI();
    }

    public void SetAttackMode(AttackMode newAttackMode, bool saveConfig)
    {
        attackMode = newAttackMode;

        if (saveConfig && WeaponConfigManager.Instance != null)
            WeaponConfigManager.Instance.SetAttackMode(newAttackMode);

        RefreshUI();

        if (logDebug)
        {
            Debug.Log(
                $"WeaponManager {name}: режим атаки изменён на {attackMode}. " +
                $"saveConfig={saveConfig}");
        }
    }

    public bool CanShoot()
    {
        if (isReloading)
            return false;

        if (projectilePrefab == null)
        {
            Debug.LogError($"WeaponManager {name}: projectilePrefab не назначен.");
            return false;
        }

        if (firePoint == null)
        {
            Debug.LogError($"WeaponManager {name}: firePoint не назначен.");
            return false;
        }

        if (Time.time < lastShotTime + currentFireCooldown)
            return false;

        if (currentElement == Element.Fire)
            return fireAmmo > 0;

        if (currentElement == Element.Water)
            return waterAmmo > 0;

        return currentAmmo > 0;
    }

    public void Shoot(Vector2 direction, GameObject owner)
    {
        if (!CanShoot())
            return;

        if (logDebug)
        {
            Debug.Log(
                $"WeaponManager {name}: Shoot. " +
                $"AttackMode={attackMode}, " +
                $"Element={currentElement}, " +
                $"Damage={GetCurrentDamage()}");
        }

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

        if (bulletScript == null)
        {
            Debug.LogError("WeaponManager: у projectilePrefab нет BulletScript.");
            return;
        }

        bulletScript.isAoE = false;
        bulletScript.SetDirection(direction);
        bulletScript.SetDamage(GetCurrentDamage());
        bulletScript.SetElement(currentElement);
        bulletScript.SetOwner(owner);

        if (logDebug)
        {
            Debug.Log(
                $"WeaponManager {name}: создана одиночная пуля. " +
                $"Damage={GetCurrentDamage()}, Element={currentElement}");
        }
    }

    private void ShootArea(Vector2 direction, GameObject owner)
    {
        GameObject prefabToUse = areaProjectilePrefab != null ? areaProjectilePrefab : projectilePrefab;

        GameObject bullet = Instantiate(prefabToUse, firePoint.position, firePoint.rotation);
        BulletScript bulletScript = bullet.GetComponent<BulletScript>();

        if (bulletScript == null)
        {
            Debug.LogError("WeaponManager: у prefab AoE-пули нет BulletScript.");
            return;
        }

        bulletScript.isAoE = true;
        bulletScript.aoeRadius = areaRadius;
        bulletScript.damageMask = areaDamageMask;

        bulletScript.SetDirection(direction);
        bulletScript.SetDamage(GetCurrentDamage());
        bulletScript.SetElement(currentElement);
        bulletScript.SetOwner(owner);

        if (logDebug)
        {
            Debug.Log(
                $"WeaponManager {name}: создана AoE-пуля. " +
                $"Damage={GetCurrentDamage()}, Element={currentElement}, Radius={areaRadius}");
        }
    }

    public void ConsumeAmmo()
    {
        if (currentElement == Element.Fire)
        {
            fireAmmo = Mathf.Max(0, fireAmmo - 1);

            if (logDebug)
                Debug.Log($"WeaponManager {name}: потрачен огненный патрон. FireAmmo={fireAmmo}");

            if (fireAmmo <= 0)
                ValidateElementAfterLoad();

            SaveInventory();
            RefreshUI();
            return;
        }

        if (currentElement == Element.Water)
        {
            waterAmmo = Mathf.Max(0, waterAmmo - 1);

            if (logDebug)
                Debug.Log($"WeaponManager {name}: потрачен водный патрон. WaterAmmo={waterAmmo}");

            if (waterAmmo <= 0)
                ValidateElementAfterLoad();

            SaveInventory();
            RefreshUI();
            return;
        }

        currentAmmo = Mathf.Max(0, currentAmmo - 1);

        if (currentAmmo <= 0)
            StartCoroutine(ReloadRoutine());

        if (logDebug)
            Debug.Log($"WeaponManager {name}: потрачен физический патрон. CurrentAmmo={currentAmmo}");

        SaveInventory();
        RefreshUI();
    }

    public void Reload()
    {
        SaveInventory();
        RefreshUI();
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
        fireAmmo = Mathf.Max(fireAmmo, amount);

        if (currentElement == Element.Physical)
            currentElement = Element.Fire;

        SaveInventory();
        RefreshUI();
    }

    public void AddWaterAmmo(int amount)
    {
        waterAmmo = Mathf.Max(waterAmmo, amount);

        if (currentElement == Element.Physical)
            currentElement = Element.Water;

        SaveInventory();
        RefreshUI();
    }

    public void AddElementAmmo(Element element, int amount)
    {
        amount = Mathf.Max(0, amount);

        if (element == Element.Fire)
        {
            fireAmmo += amount;

            if (currentElement == Element.Physical)
                currentElement = Element.Fire;
        }
        else if (element == Element.Water)
        {
            waterAmmo += amount;

            if (currentElement == Element.Physical)
                currentElement = Element.Water;
        }

        if (logDebug)
        {
            Debug.Log(
                $"WeaponManager {name}: AddElementAmmo {element} amount={amount}. " +
                $"FireAmmo={fireAmmo}, WaterAmmo={waterAmmo}, CurrentElement={currentElement}");
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
        {
            return
                $"Перезарядка... | {attackText} | " +
                $"Стихия:{GetElementNameRu()} | " +
                $"Огонь:{fireAmmo} | Вода:{waterAmmo}";
        }

        return
            $"Патроны:{currentAmmo}/{magazineSize} | {attackText} | " +
            $"Стихия:{GetElementNameRu()} | " +
            $"Огонь:{fireAmmo} | Вода:{waterAmmo}";
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