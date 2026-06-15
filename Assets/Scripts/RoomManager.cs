using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager instance;

    [Header("Префабы врагов")]
    public GameObject enemyPrefab;
    public GameObject rangedEnemyPrefab;
    [Range(0f, 1f)] public float rangedEnemyChance = 0.35f;

    [Header("Спавн")]
    public Transform[] spawnPoints;
    public int wavesCount = 1;
    public Vector2Int enemiesPerWave = new Vector2Int(3, 5);

    private readonly List<Enemy> activeEnemies =
    new List<Enemy>();
    private int currentWave = 0;

    public bool AllWavesCompleted { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (enemyPrefab == null && rangedEnemyPrefab == null)
        {
            Debug.LogError("Нужен хотя бы один префаб врага: enemyPrefab или rangedEnemyPrefab.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Spawn Points не назначены.");
            return;
        }

        StartWave();

        if (ItemSpawnManager.Instance != null)
            ItemSpawnManager.Instance.SpawnItems();
    }

    private void StartWave()
    {
        Debug.Log($"Start wave {currentWave + 1}/{wavesCount}");

        activeEnemies.RemoveAll(e => e == null);

        if (currentWave >= wavesCount)
        {
            AllWavesCompleted = true;
            return;
        }

        int count = Random.Range(enemiesPerWave.x, enemiesPerWave.y + 1);

        for (int i = 0; i < count; i++)
        {
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject prefab = ChooseEnemyPrefab();

            if (prefab == null)
                continue;

            GameObject enemyObject = Instantiate(prefab, point.position, Quaternion.identity);

            Enemy enemyComponent = enemyObject.GetComponent<Enemy>();

            if (enemyComponent != null)
            {
                activeEnemies.Add(enemyComponent);

                if (PlayerProgressManager.Instance != null)
                {
                    enemyComponent.ApplyProgressDifficulty(
                        PlayerProgressManager.Instance.GetEnemyHealthMultiplier(),
                        PlayerProgressManager.Instance.GetEnemyDamageMultiplier(),
                        PlayerProgressManager.Instance.GetEnemySpeedMultiplier()
                    );
                }
            }
        }

        currentWave++;

        if (ItemSpawnManager.Instance != null)
            ItemSpawnManager.Instance.SpawnItems();
    }

    private GameObject ChooseEnemyPrefab()
    {
        if (enemyPrefab == null) return rangedEnemyPrefab;
        if (rangedEnemyPrefab == null) return enemyPrefab;

        return Random.value < rangedEnemyChance ? rangedEnemyPrefab : enemyPrefab;
    }

    public void EnemyDied(Enemy enemy)
    {
        if (enemy != null)
            activeEnemies.Remove(enemy);

        activeEnemies.RemoveAll(e => e == null);

        Debug.Log($"Enemy died. Remaining: {activeEnemies.Count}");

        if (activeEnemies.Count > 0)
            return;

        if (currentWave >= wavesCount)
        {
            AllWavesCompleted = true;
            Debug.Log("All waves completed");
        }
        else
        {
            Debug.Log("Starting next wave...");
            Invoke(nameof(StartWave), 2f);
        }
    }
}