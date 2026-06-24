using UnityEngine;

public class SimpleMine2D : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 20;
    public Element damageElement = Element.Physical;

    [Header("Explosion")]
    public float radius = 1.2f;
    public bool damageInRadius = true;
    public LayerMask targetMask;

    [Header("Visual")]
    public GameObject explosionVisualPrefab;

    [Header("Debug")]
    public bool logDebug = true;

    private bool exploded;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (exploded)
            return;

        if (!TrapTargetUtility2D.IsValidTarget(other, targetMask))
            return;

        Explode(other);
    }

    private void Explode(Collider2D triggeringCollider)
    {
        if (exploded)
            return;

        exploded = true;

        if (logDebug)
            Debug.Log($"SimpleMine2D {name}: exploded");

        if (damageInRadius)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, targetMask);

            foreach (Collider2D hit in hits)
            {
                if (!TrapTargetUtility2D.IsValidTarget(hit, targetMask))
                    continue;

                TrapTargetUtility2D.ApplyDamageAndStatus(hit, damage, damageElement);
            }
        }
        else
        {
            TrapTargetUtility2D.ApplyDamageAndStatus(triggeringCollider, damage, damageElement);
        }

        if (explosionVisualPrefab != null)
            Instantiate(explosionVisualPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}