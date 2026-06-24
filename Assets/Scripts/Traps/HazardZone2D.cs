using System.Collections.Generic;
using UnityEngine;

public class HazardZone2D : MonoBehaviour
{
    [Header("Effect")]
    public Element statusElement = Element.Fire;
    public int damagePerTick = 0;
    public float tickInterval = 1f;

    [Header("Lifetime")]
    public bool destroyAfterLifetime = false;
    public float lifetime = 5f;

    [Header("Targets")]
    public LayerMask targetMask;

    [Header("Debug")]
    public bool logDebug = false;

    private readonly Dictionary<Transform, float> nextTickTimeByTarget = new Dictionary<Transform, float>();

    private void Start()
    {
        if (destroyAfterLifetime)
            Destroy(gameObject, lifetime);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!TrapTargetUtility2D.IsValidTarget(other, targetMask))
            return;

        Transform root = TrapTargetUtility2D.GetTargetRoot(other);

        if (root == null)
            return;

        if (!nextTickTimeByTarget.ContainsKey(root))
            nextTickTimeByTarget[root] = 0f;

        if (Time.time < nextTickTimeByTarget[root])
            return;

        nextTickTimeByTarget[root] = Time.time + tickInterval;

        TrapTargetUtility2D.ApplyDamageAndStatus(other, damagePerTick, statusElement);

        if (logDebug)
        {
            Debug.Log(
                $"HazardZone2D {name}: tick target={root.name}, " +
                $"damage={damagePerTick}, status={statusElement}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Transform root = TrapTargetUtility2D.GetTargetRoot(other);

        if (root != null && nextTickTimeByTarget.ContainsKey(root))
            nextTickTimeByTarget.Remove(root);
    }
}