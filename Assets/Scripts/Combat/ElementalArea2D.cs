using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ElementalArea2D : MonoBehaviour
{
    [Header("Эффект зоны")]
    public WeaponManager.Element element = WeaponManager.Element.Fire;
    public int enterDamage = 0;
    public int periodicDamage = 1;
    public float tickInterval = 1f;
    public bool affectPlayers = true;
    public bool affectEnemies = true;

    private float nextTickTime;

    private void Awake()
    {
        Collider2D areaCollider = GetComponent<Collider2D>();
        areaCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ApplyToTarget(other, enterDamage, true);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time < nextTickTime)
            return;

        nextTickTime = Time.time + tickInterval;

        ApplyToTarget(other, periodicDamage, false);
    }

    private void ApplyToTarget(Collider2D other, int damage, bool applyStatus)
    {
        if (affectPlayers)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null) player = other.GetComponentInParent<PlayerController>();

            if (player != null)
            {
                if (damage > 0)
                    player.TakeDamage(damage, element);

                if (applyStatus)
                {
                    StatusEffectController status =
                        player.GetComponent<StatusEffectController>();

                    if (status != null)
                        status.ApplyElementStatus(element);
                }

                return;
            }
        }

        if (affectEnemies)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy == null) enemy = other.GetComponentInParent<Enemy>();

            if (enemy != null)
            {
                if (damage > 0)
                    enemy.TakeDamage(damage, element);

                if (applyStatus)
                {
                    StatusEffectController status =
                        enemy.GetComponent<StatusEffectController>();

                    if (status != null)
                        status.ApplyElementStatus(element);
                }
            }
        }
    }
}
