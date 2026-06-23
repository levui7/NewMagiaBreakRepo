using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuffShopUI : MonoBehaviour
{
    [Header("Данные магазина")]
    public BuffItem[] shopItems;           // Список товаров

    [Header("UI")]
    public GameObject slotPrefab;          // Префаб слота товара
    public Transform slotsContainer;       // Родитель для слотов (Layout Group)
    public TextMeshProUGUI coinsText;      // Текст с монетами
    public TextMeshProUGUI crystalsText;   // Текст с кристаллами
    public TextMeshProUGUI titleText;      // Заголовок магазина
    public GameObject shopPanel;           // Панель магазина

    [Header("Звуки")]
    public AudioClip buySound;
    public AudioClip errorSound;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        PopulateShop();
        RefreshCurrency();

        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    /// <summary>
    /// Заполняет магазин товарами
    /// </summary>
    public void PopulateShop()
    {
        // Очищаем старые слоты
        foreach (Transform child in slotsContainer)
            Destroy(child.gameObject);

        // Создаём слоты для каждого товара
        foreach (BuffItem item in shopItems)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsContainer);
            BuffShopSlot slot = slotObj.GetComponent<BuffShopSlot>();

            if (slot != null)
                slot.Setup(item, this);
        }
    }

    /// <summary>
    /// Покупает баф
    /// </summary>
    public void BuyBuff(BuffItem item)
    {
        if (!item.CanAfford())
        {
            PlaySound(errorSound);
            return;
        }

        // Списываем деньги
        PlayerProgressManager.Instance.TrySpend(item.coinCost, item.crystalCost);

        // Применяем баф ко всем игрокам
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in players)
        {
            TemporaryBuffController2D buffs = player.GetComponent<TemporaryBuffController2D>();
            if (buffs == null)
                buffs = player.gameObject.AddComponent<TemporaryBuffController2D>();

            buffs.AddBuff(item.buffType, item.duration, item.multiplier);
        }

        PlaySound(buySound);
        RefreshCurrency();
    }

    /// <summary>
    /// Обновляет отображение валюты
    /// </summary>
    public void RefreshCurrency()
    {
        if (PlayerProgressManager.Instance == null) return;

        if (coinsText != null)
            coinsText.text = $"🪙 {PlayerProgressManager.Instance.coins}";

        if (crystalsText != null)
            crystalsText.text = $"💎 {PlayerProgressManager.Instance.crystals}";
    }

    public void OpenShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(true);
        Time.timeScale = 0f;
        RefreshCurrency();
    }

    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}