using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LobbyShopUIController : MonoBehaviour
{
    [Header("Texts")]
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI helpText;
    public TextMeshProUGUI resultText;

    [Header("New Weapon Upgrade Buttons")]
    public Button singleDamageButton;
    public Button areaDamageButton;
    public Button fireRateButton;

    [Header("Old / Other Upgrade Buttons")]
    public Button damageButton;
    public Button healthButton;
    public Button speedButton;
    public Button closeButton;

    [Header("Panel")]
    public GameObject shopPanel;

    private void Start()
    {
        RefreshUI();

        if (singleDamageButton != null)
            singleDamageButton.onClick.AddListener(BuySingleDamage);

        if (areaDamageButton != null)
            areaDamageButton.onClick.AddListener(BuyAreaDamage);

        if (fireRateButton != null)
            fireRateButton.onClick.AddListener(BuyFireRate);

        // Старую кнопку урона можно оставить: она будет покупать одиночный урон.
        if (damageButton != null)
            damageButton.onClick.AddListener(BuySingleDamage);

        if (healthButton != null)
            healthButton.onClick.AddListener(BuyHealth);

        if (speedButton != null)
            speedButton.onClick.AddListener(BuySpeed);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseShop);

        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    public void OpenShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(true);

        Time.timeScale = 0f;
        RefreshUI();
    }

    public void CloseShop()
    {
        Time.timeScale = 1f;

        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    public void BuySingleDamage()
    {
        bool ok = PlayerProgressManager.Instance != null && PlayerProgressManager.Instance.UpgradeSingleDamage();
        ShowResult(ok, "Урон одиночного выстрела улучшен");
    }

    public void BuyAreaDamage()
    {
        bool ok = PlayerProgressManager.Instance != null && PlayerProgressManager.Instance.UpgradeAreaDamage();
        ShowResult(ok, "Урон выстрела по площади улучшен");
    }

    public void BuyFireRate()
    {
        bool ok = PlayerProgressManager.Instance != null && PlayerProgressManager.Instance.UpgradeFireRate();
        ShowResult(ok, "Скорострельность улучшена");
    }

    public void BuyDamage()
    {
        BuySingleDamage();
    }

    public void BuyHealth()
    {
        bool ok = PlayerProgressManager.Instance != null && PlayerProgressManager.Instance.UpgradeHealth();
        ShowResult(ok, "Здоровье улучшено");
    }

    public void BuySpeed()
    {
        bool ok = PlayerProgressManager.Instance != null && PlayerProgressManager.Instance.UpgradeSpeed();
        ShowResult(ok, "Скорость передвижения улучшена");
    }

    private void ShowResult(bool success, string successText)
    {
        if (resultText != null)
            resultText.text = success ? successText : "Недостаточно ресурсов или достигнут максимум";

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (PlayerProgressManager.Instance == null)
            return;

        if (progressText != null)
            progressText.text = PlayerProgressManager.Instance.GetProgressText();

        if (helpText != null)
        {
            helpText.text =
                "Улучшения действуют на текущее оружие и следующие забеги\n\n" +
                PlayerProgressManager.Instance.GetUpgradeHelpText();
        }
    }
}
