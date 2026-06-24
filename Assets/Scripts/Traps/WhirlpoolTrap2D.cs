using System.Collections.Generic;
using UnityEngine;

public class WhirlpoolTrap2D : MonoBehaviour
{
    [Header("Pull")]
    public float pullForce = 12f;
    public float pullDuration = 1.5f;

    [Header("Push")]
    public float pushForce = 16f;
    public int damage = 12;
    public float cooldownAfterPush = 2f;

    [Header("Effect")]
    public bool applyWaterStatus = true;

    [Header("Targets")]
    public LayerMask targetMask;

    [Header("Debug")]
    public bool logDebug = true;

    private readonly Dictionary<Transform, float> enterTimeByTarget = new Dictionary<Transform, float>();
    private readonly Dictionary<Transform, float> nextAllowedPushTimeByTarget = new Dictionary<Transform, float>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!TrapTargetUtility2D.IsValidTarget(other, targetMask))
            return;

        Transform root = TrapTargetUtility2D.GetTargetRoot(other);

        if (root == null)
            return;

        enterTimeByTarget[root] = Time.time;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!TrapTargetUtility2D.IsValidTarget(other, targetMask))
            return;

        Transform root = TrapTargetUtility2D.GetTargetRoot(other);

        if (root == null)
            return;

        if (!enterTimeByTarget.ContainsKey(root))
            enterTimeByTarget[root] = Time.time;

        if (!nextAllowedPushTimeByTarget.ContainsKey(root))
            nextAllowedPushTimeByTarget[root] = 0f;

        if (Time.time < nextAllowedPushTimeByTarget[root])
            return;

        float timeInside = Time.time - enterTimeByTarget[root];

        if (timeInside < pullDuration)
        {
            PullTarget(other);
        }
        else
        {
            PushTarget(other);
            nextAllowedPushTimeByTarget[root] = Time.time + cooldownAfterPush;
            enterTimeByTarget[root] = Time.time;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Transform root = TrapTargetUtility2D.GetTargetRoot(other);

        if (root == null)
            return;

        if (enterTimeByTarget.ContainsKey(root))
            enterTimeByTarget.Remove(root);
    }

    private void PullTarget(Collider2D other)
    {
        Rigidbody2D rb = TrapTargetUtility2D.GetTargetRigidbody(other);

        if (rb == null)
            return;

        Vector2 directionToCenter = ((Vector2)transform.position - rb.position).normalized;
        rb.AddForce(directionToCenter * pullForce, ForceMode2D.Force);
    }

    private void PushTarget(Collider2D other)
    {
        Rigidbody2D rb = TrapTargetUtility2D.GetTargetRigidbody(other);

        if (rb != null)
        {
            Vector2 directionFromCenter = (rb.position - (Vector2)transform.position).normalized;

            if (directionFromCenter.sqrMagnitude <= 0.001f)
                directionFromCenter = Vector2.up;

            rb.AddForce(directionFromCenter * pushForce, ForceMode2D.Impulse);
        }

        TrapTargetUtility2D.ApplyDamage(other, damage, Element.Water);

        if (applyWaterStatus)
            TrapTargetUtility2D.ApplyStatus(other, Element.Water);

        if (logDebug)
            Debug.Log($"WhirlpoolTrap2D {name}: pushed target {other.name}");
    }
}