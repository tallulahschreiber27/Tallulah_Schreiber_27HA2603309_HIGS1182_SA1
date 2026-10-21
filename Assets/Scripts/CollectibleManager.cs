using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    [Header("Prefab Assignment")]
    [SerializeField] private GameObject collectiblePrefab;

    [Header("Finite Spawning Settings")]
    [Tooltip("Change this number to choose exactly how many spawn at start!")]
    [SerializeField] private int totalFuelCanistersToSpawn = 10;

    [Header("Spawning Boundaries")]
    [SerializeField] private Vector2 xSpawnRange = new Vector2(-198f, 173f);
    [SerializeField] private Vector2 zSpawnRange = new Vector2(-157f, 168f);

    private void Start()
    {
        // Loops a fixed amount of times to load your exact target number
        for (int i = 0; i < totalFuelCanistersToSpawn; i++)
        {
            SpawnCollectible();
        }

        Debug.Log($"CollectibleManager: Finite setup complete. Spawned {totalFuelCanistersToSpawn} canisters.");
        GameManager.Instance.SetTargetWinScore(totalFuelCanistersToSpawn);

    }

    private void SpawnCollectible()
    {
        float randomX = Random.Range(xSpawnRange.x, xSpawnRange.y);
        float randomZ = Random.Range(zSpawnRange.x, zSpawnRange.y);
        Vector3 spawnPosition = new Vector3(randomX, 0.5f, randomZ);

        Instantiate(collectiblePrefab, spawnPosition, Quaternion.identity);
    }
}
