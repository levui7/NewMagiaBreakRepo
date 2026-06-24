using UnityEngine;

public static class TrapTargetUtility2D
{
    public static bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        if (obj == null)
            return false;

        return (mask.value & (1 << obj.layer)) != 0;
    }

    public static bool IsValidTarget(Collider2D collider, LayerMask targetMask)
    {
        if (collider == null)
            return false;

        if (!IsInLayerMask(collider.gameObject, targetMask))
            return false;

        if (collider.GetComponentInParent<PlayerController>() != null)
            return true;

        if (collider.GetComponentInParent<Enemy>() != null)
            return true;

        if (collider.GetComponentInParent<BossController>() != null)
            return true;

        return false;
    }

    public static void ApplyDamage(Collider2D collider, int damage, Element element)
    {
        if (collider == null)
            return;

        if (damage <= 0)
            return;

        Enemy enemy = collider.GetComponentInParent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage, element);
            return;
        }

        BossController boss = collider.GetComponentInParent<BossController>();

        if (boss != null)
        {
            boss.TakeDamage(damage, element);
            return;
        }

        PlayerController player = collider.GetComponentInParent<PlayerController>();

        if (player != null)
        {
            player.TakeDamage(damage, element);
            return;
        }
    }

    public static void ApplyStatus(Collider2D collider, Element element)
    {
        if (collider == null)
            return;

        if (element == Element.Physical)
            return;

        StatusEffectController status = collider.GetComponentInParent<StatusEffectController>();

        if (status != null)
            status.ApplyElementStatus(element);
    }

    public static void ApplyDamageAndStatus(Collider2D collider, int damage, Element element)
    {
        ApplyDamage(collider, damage, element);
        ApplyStatus(collider, element);
    }

    public static Rigidbody2D GetTargetRigidbody(Collider2D collider)
    {
        if (collider == null)
            return null;

        PlayerController player = collider.GetComponentInParent<PlayerController>();

        if (player != null)
            return player.GetComponent<Rigidbody2D>();

        Enemy enemy = collider.GetComponentInParent<Enemy>();

        if (enemy != null)
            return enemy.GetComponent<Rigidbody2D>();

        BossController boss = collider.GetComponentInParent<BossController>();

        if (boss != null)
            return boss.GetComponent<Rigidbody2D>();

        return collider.attachedRigidbody;
    }

    public static Transform GetTargetRoot(Collider2D collider)
    {
        if (collider == null)
            return null;

        PlayerController player = collider.GetComponentInParent<PlayerController>();

        if (player != null)
            return player.transform;

        Enemy enemy = collider.GetComponentInParent<Enemy>();

        if (enemy != null)
            return enemy.transform;

        BossController boss = collider.GetComponentInParent<BossController>();

        if (boss != null)
            return boss.transform;

        return collider.transform;
    }

    public static void AddForce(Collider2D collider, Vector2 force, ForceMode2D mode)
    {
        Rigidbody2D rb = GetTargetRigidbody(collider);

        if (rb != null)
            rb.AddForce(force, mode);
    }
}