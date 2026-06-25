//using System.Collections.Generic;
//using UnityEngine;

//public class WhirlpoolTrap2D : MonoBehaviour
//{
//    [Header("Pull")]
//    public float pullForce = 12f;
//    public float pullDuration = 1.5f;

//    [Header("Push")]
//    public float pushForce = 16f;
//    public int damage = 12;
//    public float cooldownAfterPush = 2f;

//    [Header("Effect")]
//    public bool applyWaterStatus = true;

//    [Header("Targets")]
//    public LayerMask targetMask;

//    [Header("Debug")]
//    public bool logDebug = true;

//    private readonly Dictionary<Transform, float> enterTimeByTarget = new Dictionary<Transform, float>();
//    private readonly Dictionary<Transform, float> nextAllowedPushTimeByTarget = new Dictionary<Transform, float>();

//    private void OnTriggerEnter2D(Collider2D other)
//    {
//        if (!TrapTargetUtility2D.IsValidTarget(other, targetMask))
//            return;

//        Transform root = TrapTargetUtility2D.GetTargetRoot(other);

//        if (root == null)
//            return;

//        enterTimeByTarget[root] = Time.time;
//    }

//    private void OnTriggerStay2D(Collider2D other)
//    {
//        if (!TrapTargetUtility2D.IsValidTarget(other, targetMask))
//            return;

//        Transform root = TrapTargetUtility2D.GetTargetRoot(other);

//        if (root == null)
//            return;

//        if (!enterTimeByTarget.ContainsKey(root))
//            enterTimeByTarget[root] = Time.time;

//        if (!nextAllowedPushTimeByTarget.ContainsKey(root))
//            nextAllowedPushTimeByTarget[root] = 0f;

//        if (Time.time < nextAllowedPushTimeByTarget[root])
//            return;

//        float timeInside = Time.time - enterTimeByTarget[root];

//        if (timeInside < pullDuration)
//        {
//            PullTarget(other);
//        }
//        else
//        {
//            PushTarget(other);
//            nextAllowedPushTimeByTarget[root] = Time.time + cooldownAfterPush;
//            enterTimeByTarget[root] = Time.time;
//        }
//    }

//    private void OnTriggerExit2D(Collider2D other)
//    {
//        Transform root = TrapTargetUtility2D.GetTargetRoot(other);

//        if (root == null)
//            return;

//        if (enterTimeByTarget.ContainsKey(root))
//            enterTimeByTarget.Remove(root);
//    }

//    private void PullTarget(Collider2D other)
//    {
//        Rigidbody2D rb = TrapTargetUtility2D.GetTargetRigidbody(other);

//        if (rb == null)
//            return;

//        Vector2 directionToCenter = ((Vector2)transform.position - rb.position).normalized;
//        rb.AddForce(directionToCenter * pullForce, ForceMode2D.Force);
//    }

//    private void PushTarget(Collider2D other)
//    {
//        Rigidbody2D rb = TrapTargetUtility2D.GetTargetRigidbody(other);

//        if (rb != null)
//        {
//            Vector2 directionFromCenter = (rb.position - (Vector2)transform.position).normalized;

//            if (directionFromCenter.sqrMagnitude <= 0.001f)
//                directionFromCenter = Vector2.up;

//            rb.AddForce(directionFromCenter * pushForce, ForceMode2D.Impulse);
//        }

//        TrapTargetUtility2D.ApplyDamage(other, damage, Element.Water);

//        if (applyWaterStatus)
//            TrapTargetUtility2D.ApplyStatus(other, Element.Water);

//        if (logDebug)
//            Debug.Log($"WhirlpoolTrap2D {name}: pushed target {other.name}");
//    }
//}

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

    [Header("Visual Rotation")]
    [Tooltip("Объект для вращения (спрайт воронки). Если не назначен, будет вращаться этот объект")]
    public GameObject rotatingVisual;

    [Tooltip("Скорость вращения в градусах в секунду")]
    public float rotationSpeed = 180f;

    [Tooltip("Направление: true = по часовой, false = против")]
    public bool clockwise = true;

    [Tooltip("Ускорение вращения при активации (плавный старт)")]
    public bool smoothStart = true;

    [Tooltip("Время разгона до полной скорости")]
    public float smoothStartDuration = 0.5f;

    [Header("Audio")]
    public AudioClip whirlpoolSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.6f;

    private AudioSource audioSource;

    [Header("Debug")]
    public bool logDebug = true;

    private readonly Dictionary<Transform, float> enterTimeByTarget = new Dictionary<Transform, float>();
    private readonly Dictionary<Transform, float> nextAllowedPushTimeByTarget = new Dictionary<Transform, float>();

    private float currentRotationSpeed;
    private float timeSinceActivation;
    private bool isPulled;

    private void Start()
    {
        // Если не назначен объект для вращения, используем этот
        if (rotatingVisual == null)
            rotatingVisual = gameObject;

        currentRotationSpeed = 0f;
        timeSinceActivation = 0f;
        isPulled = false;

        if (whirlpoolSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = whirlpoolSound;
            audioSource.loop = true;
            audioSource.volume = soundVolume;
            audioSource.spatialBlend = 1f;
            audioSource.Play();
        }
    }

    private void Update()
    {
        // ✅ Вращаем воронку
        RotateWhirlpool();
    }

    private void RotateWhirlpool()
    {
        if (rotatingVisual == null)
            return;

        // Плавный старт вращения
        if (smoothStart && timeSinceActivation < smoothStartDuration)
        {
            timeSinceActivation += Time.deltaTime;
            float t = timeSinceActivation / smoothStartDuration;
            currentRotationSpeed = Mathf.Lerp(0f, rotationSpeed, t);
        }
        else
        {
            currentRotationSpeed = rotationSpeed;
        }

        // Вращаем
        float direction = clockwise ? -1f : 1f; // В 2D минус = по часовой
        rotatingVisual.transform.Rotate(0f, 0f, direction * currentRotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!TrapTargetUtility2D.IsValidTarget(other, targetMask))
            return;

        Transform root = TrapTargetUtility2D.GetTargetRoot(other);

        if (root == null)
            return;

        enterTimeByTarget[root] = Time.time;
        isPulled = true;

        // Ускоряем вращение когда кто-то попал в воронку
        if (smoothStart)
        {
            timeSinceActivation = smoothStartDuration;
            currentRotationSpeed = rotationSpeed;
        }
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

            // Сбрасываем ускорение после выброса
            isPulled = false;
            if (smoothStart)
            {
                timeSinceActivation = 0f;
                currentRotationSpeed = 0f;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Transform root = TrapTargetUtility2D.GetTargetRoot(other);

        if (root == null)
            return;

        if (enterTimeByTarget.ContainsKey(root))
            enterTimeByTarget.Remove(root);

        // Сбрасываем скорость вращения когда все вышли
        if (enterTimeByTarget.Count == 0)
        {
            isPulled = false;
            if (smoothStart)
            {
                timeSinceActivation = 0f;
                currentRotationSpeed = 0f;
            }
        }
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