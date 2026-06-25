using UnityEngine;

public class RangedEnemy2D : Enemy
{
    [Header("Дальний бой")]
    public GameObject enemyProjectilePrefab;
    public Transform firePoint;

    public float attackRange = 8f;
    public float keepDistance = 4f;

    public float distanceTolerance = 0.8f;

    public float fireCooldown = 1.5f;
    public int projectileDamage = 8;

    public Element projectileElement = Element.Physical;

    public bool requireLineOfSight = true;
    public LayerMask obstacleMask;

    [Header("Стрейф")]
    public bool strafeAroundPlayer = true;
    public float strafeSpeed = 2f;

    private float nextFireTime;

    private int baseProjectileDamage;
    private float baseFireCooldown;
    private bool rangedDifficultyApplied;

    protected override void Awake()
    {
        base.Awake();

        baseProjectileDamage = projectileDamage;
        baseFireCooldown = fireCooldown;
    }

    protected override void Start()
    {
        base.Start();

        nextFireTime = Time.time + 0.5f;
    }

    protected override void FixedUpdate()
    {
        if (playerTarget == null || !playerTarget.gameObject.activeInHierarchy)
        {
            FindTarget();

            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 toPlayer =
            (Vector2)(playerTarget.position - transform.position);

        float distance = toPlayer.magnitude;

        Vector2 dirToPlayer = toPlayer.normalized;

        // Поворачиваем только спрайт анимации
        if (characterAnimation != null)
            characterAnimation.SetDirection(dirToPlayer);

        // Слишком далеко
        if (distance > attackRange)
        {
            MoveToTarget();
            return;
        }

        // Слишком близко
        if (distance < keepDistance)
        {
            MoveAwayFromTarget(dirToPlayer);
            return;
        }

        // Чуть дальше нужной дистанции
        if (distance > keepDistance + distanceTolerance)
        {
            MoveToTarget();
            return;
        }

        // Оптимальная дистанция
        if (strafeAroundPlayer)
            Strafe(dirToPlayer);
        else
            rb.linearVelocity = Vector2.zero;

        TryShoot(dirToPlayer, distance);
    }

    private void MoveAwayFromTarget(Vector2 dirToPlayer)
    {
        float speedMultiplier =
            statusEffects != null
            ? statusEffects.GetSpeedMultiplier()
            : 1f;

        rb.linearVelocity =
            -dirToPlayer * moveSpeed * speedMultiplier;

        if (characterAnimation != null)
        {
            characterAnimation.SetDirection(-dirToPlayer);
            characterAnimation.SetSpeed(rb.linearVelocity.magnitude);
        }
    }

    private void Strafe(Vector2 dirToPlayer)
    {
        float speedMultiplier =
            statusEffects != null
            ? statusEffects.GetSpeedMultiplier()
            : 1f;

        Vector2 side = Vector2.Perpendicular(dirToPlayer);

        float sideDirection =
            Mathf.Sin(Time.time + GetInstanceID()) > 0
            ? 1f
            : -1f;

        rb.linearVelocity =
            side * sideDirection *
            strafeSpeed *
            speedMultiplier;

        if (characterAnimation != null)
            characterAnimation.SetSpeed(rb.linearVelocity.magnitude);
    }

    private void TryShoot(Vector2 direction, float distance)
    {
        if (enemyProjectilePrefab == null || firePoint == null)
            return;

        if (Time.time < nextFireTime)
            return;

        if (requireLineOfSight)
        {
            RaycastHit2D hit =
                Physics2D.Raycast(
                    firePoint.position,
                    direction,
                    distance,
                    obstacleMask);

            if (hit.collider != null)
                return;
        }

        nextFireTime = Time.time + fireCooldown;

        if (characterAnimation != null)
            characterAnimation.PlayAttack();

        GameObject projectile =
            Instantiate(
                enemyProjectilePrefab,
                firePoint.position,
                Quaternion.identity);

        EnemyProjectile2D projectileScript =
            projectile.GetComponent<EnemyProjectile2D>();

        if (projectileScript != null)
        {
            projectileScript.damage = projectileDamage;
            projectileScript.element = projectileElement;
            projectileScript.Launch(direction);
        }
    }

    public override void ApplyProgressDifficulty(
        float healthMultiplier,
        float damageMultiplier,
        float speedMultiplier)
    {
        base.ApplyProgressDifficulty(
            healthMultiplier,
            damageMultiplier,
            speedMultiplier);

        if (rangedDifficultyApplied)
            return;

        rangedDifficultyApplied = true;

        projectileDamage =
            Mathf.Max(
                1,
                Mathf.RoundToInt(
                    baseProjectileDamage * damageMultiplier));

        fireCooldown =
            Mathf.Max(
                0.35f,
                baseFireCooldown /
                Mathf.Clamp(speedMultiplier, 1f, 1.75f));
    }
}