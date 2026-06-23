using UnityEngine;
using System.Collections;

public class CartridgeSpawnPoint : MonoBehaviour
{
    [SerializeField]
    private GameObject cartridgePrefab;

    [SerializeField]
    private float respawnDelay = 15f;

    private GameObject spawnedObject;

    private void Start()
    {
        Spawn();
    }

    private void Spawn()
    {
        spawnedObject =
            Instantiate(
                cartridgePrefab,
                transform.position,
                Quaternion.identity,
                transform);
    }

    private void Update()
    {
        if (spawnedObject == null)
            StartCoroutine(RespawnRoutine());
    }

    private bool waiting;

    private IEnumerator RespawnRoutine()
    {
        if (waiting)
            yield break;

        waiting = true;

        yield return new WaitForSeconds(respawnDelay);

        Spawn();

        waiting = false;
    }
}