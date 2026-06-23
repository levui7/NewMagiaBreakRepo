using System.Collections.Generic;
using UnityEngine;

public class ItemSpawnManager : MonoBehaviour
{
    public static ItemSpawnManager Instance;

    [Header("Точки появления")]
    public Transform[] spawnPoints;

    [Header("Предметы")]
    public GameObject[] itemPrefabs;

    [Header("Количество предметов")]
    public int baseItemsPerWave = 2;

    [Header("Удалять старые предметы")]
    public bool clearOldItems = true;

    private readonly List<GameObject> spawnedItems =
        new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnItems()
    {
        if (spawnPoints.Length == 0)
            return;

        if (itemPrefabs.Length == 0)
            return;

        if (clearOldItems)
        {
            foreach (GameObject item in spawnedItems)
            {
                if (item != null)
                    Destroy(item);
            }

            spawnedItems.Clear();
        }

        List<int> usedPoints = new List<int>();

        int count =
            Mathf.Min(
                baseItemsPerWave +
                Random.Range(0, 2),
                spawnPoints.Length);

        for (int i = 0; i < count; i++)
        {
            int pointIndex;

            do
            {
                pointIndex = Random.Range(0, spawnPoints.Length);
            }
            while (usedPoints.Contains(pointIndex));

            usedPoints.Add(pointIndex);

            GameObject prefab =
                itemPrefabs[Random.Range(0, itemPrefabs.Length)];

            GameObject item =
                Instantiate(
                    prefab,
                    spawnPoints[pointIndex].position,
                    Quaternion.identity);

            spawnedItems.Add(item);
        }
    }
}