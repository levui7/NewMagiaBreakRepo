using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LobbyShopUIController : MonoBehaviour
{
    [Header("Texts")]
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI helpText;
    public TextMeshProUGUI resultText;

    [Header("Buttons")]
    public Button damageButton;
    public Button healthButton;
    public Button speedButton;
    public Button closeButton;

    [Header("Panel")]
    public GameObject shopPanel;

    private void Start()
    {
        RefreshUI();

        if (damageButton != null)
            damageButton.onClick.AddListener(BuyDamage);

        if (healthButton != null)
            healthButton.onClick.AddListener(BuyHealth);

        if (speedButton != null)
            speedButton.onClick.AddListener(BuySpeed);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseShop);

        shopPanel.SetActive(false);
    }

    public void OpenShop()
{
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

    public void BuyDamage()
    {
        bool ok = PlayerProgressManager.Instance != null && PlayerProgressManager.Instance.UpgradeDamage();
        ShowResult(ok, "Урон улучшен");
    }

    public void BuyHealth()
    {
        bool ok = PlayerProgressManager.Instance != null && PlayerProgressManager.Instance.UpgradeHealth();
        ShowResult(ok, "Здоровье улучшено");
    }

    public void BuySpeed()
    {
        bool ok = PlayerProgressManager.Instance != null && PlayerProgressManager.Instance.UpgradeSpeed();
        ShowResult(ok, "Скорость улучшена");
    }

    private void ShowResult(bool success, string successText)
    {
        if (resultText != null)
            resultText.text = success ? successText : "Недостаточно ресурсов";

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
                "Улучшения действуют на все последующие забеги";
        }
    }
}