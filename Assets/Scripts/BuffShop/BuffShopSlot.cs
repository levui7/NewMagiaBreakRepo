using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuffShopSlot : MonoBehaviour
{
    [Header("UI элементы")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI priceText;
    public Button buyButton;
    public Image borderImage;            // Рамка (цвет зависит от редкости)

    [Header("Анимация иконки")]
    public float animationSpeed = 5f;    // Скорость смены кадров
    public bool animateIcon = true;

    private BuffItem currentItem;
    private BuffShopUI shop;
    private int currentFrame = 0;
    private float animationTimer = 0f;

    /// <summary>
    /// Настраивает слот
    /// </summary>
    public void Setup(BuffItem item, BuffShopUI shopRef)
    {
        currentItem = item;
        shop = shopRef;

        // Название и описание
        if (nameText != null) nameText.text = item.buffName;
        if (descriptionText != null) descriptionText.text = item.description;

        // Цена
        if (priceText != null)
        {
            string price = $"🪙 {item.coinCost}";
            if (item.crystalCost > 0)
                price += $" 💎 {item.crystalCost}";
            priceText.text = price;
        }

        // Иконка
        if (iconImage != null && item.icon != null)
            iconImage.sprite = item.icon;

        // Цвет рамки по редкости
        if (borderImage != null)
            borderImage.color = item.rarityColor;

        // Кнопка покупки
        if (buyButton != null)
            buyButton.onClick.AddListener(OnBuyClicked);

        UpdateButtonState();
    }

    private void Update()
    {
        // Анимация иконки (смена кадров)
        if (animateIcon && currentItem != null &&
            currentItem.iconAnimationFrames != null &&
            currentItem.iconAnimationFrames.Length > 1 &&
            iconImage != null)
        {
            animationTimer += Time.unscaledDeltaTime * animationSpeed;

            if (animationTimer >= 1f)
            {
                animationTimer = 0f;
                currentFrame = (currentFrame + 1) % currentItem.iconAnimationFrames.Length;
                iconImage.sprite = currentItem.iconAnimationFrames[currentFrame];
            }
        }

        // Обновляем доступность кнопки
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        if (buyButton != null && currentItem != null)
            buyButton.interactable = currentItem.CanAfford();
    }

    private void OnBuyClicked()
    {
        if (shop != null && currentItem != null)
            shop.BuyBuff(currentItem);
    }
}