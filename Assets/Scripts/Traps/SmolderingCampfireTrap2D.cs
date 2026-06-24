using UnityEngine;

public class SmolderingCampfireTrap2D : MonoBehaviour
{
    [Header("Effect")]
    public int damagePerTick = 1;
    public float tickInterval = 1f;
    public LayerMask targetMask;

    private HazardZone2D hazardZone;

    private void Awake()
    {
        hazardZone = GetComponent<HazardZone2D>();

        if (hazardZone == null)
            hazardZone = gameObject.AddComponent<HazardZone2D>();

        hazardZone.statusElement = Element.Smoldering;
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
            hazardZone.statusElement = Element.Smoldering;
            hazardZone.damagePerTick = damagePerTick;
            hazardZone.tickInterval = tickInterval;
            hazardZone.targetMask = targetMask;
        }
    }
}