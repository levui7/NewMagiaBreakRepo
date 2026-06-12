using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DestructibleWall2D : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;
    public int currentHealth = 3;

    [Header("Drop / Reveal")]
    public GameObject secretRoomEntrance;
    public GameObject destroyedVisual;

    [Header("Destroy Settings")]
    public bool destroyColliderOnDeath = true;
    public bool disableSpriteOnDeath = true;

    private bool isDestroyed;

    private void Awake()
    {
        currentHealth = maxHealth;

        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = false;
    }

    public void TakeDamage(int amount)
    {
        if (isDestroyed)
            return;

        if (amount <= 0)
            return;

        currentHealth -= amount;

        if (currentHealth <= 0)
            BreakWall();
    }

    private void BreakWall()
    {
        if (isDestroyed)
            return;

        isDestroyed = true;

        if (destroyedVisual != null)
            destroyedVisual.SetActive(true);

        if (secretRoomEntrance != null)
            secretRoomEntrance.SetActive(true);

        if (disableSpriteOnDeath)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.enabled = false;
        }

        if (destroyColliderOnDeath)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;
        }

        DamagePopup2D.SpawnText(transform.position + Vector3.up * 0.5f, "CRACK", Color.gray);
    }
}