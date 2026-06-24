using System.Collections;
using UnityEngine;

public class StatusEffectController : MonoBehaviour
{
    public enum StatusType
    {
        None,
        Water,
        Fire,
        Steam,
        Smoldering
    }

    [Header("Текущие статусы")]
    public bool hasWater;
    public bool hasFire;
    public bool hasSteam;
    public bool hasSmoldering;

    [Header("Настройки длительности")]
    public float waterDuration = 3f;
    public float fireDuration = 3f;
    public float steamDuration = 5f;
    public float smolderingDuration = 10f;

    [Header("Влияние на движение")]
    [Range(0.05f, 1f)] public float waterSlowMultiplier = 0.5f;
    [Range(0.05f, 1f)] public float steamSlowMultiplier = 0.45f;

    [Header("Периодический урон")]
    public int fireTickDamage = 1;
    public int smolderingTickDamage = 1;
    public float tickInterval = 1f;

    [Header("Визуальные оверлеи статусов")]
    public GameObject waterOverlay;
    public GameObject fireOverlay;
    public GameObject steamOverlay;
    public GameObject smolderingOverlay;

    [Header("Debug")]
    public bool logDebug = true;

    private Coroutine waterRoutine;
    private Coroutine fireRoutine;
    private Coroutine steamRoutine;
    private Coroutine smolderingRoutine;

    private Enemy enemy;
    private BossController boss;
    private PlayerController player;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        boss = GetComponent<BossController>();
        player = GetComponent<PlayerController>();

