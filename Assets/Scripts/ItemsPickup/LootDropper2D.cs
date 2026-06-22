using UnityEngine;

public class LootDropper2D : MonoBehaviour
{
    [Header("Drop Prefabs")]
    public GameObject coinPrefab;
    public GameObject crystalPrefab;

    [Header("Coins")]
    public bool dropCoins = true;
    public int minCoins = 1;
    public int maxCoins = 4;

    [Header("Crystals")]
    [Range(0f, 1f)]
    public float crystalDropChance = 0.15f;
    public int minCrystals = 1;
    public int maxCrystals = 1;

    [Header("Spawn")]
    public float scatterRadius = 0.6f;

    private bool alreadyDropped;

    public void DropLoot()
    {
        if (alreadyDropped)
            return;

        alreadyDropped = true;

        if (dropCoins && coinPrefab != null)
        {
            int coinCount = Random.Range(minCoins, maxCoins + 1);

            for (int i = 0; i < coinCount; i++)
                SpawnDrop(coinPrefab);
        }

        if (crystalPrefab != null && Random.value <= crystalDropChance)
        {
            int crystalCount = Random.Range(minCrystals, maxCrystals + 1);

            for (int i = 0; i < crystalCount; i++)
                SpawnDrop(crystalPrefab);
        }
    }

    private void SpawnDrop(GameObject prefab)
    {
        Vector2 randomOffset = Random.insideUnitCircle * scatterRadius;
        Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

        Instantiate(prefab, spawnPosition, Quaternion.identity);
    }
}