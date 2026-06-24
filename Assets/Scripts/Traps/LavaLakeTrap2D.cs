using UnityEngine;

public class LavaLakeTrap2D : MonoBehaviour
{
    [Header("Effect")]
    public int damagePerTick = 3;
    public float tickInterval = 0.7f;
    public LayerMask targetMask;

    private HazardZone2D hazardZone;

    private void Awake()
    {
        hazardZone = GetComponent<HazardZone2D>();

        if (hazardZone == null)
            hazardZone = gameObject.AddComponent<HazardZone2D>();

        hazardZone.statusElement = Element.Fire;
        hazardZone.damagePerTick = damagePerTick;
        hazardZone.tickInterval = tickInterval;
        hazardZone.destroyAfterLifetime = false;
        hazardZone.targetMask = targetMask;
    }

    private void OnValidate()
    {
        if (hazardZone == null)
            hazardZone = GetComponent<HazardZone2D>();

        if (hazardZone != null)
        {
            hazardZone.statusElement = Element.Fire;
            hazardZone.damagePerTick = damagePerTick;
            hazardZone.tickInterval = tickInterval;
            hazardZone.targetMask = targetMask;
        }
    }
}