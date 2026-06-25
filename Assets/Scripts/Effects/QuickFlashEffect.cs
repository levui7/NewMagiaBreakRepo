using UnityEngine;

public class QuickFlashEffect : MonoBehaviour
{
    [Header("Настройки анимации")]
    [Tooltip("Время появления (сек)")]
    public float fadeInTime = 0.05f;  // 50 мс

    [Tooltip("Время видимости (сек)")]
    public float visibleTime = 0.1f;  // 100 мс

    [Tooltip("Время исчезновения (сек)")]
    public float fadeOutTime = 0.15f; // 150 мс

    [Tooltip("Максимальный размер вспышки")]
    public float maxScale = 3f;

    private SpriteRenderer sr;
    private float timer = 0f;
    private float totalDuration;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("QuickFlashEffect: SpriteRenderer не найден!");
            Destroy(gameObject);
            return;
        }

        // Начальное состояние: невидимый и маленький
        sr.color = new Color(1f, 1f, 1f, 0f);
        transform.localScale = Vector3.zero;

        totalDuration = fadeInTime + visibleTime + fadeOutTime;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // Фаза 1: Появление
        if (timer < fadeInTime)
        {
            float t = timer / fadeInTime;
            sr.color = new Color(1f, 1f, 1f, t);
            transform.localScale = Vector3.one * Mathf.Lerp(0f, maxScale, t);
        }
        // Фаза 2: Видимость (полностью видим)
        else if (timer < fadeInTime + visibleTime)
        {
            sr.color = Color.white;
            transform.localScale = Vector3.one * maxScale;
        }
        // Фаза 3: Исчезновение
        else if (timer < totalDuration)
        {
            float fadeT = (timer - fadeInTime - visibleTime) / fadeOutTime;
            sr.color = new Color(1f, 1f, 1f, 1f - fadeT);
            transform.localScale = Vector3.one * maxScale;
        }
        // Уничтожить после завершения
        else
        {
            Destroy(gameObject);
        }
    }
}