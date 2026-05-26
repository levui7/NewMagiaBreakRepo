using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile2D : MonoBehaviour
{
    [Header("Projectile")]
    public float speed = 8f;
    public float lifeTime = 4f;
    public int damage = 8;
    public WeaponManager.Element element = WeaponManager.Element.Physical;

    [Header("Collision")]
    public LayerMask obstacleMask;

    private Rigidbody2D rb;
    private Vector2 direction = Vector2.right;
    private GameObject owner;

    private bool launched;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    private void Start()
    {
        if (!launched)
            Launch(direction, damage, element, owner);

        Destroy(gameObject, lifeTime);
    }

    // ������������� � RangedEnemy2D � BossController.
    public void Launch(Vector2 launchDirection)
    {
        Launch(launchDirection, damage, element, owner);
    }

    public void Launch(Vector3 launchDirection)
    {
        Launch(new Vector2(launchDirection.x, launchDirection.y), damage, element, owner);
    }

    public void Launch(Vector2 launchDirection, int newDamage)
    {
        Launch(launchDirection, newDamage, element, owner);
    }

    public void Launch(Vector2 launchDirection, float newDamage)
    {
        Launch(launchDirection, Mathf.CeilToInt(newDamage), element, owner);
    }

    public void Launch(Vector2 launchDirection, int newDamage, WeaponManager.Element newElement)
    {
        Launch(launchDirection, newDamage, newElement, owner);
    }

    public void Launch(Vector2 launchDirection, float newDamage, WeaponManager.Element newElement)
    {
        Launch(launchDirection, Mathf.CeilToInt(newDamage), newElement, owner);
    }

    public void Launch(Vector2 launchDirection, int newDamage, WeaponManager.Element newElement, GameObject newOwner)
    {
        if (launchDirection.sqrMagnitude <= 0.001f)
            launchDirection = Vector2.right;

        direction = launchDirection.normalized;
        damage = newDamage;
        element = newElement;
        owner = newOwner;
        launched = true;

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.linearVelocity = direction * speed;

        PlayerController.RotateTransformToDirection2D(transform, direction);
    }

    public void Launch(Vector2 launchDirection, float newDamage, WeaponManager.Element newElement, GameObject newOwner)
    {
        Launch(launchDirection, Mathf.CeilToInt(newDamage), newElement, newOwner);
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

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    public void SetDamage(float newDamage)
    {
        damage = Mathf.CeilToInt(newDamage);
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
        if (other == null)
            return;

        if (owner != null && other.gameObject == owner)
            return;

        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
            return;

        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player != null)
        {
            StatusEffectController status = player.GetComponent<StatusEffectController>();

            if (status != null)
                status.SendMessage("ApplyElement", element, SendMessageOptions.DontRequireReceiver);

            player.TakeDamage(damage, element);

            Destroy(gameObject);
            return;
        }

        if (IsObstacle(other))
        {
            Destroy(gameObject);
            return;
        }
    }

    private bool IsObstacle(Collider2D other)
    {
        if (other == null)
            return false;

        if (other.CompareTag("Wall"))
            return true;

        if (obstacleMask.value != 0)
        {
            int objectLayerMask = 1 << other.gameObject.layer;

            if ((obstacleMask.value & objectLayerMask) != 0)
                return true;
        }

        return false;
    }
}