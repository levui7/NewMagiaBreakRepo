using UnityEngine;

[System.Serializable]
public class BuffItem
{
    [Header("Основное")]
    public string buffName = "Пиво";
    public string description = "Увеличивает урон на 30 сек";
    public Sprite icon;                    // Иконка пива (можно анимированную)
    public Sprite[] iconAnimationFrames;   // Кадры анимации (опционально)

    [Header("Цена")]
    public int coinCost = 10;
    public int crystalCost = 0;

    [Header("Эффект бафа")]
    public TemporaryBuffController2D.BuffType buffType = TemporaryBuffController2D.BuffType.Power;
    public float duration = 30f;           // Длительность в секундах
    public float multiplier = 1.3f;        // Множитель усиления

    [Header("Визуал")]
    public Color rarityColor = Color.yellow; // Цвет рамки (жёлтый для обычного)

    public bool CanAfford()
    {
        if (PlayerProgressManager.Instance == null) return false;
        return PlayerProgressManager.Instance.coins >= coinCost &&
               PlayerProgressManager.Instance.crystals >= crystalCost;
    }
}