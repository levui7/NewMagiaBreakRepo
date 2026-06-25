using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    [Header("Здоровье")]
    public int maxHealth = 30;
    public int currentHealth;

    [Header("Движение и контактный урон")]
    public float moveSpeed = 3f;
    public int contactDamage = 10;
    public Element contactElement = Element.Physical;
    public Transform playerTarget;
    public float contactDamageCooldown = 1f;
    public float stopDistance = 0.2f;

    [Header("UI")]
    public GameObject damageNumberPrefab;
    public EnemyHealthBar healthBar;

    [Header("Animation")]
    public CharacterAnimation2D characterAnimation;
    public float deathAnimationDuration = 1.0f;

    [Header("Dodging")]
    public bool canDodge = true;
    public float dodgeRadius = 2f;
    public float dodgeForce = 5f;
    public float dodgeCooldown = 1.5f;

    [Header("Movement AI")]
    public bool useZigzagMovement = true;
    public float zigzagStrength = 0.7f;
    public float zigzagFrequency = 4f;

    private float nextDodgeTime;

    private bool isDead;

    protected Rigidbody2D rb;
    protected StatusEffectController statusEffects;
    protected float lastContactDamageTime;

    private float baseMoveSpeed;
    private int baseMaxHealth;
    private int baseContactDamage;
    private bool difficultyApplied;

    [Header("Difficulty Scaling")]
    public bool autoScaleFromProgress = true;

    protected virtual void Awake()
    {
        if (characterAnimation == null)
            characterAnimation = GetComponent<CharacterAnimation2D>();

        if (characterAnimation == null)
            characterAnimation = GetComponentInChildren<CharacterAnimation2D>();

        rb = GetComponent<Rigidbody2D>();
        statusEffects = GetComponent<StatusEffectController>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        baseMoveSpeed = moveSpeed;
        baseMaxHealth = maxHealth;
        baseContactDamage = contactDamage;
    }

    protected virtual void Start()
    {
        if (autoScaleFromProgress && PlayerProgressManager.Instance != null)
        {
            ApplyProgressDifficulty(
                PlayerProgressManager.Instance.GetEnemyHealthMultiplier(),
                PlayerProgressManager.Instance.GetEnemyDamageMultiplier(),
                PlayerProgressManager.Instance.GetEnemySpeedMultiplier()
            );
        }

        if (!difficultyApplied)
            currentHealth = maxHealth;

        UpdateHealthBar();
        FindTarget();
    }

    protected virtual void FixedUpdate()
    {
        if (playerTarget == null || !playerTarget.gameObject.activeInHierarchy)
        {
            FindTarget();
            rb.linearVelocity = Vector2.zero;
            return;
        }

        MoveToTarget();
    }

    protected virtual void FindTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDist = Mathf.Infinity;
        Transform closestPlayer = null;

        foreach (GameObject p in players)
        {
            if (!p.activeInHierarchy) continue;

            float dist = Vector2.Distance(p.transform.position, transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestPlayer = p.transform;
            }
        }

        playerTarget = closestPlayer;
    }

    protected virtual float GetSpeedMultiplier()
    {
        float multiplier = 1f;

        // Проверяем статус-эффекты
        if (statusEffects != null)
            multiplier *= statusEffects.GetSpeedMultiplier();

        return multiplier;
    }

    protected virtual void MoveToTarget()
    {
        if (TryDodge())
        {
            if (characterAnimation != null)
                characterAnimation.SetSpeed(rb.linearVelocity.magnitude);

            return;
        }

        if (rb == null || playerTarget == null)
            return;

        Vector2 dir = (Vector2)(playerTarget.position - transform.position);

        if (dir.magnitude <= stopDistance)
        {
            rb.linearVelocity = Vector2.zero;

            if (characterAnimation != null)
                characterAnimation.SetSpeed(0f);

            return;
        }

        dir.Normalize();

        if (useZigzagMovement)
        {
            float zigzag =
                Mathf.Sin(Time.time * zigzagFrequency + GetInstanceID())
                * zigzagStrength;

            Vector2 side = Vector2.Perpendicular(dir);

            dir += side * zigzag;
            dir.Normalize();
        }

        // Сбрасываем поворот
        transform.rotation = Quaternion.identity;
        transform.eulerAngles = Vector3.zero;

        float speedMultiplier = GetSpeedMultiplier();
        rb.linearVelocity = dir * moveSpeed * speedMultiplier;

        if (characterAnimation != null)
        {
            characterAnimation.SetDirection(dir);
            characterAnimation.SetSpeed(rb.linearVelocity.magnitude);
        }
    }

    public virtual void ApplyProgressDifficulty(float healthMultiplier, float damageMultiplier, float speedMultiplier)
    {
        if (difficultyApplied)
            return;

        difficultyApplied = true;

        maxHealth = Mathf.Max(1, Mathf.RoundToInt(baseMaxHealth * healthMultiplier));
        currentHealth = maxHealth;

        contactDamage = Mathf.Max(1, Mathf.RoundToInt(baseContactDamage * damageMultiplier));
        moveSpeed = Mathf.Max(0.1f, baseMoveSpeed * speedMultiplier);

        UpdateHealthBar();
    }

    public virtual void TakeDamage(int amount, Element element)
    {
        if (isDead)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        ShowDamageNumber(amount, element);
        UpdateHealthBar();

        if (statusEffects != null)
            statusEffects.ApplyElementStatus(element);

        if (characterAnimation != null && currentHealth > 0f)
            characterAnimation.PlayTakeDamage();

        if (currentHealth <= 0f)
            Die();
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateHealthBar();
    }

    protected void UpdateHealthBar()
    {
        if (healthBar != null)
            healthBar.SetHealth(currentHealth, maxHealth);
    }

    protected void ShowDamageNumber(int amount, Element element)
    {
        // Новый вариант работает даже без заранее созданного prefab DamageNumber.
        DamagePopup2D.SpawnDamage(transform.position, amount, element);

        // Старый вариант оставлен для совместимости, если у тебя уже есть свой prefab урона.
        if (damageNumberPrefab == null) return;

        GameObject dn = Instantiate(damageNumberPrefab, transform.position + Vector3.up * 0.8f, Quaternion.identity);
        DamageNumber dnScript = dn.GetComponent<DamageNumber>();

        if (dnScript != null)
        {
            Color color = Color.white;
            if (element == Element.Fire) color = Color.red;
            else if (element == Element.Water) color = Color.blue;
            dnScript.Setup(amount, color);
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        TryDealContactDamage(collision.collider);
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        TryDealContactDamage(collision.collider);
    }

    protected void TryDealContactDamage(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;

        if (Time.time < lastContactDamageTime + contactDamageCooldown)
            return;

        lastContactDamageTime = Time.time;

        if (characterAnimation != null)
            characterAnimation.PlayAttack();

        player.TakeDamage(contactDamage, contactElement);
    }

    private bool TryDodge()
    {
        if (!canDodge)
            return false;

        if (Time.time < nextDodgeTime)
            return false;

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(transform.position, dodgeRadius);

        foreach (Collider2D hit in hits)
        {
            BulletScript bullet = hit.GetComponent<BulletScript>();

            if (bullet == null)
                continue;

            Vector2 toBullet =
                bullet.transform.position - transform.position;

            Vector2 dodgeDir =
                Vector2.Perpendicular(toBullet).normalized;

            rb.linearVelocity =
                dodgeDir * dodgeForce;

            nextDodgeTime =
                Time.time + dodgeCooldown;

            return true;
        }

        return false;
    }



    protected virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;

        RoomManager roomManager = FindObjectOfType<RoomManager>();

        if (roomManager != null)
            roomManager.EnemyDied(this);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        LootDropper2D lootDropper = GetComponent<LootDropper2D>();

        if (lootDropper != null)
            lootDropper.DropLoot();

        if (characterAnimation != null)
            characterAnimation.PlayDeath();

        Destroy(gameObject, deathAnimationDuration);
    }
}