using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MaterialPickup2D : MonoBehaviour
{
    [Header("Material")]
    public PlayerProgressManager.MaterialType materialType = PlayerProgressManager.MaterialType.Coins;
    public int amount = 1;

    [Header("Magnet")]
    public bool useMagnet = true;
    public float magnetRadius = 3f;
    public float magnetSpeed = 6f;

    [Header("Collect")]
    public float collectDistance = 0.35f;
    public bool debugPickup = false;

    private Transform targetPlayer;
    private bool collected;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Update()
    {
        if (collected)
            return;

        FindNearestPlayer();

        if (targetPlayer == null)
            return;

        float distance = Vector2.Distance(transform.position, targetPlayer.position);

        if (useMagnet && distance <= magnetRadius)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                targetPlayer.position,
                magnetSpeed * Time.deltaTime
            );
        }

        // Надёжный подбор без ожидания OnTriggerEnter2D.
        if (distance <= collectDistance)
        {
            PlayerController player = targetPlayer.GetComponent<PlayerController>();

            if (player == null)
                player = targetPlayer.GetComponentInParent<PlayerController>();

            if (player != null)
                Collect(player);
        }
    }

    private void FindNearestPlayer()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        float bestDistance = float.MaxValue;
        Transform bestPlayer = null;

        foreach (PlayerController player in players)
        {
            if (player == null)
                continue;

            if (!player.gameObject.activeInHierarchy)
                continue;

            float distance = Vector2.Distance(transform.position, player.transform.position);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestPlayer = player.transform;
            }
        }

        targetPlayer = bestPlayer;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
            return;

        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        Collect(player);
    }

    private void Collect(PlayerController player)
    {
        if (collected)
            return;

        collected = true;

        PlayerProgressManager progress = PlayerProgressManager.Instance;

        // На случай, если уровень запущен напрямую, без Lobby.
        if (progress == null)
        {
            GameObject progressObject = new GameObject("PlayerProgressManager_Runtime");
            progress = progressObject.AddComponent<PlayerProgressManager>();
        }

        progress.AddMaterial(materialType, amount);

        if (debugPickup)
        {
            Debug.Log(
                $"MaterialPickup2D: collected {amount} {materialType}. Coins={progress.coins}, Crystals={progress.crystals}",
                this
            );
        }

        if (MaterialsHUD2D.Instance != null)
            MaterialsHUD2D.Instance.Refresh();

        Destroy(gameObject);
    }
}