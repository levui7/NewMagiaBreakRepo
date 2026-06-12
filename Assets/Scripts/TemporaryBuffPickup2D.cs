using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TemporaryBuffPickup2D : MonoBehaviour
{
    public enum PickupBuffType
    {
        Power,
        Speed,
        PhysicalDamage,
        FireDamage,
        WaterDamage
    }

    [Header("Buff")]
    public PickupBuffType buffType = PickupBuffType.Power;
    public float duration = 12f;
    public float multiplier = 1.25f;

    [Header("Target")]
    public bool affectAllPlayers = false;

    [Header("Destroy")]
    public bool destroyAfterPickup = true;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null)
            return;

        ApplyToPlayer(player);

        if (affectAllPlayers)
        {
            PlayerController[] players = FindObjectsOfType<PlayerController>();
            foreach (PlayerController p in players)
            {
                if (p != null && p != player)
                    ApplyToPlayer(p);
            }
        }

        if (destroyAfterPickup)
            Destroy(gameObject);
    }

    private void ApplyToPlayer(PlayerController player)
    {
        TemporaryBuffController2D buffs = player.GetComponent<TemporaryBuffController2D>();
        if (buffs == null)
            buffs = player.gameObject.AddComponent<TemporaryBuffController2D>();

        switch (buffType)
        {
            case PickupBuffType.Power:
                buffs.AddPowerBuff(duration, multiplier);
                break;
            case PickupBuffType.Speed:
                buffs.AddSpeedBuff(duration, multiplier);
                break;
            case PickupBuffType.PhysicalDamage:
                buffs.AddPhysicalDamageBuff(duration, multiplier);
                break;
            case PickupBuffType.FireDamage:
                buffs.AddFireDamageBuff(duration, multiplier);
                break;
            case PickupBuffType.WaterDamage:
                buffs.AddWaterDamageBuff(duration, multiplier);
                break;
        }

        DamagePopup2D.SpawnStatus(player.transform.position, buffType.ToString(), Color.yellow);
    }
}