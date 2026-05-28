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

    private Transform targetPlayer;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Update()
    {
        if (!useMagnet)
            return;

        FindNearestPlayer();

        if (targetPlayer == null)
            return;

        float distance = Vector2.Distance(transform.position, targetPlayer.position);

        if (distance <= magnetRadius)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                targetPlayer.position,
                magnetSpeed * Time.deltaTime
            );
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
        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        if (PlayerProgressManager.Instance == null)
        {
            Debug.LogWarning("MaterialPickup2D: PlayerProgressManager не найден.");
            return;
        }

        PlayerProgressManager.Instance.AddMaterial(materialType, amount);

        Destroy(gameObject);
    }
}