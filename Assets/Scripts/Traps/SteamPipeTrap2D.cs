using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamPipeTrap2D : MonoBehaviour
{
    [Header("Cycle")]
    public float delayBeforeFirstEmission = 1f;
    public float inactiveDuration = 3f;
    public float activeDuration = 2f;

    [Header("Damage")]
    public int damagePerTick = 2;
    public float tickInterval = 0.5f;
    public LayerMask targetMask;

    [Header("Area")]
    public Collider2D steamTriggerArea;

    [Header("Visual")]
    public GameObject steamVisual;

    [Header("Audio")]
    public AudioClip steamSound;

    [Header("Debug")]
    public bool logDebug = true;

    private bool isEmitting;
    private readonly Dictionary<Transform, float> nextTickTimeByTarget = new Dictionary<Transform, float>();
    private AudioSource audioSource;

    private void Start()
    {
        if (steamTriggerArea != null)
            steamTriggerArea.isTrigger = true;

        SetSteamActive(false);
        StartCoroutine(SteamCycle());

        if (steamSound != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.clip = steamSound;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.volume = 0.6f;
            audioSource.spatialBlend = 1f;
        }
    }

    private IEnumerator SteamCycle()
    {
        yield return new WaitForSeconds(delayBeforeFirstEmission);

        while (true)
        {
            SetSteamActive(true);
            yield return new WaitForSeconds(activeDuration);

            SetSteamActive(false);
            yield return new WaitForSeconds(inactiveDuration);
        }
    }

    private void SetSteamActive(bool active)
    {
        isEmitting = active;

        if (steamVisual != null)
            steamVisual.SetActive(active);

        if (audioSource != null)
        {
            if (active)
                audioSource.Play();
            else
                audioSource.Stop();
        }

        if (logDebug)
            Debug.Log($"SteamPipeTrap2D {name}: emitting={active}");
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isEmitting)
            return;

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

        TrapTargetUtility2D.ApplyDamageAndStatus(other, damagePerTick, Element.Steam);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Transform root = TrapTargetUtility2D.GetTargetRoot(other);

        if (root != null && nextTickTimeByTarget.ContainsKey(root))
            nextTickTimeByTarget.Remove(root);
    }
}