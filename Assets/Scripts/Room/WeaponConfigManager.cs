using UnityEngine;

public class WeaponConfigManager : MonoBehaviour
{
    public static WeaponConfigManager Instance { get; private set; }

    [Header("Выбранный режим атаки")]
    public AttackMode selectedAttackMode = AttackMode.Single;

    [Header("Стартовая стихия забега")]
    public Element selectedInitialElement = Element.Fire;

    [Header("Стартовые стихийные патроны")]
    public int startingFireAmmo = 6;
    public int startingWaterAmmo = 6;

    private const string AttackModeKey = "WeaponConfig_AttackMode";
    private const string InitialElementKey = "WeaponConfig_InitialElement";
    private const string FireAmmoKey = "WeaponConfig_StartingFireAmmo";
    private const string WaterAmmoKey = "WeaponConfig_StartingWaterAmmo";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadConfig();
    }

    public void SetAttackMode(AttackMode mode)
    {
        selectedAttackMode = mode;
        SaveConfig();
    }

    public void SetInitialElement(Element element)
    {
        if (element != Element.Fire && element != Element.Water)
            element = Element.Fire;

        selectedInitialElement = element;
        SaveConfig();
    }

    public void ApplyConfigToCurrentWeapons()
    {
        WeaponManager[] weapons = FindObjectsOfType<WeaponManager>();

        foreach (WeaponManager weapon in weapons)
        {
            if (weapon == null || !weapon.gameObject.activeInHierarchy)
                continue;

            ApplyConfigToWeapon(weapon, true);
        }
    }

    public void ApplyAttackModeOnly(WeaponManager weapon)
    {
        if (weapon == null)
            return;

        weapon.attackMode = selectedAttackMode;
    }

    public void ApplyConfigToWeapon(WeaponManager weapon, bool applyInitialElement)
    {
        if (weapon == null)
            return;

        weapon.attackMode = selectedAttackMode;

        if (!applyInitialElement)
            return;

        if (selectedInitialElement == Element.Fire)
        {
            weapon.EnsureElementAmmo(Element.Fire, startingFireAmmo);
            weapon.SetElement(Element.Fire);
        }
        else if (selectedInitialElement == Element.Water)
        {
            weapon.EnsureElementAmmo(Element.Water, startingWaterAmmo);
            weapon.SetElement(Element.Water);
        }

        if (PlayerInventoryManager.Instance != null)
            PlayerInventoryManager.Instance.SaveFromWeapon(weapon);
    }

    public string GetAttackModeNameRu()
    {
        return selectedAttackMode == AttackMode.Area ? "По площади" : "Одиночный";
    }

    public string GetInitialElementNameRu()
    {
        return selectedInitialElement == Element.Water ? "Вода" : "Огонь";
    }

    public void SaveConfig()
    {
        PlayerPrefs.SetInt(AttackModeKey, (int)selectedAttackMode);
        PlayerPrefs.SetInt(InitialElementKey, (int)selectedInitialElement);
        PlayerPrefs.SetInt(FireAmmoKey, Mathf.Max(0, startingFireAmmo));
        PlayerPrefs.SetInt(WaterAmmoKey, Mathf.Max(0, startingWaterAmmo));
        PlayerPrefs.Save();
    }

    public void LoadConfig()
    {
        selectedAttackMode = IntToAttackMode(PlayerPrefs.GetInt(AttackModeKey, (int)selectedAttackMode));
        selectedInitialElement = IntToElement(PlayerPrefs.GetInt(InitialElementKey, (int)selectedInitialElement));
        startingFireAmmo = Mathf.Max(0, PlayerPrefs.GetInt(FireAmmoKey, startingFireAmmo));
        startingWaterAmmo = Mathf.Max(0, PlayerPrefs.GetInt(WaterAmmoKey, startingWaterAmmo));

        if (selectedInitialElement != Element.Fire && selectedInitialElement != Element.Water)
            selectedInitialElement = Element.Fire;
    }

    private AttackMode IntToAttackMode(int value)
    {
        if (System.Enum.IsDefined(typeof(AttackMode), value))
            return (AttackMode)value;

        return AttackMode.Single;
    }

    private Element IntToElement(int value)
    {
        if (System.Enum.IsDefined(typeof(Element), value))
            return (Element)value;

        return Element.Fire;
    }
}
