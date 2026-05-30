using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BossController : MonoBehaviour
{
    [Header("Фазы")]
    public int phase = 1;

    [Header("Здоровье")]
    public int maxHealth = 250;
    public int currentHealth;

    [Header("Движение")]
    public float moveSpeed = 1.8f;
    public float phase2MoveSpeed = 2.3f;
    public float phase3MoveSpeed = 2.8f;
    public float preferredDistance = 6f;
    public float stopDeadZone = 0.4f;
    public float rotateSpeed = 12f;

    [Header("Стрельба")]
    public EnemyProjectile2D projectilePrefab;
    public Transform firePoint;
    public float fireCooldown = 1.4f;
    public float phase2FireCooldown = 1.05f;
    public float phase3FireCooldown = 0.75f;
    public int projectileDamage = 10;
    public float projectileSpeed = 8f;
    public WeaponManager.Element projectileElement = WeaponManager.Element.Physical;

    [Header("Спецатаки")]
    public bool useRadialAttack = true;
    public float radialAttackCooldown = 6f;
    public int radialProjectileCount = 10;
    public int phase3RadialProjectileCount = 16;

    [Header("Призыв врагов")]
    public bool canSummonEnemies = false;
    public Enemy enemyPrefab;
    public Transform[] summonPoints;
    public float summonCooldown = 10f;
    public int summonsPerWave = 2;

    [Header("Визуал фаз")]
    public GameObject fireShieldVisual;
    public GameObject iceFloorVisual;
    public GameObject enragedVisual;

    [Header("UI")]
    public EnemyHealthBar healthBar;

    [Header("Animation")]
    public CharacterAnimation2D characterAnimation;
    public float deathAnimationDuration = 1.5f;

    private Rigidbody2D rb;
    private Collider2D bossCollider;
    private StatusEffectController statusEffects;
    private Transform target;
    private float fireTimer;
    private float radialTimer;
    private float summonTimer;
    private bool isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bossCollider = GetComponent<Collider2D>();
        statusEffects = GetComponent<StatusEffectController>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        bossCollider.isTrigger = false;

        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform, false);
            fp.transform.localPosition = new Vector3(0.75f, 0f, 0f);
            firePoint = fp.transform;
        }

        if (characterAnimation == null)
            characterAnimation = GetComponent<CharacterAnimation2D>();

        if (characterAnimation == null)
            characterAnimation = GetComponentInChildren<CharacterAnimation2D>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();

        SetPhaseVisuals();
        FindTarget();

        fireTimer = 0.5f;
        radialTimer = 2.5f;
        summonTimer = 4f;
    }

    private void Update()
    {
        if (isDead) return;

        if (target == null)
            FindTarget();

        UpdatePhaseByHealth();
        UpdateTimers();
    }

    private void FixedUpdate()
    {
        if (isDead || target == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        MoveAroundTarget();
    }

    private void FindTarget()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        float bestDistance = float.MaxValue;
        Transform bestTarget = null;

        foreach (PlayerController player in players)
        {
            if (player == null || !player.gameObject.activeInHierarchy) continue;

            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTarget = player.transform;
            }
        }

        target = bestTarget;
    }

    private void UpdatePhaseByHealth()
    {
        if (currentHealth <= maxHealth * 0.35f && phase < 3)
            ChangePhase(3);
        else if (currentHealth <= maxHealth * 0.7f && phase < 2)
            ChangePhase(2);
    }

    private void ChangePhase(int newPhase)
    {
        phase = newPhase;
        SetPhaseVisuals();

        string label = phase == 2 ? "Фаза 2" : "Фаза 3";
        DamagePopup2D.SpawnText(transform.position + Vector3.up * 1.1f, label, Color.yellow);
    }

    private void SetPhaseVisuals()
    {
        if (fireShieldVisual != null) fireShieldVisual.SetActive(phase == 1);
        if (iceFloorVisual != null) iceFloorVisual.SetActive(phase == 2);
        if (enragedVisual != null) enragedVisual.SetActive(phase == 3);
    }

    private void UpdateTimers()
    {
        fireTimer -= Time.deltaTime;
        radialTimer -= Time.deltaTime;
        summonTimer -= Time.deltaTime;

        if (target != null && fireTimer <= 0f)
        {
            ShootAtTarget();
            fireTimer = GetCurrentFireCooldown();
        }

        if (useRadialAttack && phase >= 2 && radialTimer <= 0f)
        {
            RadialAttack();
            radialTimer = radialAttackCooldown;
        }

        if (canSummonEnemies && phase >= 2 && enemyPrefab != null && summonTimer <= 0f)
        {
            SummonEnemies();
            summonTimer = summonCooldown;
        }
    }

    private void MoveAroundTarget()
    {
        Vector2 toTarget = target.position - transform.position;
        float distance = toTarget.magnitude;

        if (distance < 0.01f)
        {
            rb.linearVelocity = Vector2.zero;

            if (characterAnimation != null)
                characterAnimation.SetSpeed(0f);

            return;
        }

        Vector2 directionToTarget = toTarget.normalized;
        Vector2 moveDirection = Vector2.zero;

        if (distance > preferredDistance + stopDeadZone)
            moveDirection = directionToTarget;
        else if (distance < preferredDistance - stopDeadZone)
            moveDirection = -directionToTarget;
        else
            moveDirection = Vector2.Perpendicular(directionToTarget) * 0.45f;

        rb.linearVelocity = moveDirection * GetCurrentMoveSpeed();
        RotateToDirection(directionToTarget);

        if (characterAnimation != null)
            characterAnimation.SetSpeed(rb.linearVelocity.magnitude);
    }

    private float GetCurrentMoveSpeed()
    {
        if (phase >= 3) return phase3MoveSpeed;
        if (phase == 2) return phase2MoveSpeed;
        return moveSpeed;
    }

    private float GetCurrentFireCooldown()
    {
        if (phase >= 3) return phase3FireCooldown;
        if (phase == 2) return phase2FireCooldown;
        return fireCooldown;
    }

    private void RotateToDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }

    private void ShootAtTarget()
    {
        if (target == null)
            return;

        if (characterAnimation != null)
            characterAnimation.PlayAttack();

        Vector2 direction =
            (target.position - firePoint.position).normalized;

        SpawnProjectile(direction,
            projectileElement,
            projectileDamage);
    }

    private void RadialAttack()
    {
        int count = phase >= 3 ? phase3RadialProjectileCount : radialProjectileCount;
        WeaponManager.Element element = phase >= 3 ? WeaponManager.Element.Fire : WeaponManager.Element.Water;

        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            SpawnProjectile(direction, element, Mathf.Max(1, projectileDamage - 2));
        }

        DamagePopup2D.SpawnText(transform.position + Vector3.up * 1.1f, "Волна", Color.cyan);
    }

    private void SpawnProjectile(Vector2 direction, WeaponManager.Element element, int damage)
    {
        EnemyProjectile2D projectile = null;

        if (projectilePrefab != null)
        {
            projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        }
        else
        {
            projectile = CreateRuntimeProjectile(firePoint.position);
        }

        projectile.damage = damage;
        projectile.element = element;
        projectile.speed = projectileSpeed;
        projectile.Launch(direction);
    }

    private EnemyProjectile2D CreateRuntimeProjectile(Vector3 position)
    {
        GameObject obj = new GameObject("BossProjectile_Runtime");
        obj.transform.position = position;
        obj.tag = "EnemyProjectile";

        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateFallbackCircleSprite();
        renderer.color = Color.red;
        renderer.sortingOrder = 30;

        CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.18f;

        Rigidbody2D body = obj.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;

        return obj.AddComponent<EnemyProjectile2D>();
    }

    private Sprite CreateFallbackCircleSprite()
    {
        Texture2D tex = new Texture2D(16, 16);
        Color clear = new Color(0f, 0f, 0f, 0f);
        Vector2 center = new Vector2(7.5f, 7.5f);

        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center);
                tex.SetPixel(x, y, d <= 7f ? Color.white : clear);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
    }

    private void SummonEnemies()
    {
        if (summonPoints == null || summonPoints.Length == 0) return;

        int spawned = 0;
        for (int i = 0; i < summonPoints.Length && spawned < summonsPerWave; i++)
        {
            if (summonPoints[i] == null) continue;
            Instantiate(enemyPrefab, summonPoints[i].position, Quaternion.identity);
            spawned++;
        }

        DamagePopup2D.SpawnText(transform.position + Vector3.up * 1.1f, "Призыв", Color.magenta);
    }

    public void TakeDamage(int amount, WeaponManager.Element element)
    {
        if (isDead) return;

        // В первой фазе огонь слабее из-за щита, но босс всё равно получает часть урона,
        // чтобы игрок видел обратную связь, а бой не казался сломанным.
        if (phase == 1 && element == WeaponManager.Element.Fire)
            amount = Mathf.Max(1, Mathf.RoundToInt(amount * 0.35f));

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        DamagePopup2D.SpawnDamage(transform.position, amount, element);

        if (characterAnimation != null && currentHealth > 0)
            characterAnimation.PlayTakeDamage();

        if (statusEffects != null)
            statusEffects.ApplyElementStatus(element);

        UpdateHealthBar();

        if (currentHealth <= 0)
            Die();
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
            healthBar.SetHealth(currentHealth, maxHealth);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        DamagePopup2D.SpawnText(transform.position + Vector3.up * 1.1f, "Босс побеждён", Color.green);
        LootDropper2D lootDropper = GetComponent<LootDropper2D>();

        if (lootDropper != null)
            lootDropper.DropLoot();
        if (PlayerInventoryManager.Instance != null)
            PlayerInventoryManager.Instance.SaveAllWeaponsInScene();

        if (MaterialsHUD2D.Instance != null)
            MaterialsHUD2D.Instance.Refresh();

        if (characterAnimation != null)
            characterAnimation.PlayDeath();

        StartCoroutine(LoadVictoryAfterDeath());
    }

    private IEnumerator LoadVictoryAfterDeath()
    {
        yield return new WaitForSeconds(deathAnimationDuration);

        Destroy(gameObject);
    }
}
