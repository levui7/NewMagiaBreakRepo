using UnityEngine;

public class IceMine2D : MonoBehaviour
{
    [Header("Explosion")]
    public int explosionDamage = 5;
    public float explosionRadius = 1.5f;
    public LayerMask targetMask;

    [Header("Ice Zone")]
    public GameObject iceZonePrefab;
    public float iceZoneRadius = 2.5f;
    public float iceZoneLifetime = 5f;
    public float iceTickInterval = 1f;

    [Header("Visual")]
    [Tooltip("Префаб с партиклами взрыва")]
    public GameObject explosionVisualPrefab;

    [Tooltip("Префаб вспышки")]
    public GameObject flashPrefab;

    [Header("Audio")]
    public AudioClip explosionSound;
    [Range(0f, 1f)]
    public float explosionVolume = 0.8f;

    [Header("Debug")]
    public bool logDebug = true;

    private bool exploded;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (exploded)
            return;

        if (!TrapTargetUtility2D.IsValidTarget(other, targetMask))
            return;

        Explode();
    }

    private void Explode()
    {
        if (exploded)
            return;

        exploded = true;

        if (logDebug)
            Debug.Log($"IceMine2D {name}: exploded");

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, targetMask);

        foreach (Collider2D hit in hits)
        {
            if (!TrapTargetUtility2D.IsValidTarget(hit, targetMask))
                continue;

            TrapTargetUtility2D.ApplyDamageAndStatus(hit, explosionDamage, Element.Water);
        }

        // 2. НОВОЕ: Создаём вспышку (быстрый спрайт)
        if (flashPrefab != null)
        {
            Instantiate(flashPrefab, transform.position, Quaternion.identity);
        }

        // 3. НОВОЕ: Создаём партиклы взрыва
        if (explosionVisualPrefab != null)
        {
            Instantiate(explosionVisualPrefab, transform.position, Quaternion.identity);
        }

        // 4. НОВОЕ: Проигрываем звук
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, explosionVolume);
        }

        SpawnIceZone();

        if (explosionVisualPrefab != null)
            Instantiate(explosionVisualPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    private void SpawnIceZone()
    {
        GameObject zone;

        if (iceZonePrefab != null)
        {
            zone = Instantiate(iceZonePrefab, transform.position, Quaternion.identity);
        }
        else
        {
            zone = new GameObject("Runtime_IceZone");
            zone.transform.position = transform.position;

            CircleCollider2D collider = zone.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = iceZoneRadius;

            HazardZone2D hazard = zone.AddComponent<HazardZone2D>();
            hazard.statusElement = Element.Water;
            hazard.damagePerTick = 0;
            hazard.tickInterval = iceTickInterval;
            hazard.destroyAfterLifetime = true;
            hazard.lifetime = iceZoneLifetime;
            hazard.targetMask = targetMask;
        }

        CircleCollider2D zoneCollider = zone.GetComponent<CircleCollider2D>();

        if (zoneCollider != null)
        {
            zoneCollider.isTrigger = true;
            zoneCollider.radius = iceZoneRadius;
        }

        HazardZone2D zoneHazard = zone.GetComponent<HazardZone2D>();

        if (zoneHazard != null)
        {
            zoneHazard.statusElement = Element.Water;
            zoneHazard.damagePerTick = 0;
            zoneHazard.tickInterval = iceTickInterval;
            zoneHazard.destroyAfterLifetime = true;
            zoneHazard.lifetime = iceZoneLifetime;
            zoneHazard.targetMask = targetMask;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
        Gizmos.DrawWireSphere(transform.position, iceZoneRadius);
    }
}