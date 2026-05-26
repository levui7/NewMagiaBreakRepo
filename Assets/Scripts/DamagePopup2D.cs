using UnityEngine;
using TMPro;

/// <summary>
/// Универсальный всплывающий текст для 2D-проекта:
/// урон, лечение, статусы, служебные сообщения.
/// 
/// Важно:
/// этот класс специально содержит несколько перегрузок SpawnDamage / SpawnHeal / SpawnStatus,
/// чтобы быть совместимым с PlayerController, Enemy, BossController и StatusEffectController.
/// </summary>
public class DamagePopup2D : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField] private float moveSpeed = 1.2f;
    [SerializeField] private float lifeTime = 0.85f;
    [SerializeField] private float fadeStartTime = 0.35f;

    private TextMeshPro textMesh;
    private Color startColor;
    private float timer;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();

        if (textMesh == null)
            textMesh = gameObject.AddComponent<TextMeshPro>();

        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.sortingOrder = 500;
        textMesh.enableWordWrapping = false;
        textMesh.raycastTarget = false;

        startColor = textMesh.color;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        if (timer >= fadeStartTime)
        {
            float fadeT = Mathf.InverseLerp(fadeStartTime, lifeTime, timer);
            Color c = startColor;
            c.a = Mathf.Lerp(startColor.a, 0f, fadeT);

            if (textMesh != null)
                textMesh.color = c;
        }

        if (timer >= lifeTime)
            Destroy(gameObject);
    }

    // ---------------------------------------------------------------------
    // БАЗОВЫЙ МЕТОД
    // ---------------------------------------------------------------------

    public static DamagePopup2D SpawnText(Vector3 worldPosition, string text, Color color, float fontSize = 4f)
    {
        GameObject popupObject = new GameObject("DamagePopup2D");
        popupObject.transform.position = worldPosition + new Vector3(0f, 0.65f, 0f);

        TextMeshPro tmp = popupObject.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.sortingOrder = 500;
        tmp.enableWordWrapping = false;
        tmp.raycastTarget = false;

        DamagePopup2D popup = popupObject.AddComponent<DamagePopup2D>();
        popup.startColor = color;

        return popup;
    }

    // ---------------------------------------------------------------------
    // УРОН
    // ---------------------------------------------------------------------

    public static DamagePopup2D SpawnDamage(Vector3 worldPosition, int amount)
    {
        return SpawnDamage(worldPosition, (float)amount, null);
    }

    public static DamagePopup2D SpawnDamage(Vector3 worldPosition, float amount)
    {
        return SpawnDamage(worldPosition, amount, null);
    }

    public static DamagePopup2D SpawnDamage(Vector3 worldPosition, int amount, object elementOrColor)
    {
        return SpawnDamage(worldPosition, (float)amount, elementOrColor);
    }

    public static DamagePopup2D SpawnDamage(Vector3 worldPosition, float amount, object elementOrColor)
    {
        Color color = ResolveDamageColor(elementOrColor);
        string text = "-" + Mathf.RoundToInt(amount).ToString();
        return SpawnText(worldPosition, text, color, 4f);
    }

    // На случай, если в каком-то скрипте передаётся Color напрямую.
    public static DamagePopup2D SpawnDamage(Vector3 worldPosition, int amount, Color color)
    {
        return SpawnText(worldPosition, "-" + amount.ToString(), color, 4f);
    }

    public static DamagePopup2D SpawnDamage(Vector3 worldPosition, float amount, Color color)
    {
        return SpawnText(worldPosition, "-" + Mathf.RoundToInt(amount).ToString(), color, 4f);
    }

    // ---------------------------------------------------------------------
    // ЛЕЧЕНИЕ
    // ---------------------------------------------------------------------

    public static DamagePopup2D SpawnHeal(Vector3 worldPosition, int amount)
    {
        return SpawnHeal(worldPosition, (float)amount);
    }

    public static DamagePopup2D SpawnHeal(Vector3 worldPosition, float amount)
    {
        return SpawnText(worldPosition, "+" + Mathf.RoundToInt(amount).ToString(), new Color(0.35f, 1f, 0.35f), 4f);
    }

    public static DamagePopup2D SpawnHeal(Vector3 worldPosition, int amount, Color color)
    {
        return SpawnText(worldPosition, "+" + amount.ToString(), color, 4f);
    }

    public static DamagePopup2D SpawnHeal(Vector3 worldPosition, float amount, Color color)
    {
        return SpawnText(worldPosition, "+" + Mathf.RoundToInt(amount).ToString(), color, 4f);
    }

    // ---------------------------------------------------------------------
    // СТАТУСЫ
    // ---------------------------------------------------------------------

    public static DamagePopup2D SpawnStatus(Vector3 worldPosition, string statusName)
    {
        return SpawnStatus(worldPosition, statusName, ResolveStatusColor(statusName));
    }

    public static DamagePopup2D SpawnStatus(Vector3 worldPosition, string statusName, Color color)
    {
        return SpawnText(worldPosition + new Vector3(0f, 0.35f, 0f), statusName, color, 3.2f);
    }

    public static DamagePopup2D SpawnStatus(Vector3 worldPosition, object status)
    {
        string statusName = status != null ? status.ToString() : "Status";
        return SpawnStatus(worldPosition, statusName);
    }

    public static DamagePopup2D SpawnStatus(Vector3 worldPosition, object status, Color color)
    {
        string statusName = status != null ? status.ToString() : "Status";
        return SpawnStatus(worldPosition, statusName, color);
    }

    // ---------------------------------------------------------------------
    // ЦВЕТА
    // ---------------------------------------------------------------------

    private static Color ResolveDamageColor(object elementOrColor)
    {
        if (elementOrColor is Color directColor)
            return directColor;

        if (elementOrColor == null)
            return Color.white;

        string value = elementOrColor.ToString().ToLowerInvariant();

        if (value.Contains("fire") || value.Contains("огонь"))
            return new Color(1f, 0.35f, 0.15f);

        if (value.Contains("water") || value.Contains("вода"))
            return new Color(0.35f, 0.75f, 1f);

        if (value.Contains("steam") || value.Contains("пар"))
            return new Color(0.85f, 0.85f, 0.85f);

        if (value.Contains("smolder") || value.Contains("тление"))
            return new Color(1f, 0.55f, 0.25f);

        if (value.Contains("heal") || value.Contains("леч"))
            return new Color(0.35f, 1f, 0.35f);

        return Color.white;
    }

    private static Color ResolveStatusColor(string statusName)
    {
        if (string.IsNullOrWhiteSpace(statusName))
            return Color.white;

        string value = statusName.ToLowerInvariant();

        if (value.Contains("fire") || value.Contains("огонь"))
            return new Color(1f, 0.35f, 0.15f);

        if (value.Contains("water") || value.Contains("вода"))
            return new Color(0.35f, 0.75f, 1f);

        if (value.Contains("steam") || value.Contains("пар"))
            return new Color(0.85f, 0.85f, 0.85f);

        if (value.Contains("smolder") || value.Contains("тление"))
            return new Color(1f, 0.55f, 0.25f);

        return Color.white;
    }
}
