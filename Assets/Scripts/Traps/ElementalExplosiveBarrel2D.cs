using UnityEngine;

public class ElementalExplosiveBarrel2D : MonoBehaviour
{
    [Header("Element")]
    public Element explosionElement = Element.Fire;

    [Header("Explosion")]
    public int damage = 15;
    public float radius = 2.5f;
    public LayerMask targetMask;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public bool explodeOnProjectileHit = true;

    [Header("Visual")]
    public GameObject normalVisual;
    public GameObject explosionVisualPrefab;

    [Header("Debug")]
    public bool logDebug = true;

    private bool playerInside;
    private bool exploded;

    private void Update()
    {
        if (exploded)
            return;

        if (playerInside && Input.GetKeyDown(interactKey))
            Explode();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (exploded)
            return;

        if (other.GetComponentInParent<PlayerController>() != null)
            playerInside = true;

        if (explodeOnProjectileHit && other.GetComponentInParent<BulletScript>() != null)
            Explode();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerController>() != null)
            playerInside = false;
    }

    public void Explode()
    {
        if (exploded)
            return;

        exploded = true;

        if (logDebug)
        {
            Debug.Log(
                $"ElementalExplosiveBarrel2D {name}: explosion. " +
                $"Element={explosionElement}, Damage={damage}, Radius={radius}");
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, targetMask);

        foreach (Collider2D hit in hits)
        {
            if (!TrapTargetUtility2D.IsValidTarget(hit, targetMask))
                continue;

            TrapTargetUtility2D.ApplyDamageAndStatus(hit, damage, explosionElement);
        }

        if (explosionVisualPrefab != null)
            Instantiate(explosionVisualPrefab, transform.position, Quaternion.identity);

        if (normalVisual != null)
            normalVisual.SetActive(false);

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}