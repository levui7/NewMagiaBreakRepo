using UnityEngine;

public class LavaLakeTrap2D : MonoBehaviour
{
    [Header("Effect")]
    public int damagePerTick = 3;
    public float tickInterval = 0.7f;
    public LayerMask targetMask;

    [Header("Audio")]
    [Tooltip("Звук лавы")]
    public AudioClip lavaSound;

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

        if (lavaSound != null)
        {
            AudioSource audio = GetComponent<AudioSource>();
            if (audio == null) audio = gameObject.AddComponent<AudioSource>();

            audio.clip = lavaSound;
            audio.loop = true;
            audio.playOnAwake = true;
            audio.volume = 0.7f;
            audio.spatialBlend = 1f;
            audio.Play();
        }
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