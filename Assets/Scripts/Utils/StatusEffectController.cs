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

    private Coroutine waterRoutine;
    private Coroutine fireRoutine;
    private Coroutine steamRoutine;
    private Coroutine smolderingRoutine;

    private WeaponManager.Element lastElementStatus = WeaponManager.Element.Physical;
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

    public void ApplyElementStatus(WeaponManager.Element element)
    {
        if (element == WeaponManager.Element.Physical)
            return;

        if (element == WeaponManager.Element.Water)
        {
            if (lastElementStatus == WeaponManager.Element.Fire || hasFire)
                ApplySmoldering();
            else
                ApplyWater();

            lastElementStatus = WeaponManager.Element.Water;
        }
        else if (element == WeaponManager.Element.Fire)
        {
            if (lastElementStatus == WeaponManager.Element.Water || hasWater)
                ApplySteam();
            else
                ApplyFire();

            lastElementStatus = WeaponManager.Element.Fire;
        }
    }

    public float GetSpeedMultiplier()
    {
        if (hasSteam) return steamSlowMultiplier;
        if (hasWater) return waterSlowMultiplier;
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
        string result = "Нет";

        if (hasWater) result = "Вода: замедление";
        if (hasFire) result = result == "Нет" ? "Огонь: периодический урон" : result + " | Огонь";
        if (hasSteam) result = "Пар: сильное замедление";
        if (hasSmoldering) result = "Тление: длительный урон";

        return result;
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
        lastElementStatus = WeaponManager.Element.Physical;
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
        if (amount <= 0) return;

        if (enemy != null)
            enemy.TakeDamage(amount, WeaponManager.Element.Physical);
        else if (boss != null)
            boss.TakeDamage(amount, WeaponManager.Element.Physical);
        else if (player != null)
            player.TakeDamage(amount, WeaponManager.Element.Physical);
    }

    private void RefreshVisuals()
    {
        if (waterOverlay != null) waterOverlay.SetActive(hasWater);
        if (fireOverlay != null) fireOverlay.SetActive(hasFire);
        if (steamOverlay != null) steamOverlay.SetActive(hasSteam);
        if (smolderingOverlay != null) smolderingOverlay.SetActive(hasSmoldering);

        if (player != null && UIManager.instance != null)
            UIManager.instance.UpdateStatus(player.playerID, GetStatusDebugString());
    }

    private void StopRoutine(ref Coroutine routine)
    {
        if (routine == null) return;
        StopCoroutine(routine);
        routine = null;
    }
}
