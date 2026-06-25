//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class FloorSpikesTrap2D : MonoBehaviour
//{
//    [Header("Cycle")]
//    public float startDelay = 0f;
//    public float hiddenDuration = 2f;
//    public float warningDuration = 0.5f;
//    public float activeDuration = 1f;

//    [Header("Damage")]
//    public int damage = 15;
//    public float damageCooldownPerTarget = 0.8f;
//    public LayerMask targetMask;

//    [Header("Visual")]
//    public GameObject hiddenVisual;
//    public GameObject warningVisual;
//    public GameObject activeVisual;

//    [Header("Collider")]
//    public Collider2D damageTrigger;

//    [Header("Debug")]
//    public bool logDebug = true;

//    private bool isActive;
//    private readonly Dictionary<Transform, float> nextDamageTimeByTarget = new Dictionary<Transform, float>();

//    private void Start()
//    {
//        if (damageTrigger == null)
//            damageTrigger = GetComponent<Collider2D>();

//        if (damageTrigger != null)
//            damageTrigger.isTrigger = true;

//        StartCoroutine(SpikeCycle());
//    }

//    private IEnumerator SpikeCycle()
//    {
//        SetState(false, false);

//        if (startDelay > 0f)
//            yield return new WaitForSeconds(startDelay);

//        while (true)
//        {
//            SetState(false, false);
//            yield return new WaitForSeconds(hiddenDuration);

//            SetState(false, true);
//            yield return new WaitForSeconds(warningDuration);

//            SetState(true, false);
//            yield return new WaitForSeconds(activeDuration);
//        }
//    }

//    private void SetState(bool active, bool warning)
//    {
//        isActive = active;

//        if (hiddenVisual != null)
//            hiddenVisual.SetActive(!active && !warning);

//        if (warningVisual != null)
//            warningVisual.SetActive(warning);

//        if (activeVisual != null)
//            activeVisual.SetActive(active);

//        if (logDebug)
//            Debug.Log($"FloorSpikesTrap2D {name}: active={active}, warning={warning}");
//    }

//    private void OnTriggerStay2D(Collider2D other)
//    {
//        if (!isActive)
//            return;

//        if (!TrapTargetUtility2D.IsValidTarget(other, targetMask))
//            return;

//        Transform root = TrapTargetUtility2D.GetTargetRoot(other);

//        if (root == null)
//            return;

//        if (!nextDamageTimeByTarget.ContainsKey(root))
//            nextDamageTimeByTarget[root] = 0f;

//        if (Time.time < nextDamageTimeByTarget[root])
//            return;

//        nextDamageTimeByTarget[root] = Time.time + damageCooldownPerTarget;

//        TrapTargetUtility2D.ApplyDamage(other, damage, Element.Physical);
//    }

//    private void OnTriggerExit2D(Collider2D other)
//    {
//        Transform root = TrapTargetUtility2D.GetTargetRoot(other);

//        if (root != null && nextDamageTimeByTarget.ContainsKey(root))
//            nextDamageTimeByTarget.Remove(root);
//    }
//}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorSpikesTrap2D : MonoBehaviour
{
    [Header("Cycle")]
    public float startDelay = 0f;
    public float hiddenDuration = 2f;
    public float warningDuration = 0.5f;
    public float activeDuration = 1f;

    [Header("Damage")]
    public int damage = 15;
    public float damageCooldownPerTarget = 0.8f;
    public LayerMask targetMask;

    [Header("Visual")]
    public GameObject hiddenVisual;
    public GameObject warningVisual;
    public GameObject activeVisual;

    [Header("Audio")]
    [Tooltip("Звук выдвижения шипов")]
    public AudioClip extendSound;

    [Tooltip("Звук втягивания шипов")]
    public AudioClip retractSound;

    [Tooltip("Звук предупреждения")]
    public AudioClip warningSound;

    [Range(0f, 1f)]
    public float volume = 0.7f;

    [Header("Collider")]
    public Collider2D damageTrigger;

    [Header("Debug")]
    public bool logDebug = true;

    private bool isActive;
    private readonly Dictionary<Transform, float> nextDamageTimeByTarget = new Dictionary<Transform, float>();
    private AudioSource audioSource;

    private void Start()
    {
        if (damageTrigger == null)
            damageTrigger = GetComponent<Collider2D>();

        if (damageTrigger != null)
            damageTrigger.isTrigger = true;

        // ✅ Настраиваем звук
        SetupAudio();

        StartCoroutine(SpikeCycle());
    }

    private void SetupAudio()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.volume = volume;
        audioSource.spatialBlend = 1f;  // 3D звук
        audioSource.minDistance = 5f;
        audioSource.maxDistance = 15f;
        audioSource.playOnAwake = false;
    }

    private IEnumerator SpikeCycle()
    {
        SetState(false, false);

        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        while (true)
        {
            // HIDDEN → WARNING
            SetState(false, false);
            yield return new WaitForSeconds(hiddenDuration);

            // ✅ Звук предупреждения
            PlaySound(warningSound);

            SetState(false, true);
            yield return new WaitForSeconds(warningDuration);

            // ✅ Звук выдвижения шипов
            PlaySound(extendSound);

            SetState(true, false);
            yield return new WaitForSeconds(activeDuration);

            // ✅ Звук втягивания шипов
            PlaySound(retractSound);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void SetState(bool active, bool warning)
    {
        isActive = active;

        if (hiddenVisual != null)
            hiddenVisual.SetActive(!active && !warning);

        if (warningVisual != null)
            warningVisual.SetActive(warning);

        if (activeVisual != null)
            activeVisual.SetActive(active);

        if (logDebug)
            Debug.Log($"FloorSpikesTrap2D {name}: active={active}, warning={warning}");
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isActive)
            return;

        if (!TrapTargetUtility2D.IsValidTarget(other, targetMask))
            return;

        Transform root = TrapTargetUtility2D.GetTargetRoot(other);

        if (root == null)
            return;

        if (!nextDamageTimeByTarget.ContainsKey(root))
            nextDamageTimeByTarget[root] = 0f;

        if (Time.time < nextDamageTimeByTarget[root])
            return;

        nextDamageTimeByTarget[root] = Time.time + damageCooldownPerTarget;

        TrapTargetUtility2D.ApplyDamage(other, damage, Element.Physical);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Transform root = TrapTargetUtility2D.GetTargetRoot(other);

        if (root != null && nextDamageTimeByTarget.ContainsKey(root))
            nextDamageTimeByTarget.Remove(root);
    }
}