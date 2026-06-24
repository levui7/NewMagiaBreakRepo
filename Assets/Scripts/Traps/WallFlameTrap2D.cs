using System.Collections;
using UnityEngine;

public class WallFlameTrap2D : MonoBehaviour
{
    [Header("Mode")]
    public bool useCycle = true;

    [Header("Cycle")]
    public float startDelay = 0f;
    public float activeDuration = 2f;
    public float inactiveDuration = 2f;

    [Header("Effect")]
    public int damagePerTick = 2;
    public float tickInterval = 0.5f;
    public LayerMask targetMask;

    [Header("Visual")]
    public GameObject flameVisual;

    [Header("Collider")]
    public Collider2D flameTrigger;

    [Header("Debug")]
    public bool logDebug = true;

    private HazardZone2D hazardZone;

    private void Awake()
    {
        if (flameTrigger == null)
            flameTrigger = GetComponent<Collider2D>();

        if (flameTrigger != null)
            flameTrigger.isTrigger = true;

        hazardZone = GetComponent<HazardZone2D>();

        if (hazardZone == null)
            hazardZone = gameObject.AddComponent<HazardZone2D>();

        hazardZone.statusElement = Element.Fire;
        hazardZone.damagePerTick = damagePerTick;
        hazardZone.tickInterval = tickInterval;
        hazardZone.destroyAfterLifetime = false;
        hazardZone.targetMask = targetMask;
    }

    private void Start()
    {
        if (useCycle)
            StartCoroutine(FlameCycle());
        else
            SetActiveState(true);
    }

    private IEnumerator FlameCycle()
    {
        SetActiveState(false);

        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        while (true)
        {
            SetActiveState(true);
            yield return new WaitForSeconds(activeDuration);

            SetActiveState(false);
            yield return new WaitForSeconds(inactiveDuration);
        }
    }

    private void SetActiveState(bool active)
    {
        if (flameTrigger != null)
            flameTrigger.enabled = active;

        if (hazardZone != null)
            hazardZone.enabled = active;

        if (flameVisual != null)
            flameVisual.SetActive(active);

        if (logDebug)
            Debug.Log($"WallFlameTrap2D {name}: active={active}");
    }
}