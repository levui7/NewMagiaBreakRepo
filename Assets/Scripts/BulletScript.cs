using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletScript : MonoBehaviour
{
    [Header("Bullet")]
    public float speed = 14f;
    public float lifeTime = 3f;
    public float damage = 10f;
    public WeaponManager.Element element = WeaponManager.Element.Physical;

    [Header("AoE")]
    public bool isAoE = false;
    public float aoeRadius = 2.5f;
    public LayerMask damageMask;

    [Header("Owner")]
    public GameObject owner;

    private Rigidbody2D rb;
    private Vector2 direction = Vector2.right;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        if (damageMask.value == 0)
        {
            int enemyLayer = LayerMask.NameToLayer("Enemy");

            if (enemyLayer >= 0)
                damageMask = 1 << enemyLayer;
            else
                damageMask = ~0;
        }
    }

    private void Start()
    {
        rb.linearVelocity = direction.normalized * speed;
        PlayerController.RotateTransformToDirection2D(transform, direction);

        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(Vector2 newDirection)
    {
        if (newDirection.sqrMagnitude <= 0.001f)
            return;

        direction = newDirection.normalized;

        if (rb != null)
            rb.linearVelocity = direction * speed;

        PlayerController.RotateTransformToDirection2D(transform, direction);
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    public void SetElement(WeaponManager.Element newElement)
    {
        element = newElement;
    }

    public void SetOwner(GameObject newOwner)
    {
        owner = newOwner;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (owner != null && other.gameObject == owner)
            return;

        if (other.CompareTag("Player"))
            return;

        if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Enemy") ||
            other.CompareTag("Boss") ||
            other.GetComponentInParent<Enemy>() != null ||
            other.GetComponentInParent<BossController>() != null)
        {
            if (isAoE)
                ApplyAoEDamage();
            else
                ApplyDamageTo(other);

            Destroy(gameObject);
        }
    }

    private void ApplyDamageTo(Collider2D target)
    {
        int damageInt = Mathf.CeilToInt(damage);

        StatusEffectController status = target.GetComponentInParent<StatusEffectController>();

        if (status != null)
        {
            status.SendMessage("ApplyElement", element, SendMessageOptions.DontRequireReceiver);
        }

        Enemy enemy = target.GetComponentInParent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeDamage(damageInt, element);
            return;
        }

        BossController boss = target.GetComponentInParent<BossController>();

        if (boss != null)
        {
            boss.TakeDamage(damageInt, element);
            return;
        }

        PlayerController player = target.GetComponentInParent<PlayerController>();

        if (player != null)
        {
            player.TakeDamage(damageInt, element);
            return;
        }
    }

    private void ApplyAoEDamage()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(transform.position, aoeRadius, damageMask);

        int damageInt = Mathf.CeilToInt(damage);

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            if (owner != null && hit.gameObject == owner)
                continue;

            WallHitReceiver2D wall =
                hit.GetComponentInParent<WallHitReceiver2D>();

            if (wall != null)
            {
                wall.TakeHit(damageInt);
                continue;
            }

            ApplyDamageTo(hit);
        }
    }
}