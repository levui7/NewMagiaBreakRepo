using UnityEngine;

public class RoomRewardSpawner2D : MonoBehaviour
{
    [Header("Pool")]
    public GameObject[] possiblePickups;

    [Header("Spawn")]
    [Min(0)] public int minPickups = 2;
    [Min(0)] public int maxPickups = 5;

    [Header("Optional")]
    public Transform[] spawnPoints;
    public bool spawnOnStart = true;
    public bool spawnOnce = true;

    private bool spawned;

    private void Start()
    {
        if (spawnOnStart)
            SpawnRewards();
    }

    public void SpawnRewards()
    {
        if (spawnOnce && spawned)
            return;

        if (possiblePickups == null || possiblePickups.Length == 0)
            return;

        int count = Random.Range(minPickups, maxPickups + 1);
        spawned = true;

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = possiblePickups[Random.Range(0, possiblePickups.Length)];
            if (prefab == null)
                continue;

            Vector3 position = transform.position;

            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
                if (point != null)
                    position = point.position;
            }
            else
            {
                position += (Vector3)Random.insideUnitCircle * 1.5f;
            }

            Instantiate(prefab, position, Quaternion.identity);
        }
    }
}