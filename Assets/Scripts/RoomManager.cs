using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager instance;

    [Header("Prefabs")]
    public GameObject meleeEnemyPrefab;
    public GameObject rangedEnemyPrefab;

    [Header("Spawn")]
    public Transform[] spawnPoints;
    public GameObject door;

    [Header("Waves")]
    [Min(1)] public int minWaves = 1;
    [Min(1)] public int maxWaves = 5;
    public Vector2Int enemiesPerWave = new Vector2Int(3, 5);
    public float delayBeforeNextWave = 1.5f;

    [Header("Enemy Mix")]
    [Range(0f, 1f)] public float rangedEnemyChance = 0.35f;
    [Range(0f, 1f)] public float fireChance = 0.30f;
    [Range(0f, 1f)] public float waterChance = 0.30f;

    private readonly List<GameObject> activeEnemies = new List<GameObject>();
    private int totalWaves;
    private int currentWave;
    private bool roomCleared;
    private bool waitingForNextWave;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (meleeEnemyPrefab == null && rangedEnemyPrefab == null)
        {
            Debug.LogError("RoomManager: нужен хотя бы один префаб врага.");
            enabled = false;
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("RoomManager: spawnPoints не назначены.");
            enabled = false;
            return;
        }

        if (maxWaves < minWaves)
            maxWaves = minWaves;

        totalWaves = Random.Range(minWaves, maxWaves + 1);
        currentWave = 0;
        roomCleared = false;
        waitingForNextWave = false;

        if (door != null)
            door.SetActive(false);

        StartWave();
    }

    private void StartWave()
    {
        if (roomCleared)
            return;

        waitingForNextWave = false;
        activeEnemies.RemoveAll(e => e == null);

        if (currentWave >= totalWaves)
        {
            OpenDoor();
            return;
        }

        int count = Random.Range(enemiesPerWave.x, enemiesPerWave.y + 1);

        for (int i = 0; i < count; i++)
        {
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject prefab = ChooseEnemyPrefab();

            if (prefab == null)
                continue;

            GameObject enemy = Instantiate(prefab, point.position, Quaternion.identity);
            ConfigureSpawnedEnemy(enemy);
            activeEnemies.Add(enemy);
        }

        currentWave++;

        if (activeEnemies.Count == 0)
            ScheduleNextWave();
    }

    private GameObject ChooseEnemyPrefab()
    {
        bool useRanged = rangedEnemyPrefab != null && (meleeEnemyPrefab == null || Random.value < rangedEnemyChance);

        if (useRanged)
            return rangedEnemyPrefab;

        return meleeEnemyPrefab != null ? meleeEnemyPrefab : rangedEnemyPrefab;
    }

    private void ConfigureSpawnedEnemy(GameObject enemyObject)
    {
        WeaponManager.Element element = ChooseElement();

        Enemy enemy = enemyObject.GetComponent<Enemy>();
        if (enemy != null)
            enemy.contactElement = element;

        RangedEnemy2D ranged = enemyObject.GetComponent<RangedEnemy2D>();
        if (ranged != null)
            ranged.projectileElement = element;
    }

    private WeaponManager.Element ChooseElement()
    {
        float roll = Random.value;

        if (roll < fireChance)
            return WeaponManager.Element.Fire;

        if (roll < fireChance + waterChance)
            return WeaponManager.Element.Water;

        return WeaponManager.Element.Physical;
    }

    private void ScheduleNextWave()
    {
        if (waitingForNextWave || roomCleared)
            return;

        if (currentWave >= totalWaves)
        {
            OpenDoor();
            return;
        }

        waitingForNextWave = true;
        Invoke(nameof(StartWave), delayBeforeNextWave);
    }

    public void EnemyDied()
    {
        activeEnemies.RemoveAll(e => e == null);

        if (roomCleared)
            return;

        if (activeEnemies.Count == 0)
            ScheduleNextWave();
    }

    private void OpenDoor()
    {
        if (door != null)
            door.SetActive(true);
    }
}