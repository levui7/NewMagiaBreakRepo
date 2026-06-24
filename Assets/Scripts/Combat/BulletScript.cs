using UnityEngine;

public class BulletScript : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 12f;
    public float lifeTime = 3f;

    [Header("Damage")]
    [SerializeField] private float damage = 1f;
    [SerializeField] private Element element = Element.Physical;

    [Header("AoE")]
    public bool isAoE = false;
    public float aoeRadius = 2.5f;
    public LayerMask damageMask;

    [Header("Debug")]
    public bool logDebug = true;
    public bool drawAoEGizmo = true;

    private Vector2 direction = Vector2.right;
    private GameObject owner;
    private bool hasExploded = false;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);
    }

    public void SetDirection(Vector2 newDirection)
    {
        if (newDirection.sqrMagnitude <= 0.001f)
            newDirection = Vector2.right;

        direction = newDirection.normalized;
    }

    public void SetDamage(float newDamage)
    {
        damage = Mathf.Max(0f, newDamage);
    }

    public void SetElement(Element newElement)
    {
        element = newElement;
    }

    public void SetOwner(GameObject newOwner)
    {
        owner = newOwner;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded)
            return;

        if (other == null)
            return;

        if (owner != null)
        {
            if (other.gameObject == owner)
                return;

            if (other.transform.IsChildOf(owner.transform))
                return;
        }

        if (isAoE)
        {
            ExplodeAoE();
            return;
        }

        ApplyDamageTo(other);
        Destroy(gameObject);
    }

    private void ExplodeAoE()
    {
        if (hasExploded)
            return;

        hasExploded = true;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aoeRadius, damageMask);
        int damageInt = Mathf.CeilToInt(damage);

        if (logDebug)
        {
            Debug.Log(
                $"BulletScript AoE explosion: " +
                $"position={transform.position}, " +
                $"radius={aoeRadius}, " +
                $"hits={hits.Length}, " +
                $"damage={damageInt}, " +
                $"element={element}");
        }

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            if (owner != null)
            {
                if (hit.gameObject == owner)
                    continue;

                if (hit.transform.IsChildOf(owner.transform))
                    continue;
            }

            ApplyDamageTo(hit);
        }

        Destroy(gameObject);
    }

    private void ApplyDamageTo(Collider2D target)
    {
        if (target == null)
            return;

        int damageInt = Mathf.CeilToInt(damage);

        Enemy enemy = target.GetComponentInParent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeDamage(damageInt, element);

            if (logDebug)
                Debug.Log($"BulletScript: enemy hit {enemy.name}, damage={damageInt}, element={element}");

            return;
        }

        TrainingDummy dummy = target.GetComponentInParent<TrainingDummy>();

        if (dummy != null)
        {
            dummy.TakeDamage(damageInt, element);
            return;
        }

        BossController boss = target.GetComponentInParent<BossController>();

        if (boss != null)
        {
            boss.TakeDamage(damageInt, element);

            if (logDebug)
                Debug.Log($"BulletScript: boss hit {boss.name}, damage={damageInt}, element={element}");

            return;
        }

        PlayerController player = target.GetComponentInParent<PlayerController>();

        if (player != null)
        {
            player.TakeDamage(damageInt, element);

            if (logDebug)
                Debug.Log($"BulletScript: player hit {player.name}, damage={damageInt}, element={element}");

            return;
        }

        WallHitReceiver2D wall = target.GetComponentInParent<WallHitReceiver2D>();

        if (wall != null)
        {
            wall.TakeHit(damageInt);

            if (logDebug)
                Debug.Log($"BulletScript: wall hit {wall.name}, damage={damageInt}");

            return;
        }

        if (logDebug)
            Debug.Log($"BulletScript: collider found, but no damage receiver: {target.name}");
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawAoEGizmo)
            return;

        if (!isAoE)
            return;

        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}