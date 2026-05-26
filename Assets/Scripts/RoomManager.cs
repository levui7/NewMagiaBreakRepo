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
    public GameObject door;
    public int wavesCount = 1;
    public Vector2Int enemiesPerWave = new Vector2Int(3, 5);

    private readonly List<GameObject> activeEnemies = new List<GameObject>();
    private int currentWave = 0;

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

        if (door != null)
            door.SetActive(true);

        StartWave();
    }

    private void StartWave()
    {
        activeEnemies.RemoveAll(e => e == null);

        if (currentWave >= wavesCount)
        {
            OpenDoor();
            return;
        }

        int count = Random.Range(enemiesPerWave.x, enemiesPerWave.y + 1);
        for (int i = 0; i < count; i++)
        {
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject prefab = ChooseEnemyPrefab();

            if (prefab == null) continue;

            GameObject enemy = Instantiate(prefab, point.position, Quaternion.identity);
            activeEnemies.Add(enemy);
        }

        currentWave++;
    }

    private GameObject ChooseEnemyPrefab()
    {
        if (enemyPrefab == null) return rangedEnemyPrefab;
        if (rangedEnemyPrefab == null) return enemyPrefab;

        return Random.value < rangedEnemyChance ? rangedEnemyPrefab : enemyPrefab;
    }

    public void EnemyDied()
    {
        activeEnemies.RemoveAll(e => e == null);

        if (activeEnemies.Count == 0)
            Invoke(nameof(StartWave), 2f);
    }

    private void OpenDoor()
    {
        if (door != null)
            door.SetActive(false);
    }
}
