using System;
using UnityEngine;

public class PlayerProgressManager : MonoBehaviour
{
    public static PlayerProgressManager Instance { get; private set; }

    public enum MaterialType
    {
        Coins,
        Crystals
    }

    [Header("Resources")]
    public int coins = 0;
    public int crystals = 0;

    [Header("Upgrade Levels")]
    public int damageLevel = 0;
    public int healthLevel = 0;
    public int speedLevel = 0;

    [Header("Upgrade Limits")]
    public int maxDamageLevel = 5;
    public int maxHealthLevel = 5;
    public int maxSpeedLevel = 5;

    [Header("Upgrade Bonuses")]
    public float damageBonusPerLevel = 2f;
    public float healthBonusPerLevel = 20f;
    public float speedBonusPerLevel = 0.35f;

    [Header("Saving")]
    public bool loadOnAwake = true;
    public bool saveOnEveryChange = true;

    public event Action OnProgressChanged;

    private const string CoinsKey = "Progress_Coins";
    private const string CrystalsKey = "Progress_Crystals";
    private const string DamageLevelKey = "Progress_DamageLevel";
    private const string HealthLevelKey = "Progress_HealthLevel";
    private const string SpeedLevelKey = "Progress_SpeedLevel";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (loadOnAwake)
            LoadProgress();
    }

    public void AddMaterial(MaterialType type, int amount)
    {
        amount = Mathf.Max(0, amount);

        if (amount <= 0)
            return;

        switch (type)
        {
            case MaterialType.Coins:
                coins += amount;
                break;

            case MaterialType.Crystals:
                crystals += amount;
                break;
        }

        if (saveOnEveryChange)
            SaveProgress();

        OnProgressChanged?.Invoke();

        if (MaterialsHUD2D.Instance != null)
            MaterialsHUD2D.Instance.Refresh();
    }

    public bool TrySpend(int coinCost, int crystalCost)
    {
        coinCost = Mathf.Max(0, coinCost);
        crystalCost = Mathf.Max(0, crystalCost);

        if (coins < coinCost)
            return false;

        if (crystals < crystalCost)
            return false;

        coins -= coinCost;
        crystals -= crystalCost;

        if (saveOnEveryChange)
            SaveProgress();

        OnProgressChanged?.Invoke();

        if (MaterialsHUD2D.Instance != null)
            MaterialsHUD2D.Instance.Refresh();

        return true;
    }

    public bool UpgradeDamage()
    {
        if (damageLevel >= maxDamageLevel)
            return false;

        int coinCost = GetDamageCoinCost();
        int crystalCost = GetDamageCrystalCost();

        if (!TrySpend(coinCost, crystalCost))
            return false;

        damageLevel++;
        ApplyUpgradesToAllPlayers();

        if (saveOnEveryChange)
            SaveProgress();

        OnProgressChanged?.Invoke();

        return true;
    }

    public bool UpgradeHealth()
    {
        if (healthLevel >= maxHealthLevel)
            return false;

        int coinCost = GetHealthCoinCost();
        int crystalCost = GetHealthCrystalCost();

        if (!TrySpend(coinCost, crystalCost))
            return false;

        healthLevel++;
        ApplyUpgradesToAllPlayers();

        if (saveOnEveryChange)
            SaveProgress();

        OnProgressChanged?.Invoke();

        return true;
    }

    public bool UpgradeSpeed()
    {
        if (speedLevel >= maxSpeedLevel)
            return false;

        int coinCost = GetSpeedCoinCost();
        int crystalCost = GetSpeedCrystalCost();

        if (!TrySpend(coinCost, crystalCost))
            return false;

        speedLevel++;
        ApplyUpgradesToAllPlayers();

        if (saveOnEveryChange)
            SaveProgress();

        OnProgressChanged?.Invoke();

        return true;
    }

    public void ApplyUpgradesToAllPlayers()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach (PlayerController player in players)
        {
            if (player != null)
                ApplyUpgradesToPlayer(player);
        }
    }

    public void ApplyUpgradesToPlayer(PlayerController player)
    {
        if (player == null)
            return;

        player.moveSpeed = 5f + speedLevel * speedBonusPerLevel;

        bool wasFullHealth = player.currentHealth >= player.maxHealth - 0.01f;

        player.maxHealth = 100f + healthLevel * healthBonusPerLevel;

        if (wasFullHealth)
            player.currentHealth = player.maxHealth;
        else
            player.currentHealth = Mathf.Clamp(player.currentHealth, 0f, player.maxHealth);

        WeaponManager weapon = player.GetComponent<WeaponManager>();

        if (weapon != null)
        {
            weapon.physicalDamage = 10f + damageLevel * damageBonusPerLevel;
            weapon.fireDamage = 8f + damageLevel * damageBonusPerLevel;
            weapon.waterDamage = 8f + damageLevel * damageBonusPerLevel;
        }

        if (UIManager.Instance != null)
            UIManager.Instance.UpdatePlayerHUD(player, weapon);
    }

    public float GetEnemyHealthMultiplier()
    {
        return 1f
               + damageLevel * 0.15f
               + healthLevel * 0.25f
               + speedLevel * 0.10f;
    }

    public float GetEnemyDamageMultiplier()
    {
        return 1f
               + damageLevel * 0.12f
               + healthLevel * 0.18f
               + speedLevel * 0.08f;
    }

    public float GetEnemySpeedMultiplier()
    {
        return 1f
               + damageLevel * 0.05f
               + healthLevel * 0.05f
               + speedLevel * 0.12f;
    }

    public int GetDamageCoinCost()
    {
        return 10 + damageLevel * 8;
    }

    public int GetDamageCrystalCost()
    {
        return damageLevel >= 2 ? 1 + damageLevel : 0;
    }

    public int GetHealthCoinCost()
    {
        return 12 + healthLevel * 10;
    }

    public int GetHealthCrystalCost()
    {
        return healthLevel >= 2 ? 1 + healthLevel : 0;
    }

    public int GetSpeedCoinCost()
    {
        return 8 + speedLevel * 8;
    }

    public int GetSpeedCrystalCost()
    {
        return speedLevel >= 3 ? 1 + speedLevel : 0;
    }

    public string GetProgressText()
    {
        return
            $"ћонеты: {coins} |  ристаллы: {crystals}\n" +
            $"”рон: {damageLevel}/{maxDamageLevel} | HP: {healthLevel}/{maxHealthLevel} | —корость: {speedLevel}/{maxSpeedLevel}";
    }

    public string GetUpgradeHelpText()
    {
        return
            $"F Ч ”рон: {GetDamageCostText()}\n" +
            $"G Ч «доровье: {GetHealthCostText()}\n" +
            $"H Ч —корость: {GetSpeedCostText()}";
    }

    public string GetDamageCostText()
    {
        if (damageLevel >= maxDamageLevel)
            return "максимум";

        return $"{GetDamageCoinCost()} монет, {GetDamageCrystalCost()} крист.";
    }

    public string GetHealthCostText()
    {
        if (healthLevel >= maxHealthLevel)
            return "максимум";

        return $"{GetHealthCoinCost()} монет, {GetHealthCrystalCost()} крист.";
    }

    public string GetSpeedCostText()
    {
        if (speedLevel >= maxSpeedLevel)
            return "максимум";

        return $"{GetSpeedCoinCost()} монет, {GetSpeedCrystalCost()} крист.";
    }

    public void SaveProgress()
    {
        PlayerPrefs.SetInt(CoinsKey, coins);
        PlayerPrefs.SetInt(CrystalsKey, crystals);
        PlayerPrefs.SetInt(DamageLevelKey, damageLevel);
        PlayerPrefs.SetInt(HealthLevelKey, healthLevel);
        PlayerPrefs.SetInt(SpeedLevelKey, speedLevel);
        PlayerPrefs.Save();
    }

    public void LoadProgress()
    {
        coins = PlayerPrefs.GetInt(CoinsKey, coins);
        crystals = PlayerPrefs.GetInt(CrystalsKey, crystals);

        damageLevel = PlayerPrefs.GetInt(DamageLevelKey, damageLevel);
        healthLevel = PlayerPrefs.GetInt(HealthLevelKey, healthLevel);
        speedLevel = PlayerPrefs.GetInt(SpeedLevelKey, speedLevel);

        OnProgressChanged?.Invoke();

        if (MaterialsHUD2D.Instance != null)
            MaterialsHUD2D.Instance.Refresh();
    }

    public void ResetProgress()
    {
        coins = 0;
        crystals = 0;

        damageLevel = 0;
        healthLevel = 0;
        speedLevel = 0;

        SaveProgress();

        OnProgressChanged?.Invoke();

        if (MaterialsHUD2D.Instance != null)
            MaterialsHUD2D.Instance.Refresh();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveProgress();
    }

    private void OnApplicationQuit()
    {
        SaveProgress();
    }
}