        RefreshVisuals();
    }

    public void ApplyElementStatus(Element element)
    {
        if (element == Element.Physical)
            return;

        if (logDebug)
            Debug.Log($"{name}: получил статусный элемент {element}. До: {GetStatusDebugString()}");

        switch (element)
        {
            case Element.Fire:
                ApplyFireElement();
                break;

            case Element.Water:
                ApplyWaterElement();
                break;

            case Element.Steam:
                ApplySteam();
                break;

            case Element.Smoldering:
                ApplySmoldering();
                break;
        }

        if (logDebug)
            Debug.Log($"{name}: после применения статуса: {GetStatusDebugString()}");
    }

    private void ApplyFireElement()
    {
        if (hasWater)
        {
            ApplySteam();
            return;
        }

        if (hasSmoldering)
        {
            RestartSmoldering();
            return;
        }

        ApplyFire();
    }

    private void ApplyWaterElement()
    {
        if (hasFire)
        {
            ApplySmoldering();
            return;
        }

        if (hasSteam)
        {
            RestartSteam();
            return;
        }

        ApplyWater();
    }

    public float GetSpeedMultiplier()
    {
        if (hasSteam)
            return steamSlowMultiplier;

        if (hasWater)
            return waterSlowMultiplier;

        return 1f;
    }

    public bool HasSmoldering()
    {
        return hasSmoldering;
    }

    public bool HasSteam()
    {
        return hasSteam;
    }

    public string GetStatusDebugString()
    {
        string result = "";

        if (hasWater)
            result += "Вода: замедление";

        if (hasFire)
            result += AddSeparator(result) + "Огонь: периодический урон";

        if (hasSteam)
            result += AddSeparator(result) + "Пар: сильное замедление";

        if (hasSmoldering)
            result += AddSeparator(result) + "Тление: длительный урон";

        if (string.IsNullOrEmpty(result))
            result = "Нет";

        return result;
    }

    private string AddSeparator(string current)
    {
        return string.IsNullOrEmpty(current) ? "" : " | ";
    }

    public void ClearAllStatuses()
    {
        StopRoutine(ref waterRoutine);
        StopRoutine(ref fireRoutine);
        StopRoutine(ref steamRoutine);
        StopRoutine(ref smolderingRoutine);

        hasWater = false;
        hasFire = false;
        hasSteam = false;
        hasSmoldering = false;

        RefreshVisuals();
    }

    private void ApplyWater()
    {
        StopRoutine(ref waterRoutine);

        hasWater = false;

        DamagePopup2D.SpawnStatus(transform.position, "Вода");
        waterRoutine = StartCoroutine(WaterCoroutine());
    }

    private void ApplyFire()
    {
        StopRoutine(ref fireRoutine);

        hasFire = false;

        DamagePopup2D.SpawnStatus(transform.position, "Огонь");
        fireRoutine = StartCoroutine(FireCoroutine());
    }

    private void ApplySteam()
    {
        StopRoutine(ref waterRoutine);
        StopRoutine(ref fireRoutine);
        StopRoutine(ref steamRoutine);

        hasWater = false;
        hasFire = false;
        hasSteam = false;

        DamagePopup2D.SpawnStatus(transform.position, "Пар");
        steamRoutine = StartCoroutine(SteamCoroutine());
    }

    private void ApplySmoldering()
    {
        StopRoutine(ref waterRoutine);
        StopRoutine(ref fireRoutine);
        StopRoutine(ref smolderingRoutine);

        hasWater = false;
        hasFire = false;
        hasSmoldering = false;

        DamagePopup2D.SpawnStatus(transform.position, "Тление");
        smolderingRoutine = StartCoroutine(SmolderingCoroutine());
    }

    private void RestartSteam()
    {
        StopRoutine(ref steamRoutine);

        hasSteam = false;

        DamagePopup2D.SpawnStatus(transform.position, "Пар+");
        steamRoutine = StartCoroutine(SteamCoroutine());
    }

    private void RestartSmoldering()
    {
        StopRoutine(ref smolderingRoutine);

        hasSmoldering = false;

        DamagePopup2D.SpawnStatus(transform.position, "Тление+");
        smolderingRoutine = StartCoroutine(SmolderingCoroutine());
    }

    private IEnumerator WaterCoroutine()
    {
        hasWater = true;
        RefreshVisuals();

        yield return new WaitForSeconds(waterDuration);

        hasWater = false;
        RefreshVisuals();

        waterRoutine = null;
    }

    private IEnumerator FireCoroutine()
    {
        hasFire = true;
        RefreshVisuals();

        float timer = fireDuration;

        while (timer > 0f)
        {
            yield return new WaitForSeconds(tickInterval);

            DealStatusDamage(fireTickDamage);

            timer -= tickInterval;
        }

        hasFire = false;
        RefreshVisuals();

        fireRoutine = null;
    }

    private IEnumerator SteamCoroutine()
    {
        hasSteam = true;

        hasWater = false;
        hasFire = false;

        RefreshVisuals();

        yield return new WaitForSeconds(steamDuration);

        hasSteam = false;
        RefreshVisuals();

        steamRoutine = null;
    }

    private IEnumerator SmolderingCoroutine()
    {
        hasSmoldering = true;

        hasWater = false;
        hasFire = false;

        RefreshVisuals();

        float timer = smolderingDuration;

        while (timer > 0f)
        {
            yield return new WaitForSeconds(tickInterval);

            DealStatusDamage(smolderingTickDamage);

            timer -= tickInterval;
        }

        hasSmoldering = false;
        RefreshVisuals();

        smolderingRoutine = null;
    }

    private void DealStatusDamage(int amount)
    {
        if (amount <= 0)
            return;

        if (enemy != null)
            enemy.TakeDamage(amount, Element.Physical);
        else if (boss != null)
            boss.TakeDamage(amount, Element.Physical);
        else if (player != null)
            player.TakeDamage(amount, Element.Physical);
    }

    private void RefreshVisuals()
    {
        if (waterOverlay != null)
            waterOverlay.SetActive(hasWater);

        if (fireOverlay != null)
            fireOverlay.SetActive(hasFire);

        if (steamOverlay != null)
            steamOverlay.SetActive(hasSteam);

        if (smolderingOverlay != null)
            smolderingOverlay.SetActive(hasSmoldering);
    }

    private void StopRoutine(ref Coroutine routine)
    {
        if (routine == null)
            return;

        StopCoroutine(routine);
        routine = null;
    }
}