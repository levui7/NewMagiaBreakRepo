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

        ApplyBuff(player);

        if (affectAllPlayers)
        {
            PlayerController[] players = FindObjectsOfType<PlayerController>();
            foreach (PlayerController p in players)
            {
                if (p != null && p != player)
                    ApplyBuff(p);
            }
        }

        if (destroyAfterPickup)
            Destroy(gameObject);
    }

    private void ApplyBuff(PlayerController player)
    {
        if (player == null)
            return;

        TemporaryBuffController2D buff = player.GetComponent<TemporaryBuffController2D>();
        if (buff == null)
            buff = player.gameObject.AddComponent<TemporaryBuffController2D>();

        switch (buffType)
        {
            case PickupBuffType.Power:
                buff.AddPowerBuff(duration, multiplier);
                break;

            case PickupBuffType.Speed:
                buff.AddSpeedBuff(duration, multiplier);
                break;

            case PickupBuffType.PhysicalDamage:
                buff.AddPhysicalDamageBuff(duration, multiplier);
                break;

            case PickupBuffType.FireDamage:
                buff.AddFireDamageBuff(duration, multiplier);
                break;

            case PickupBuffType.WaterDamage:
                buff.AddWaterDamageBuff(duration, multiplier);
                break;
        }
    }
}