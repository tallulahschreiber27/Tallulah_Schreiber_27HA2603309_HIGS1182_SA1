using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [Header("Prefabs & References")]
    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private Transform playerTransform;

    [Header("Spawn Settings")]
    [SerializeField] private Vector3 spawnRange = new Vector3(20f, 0f, 20f);

    [Header("Spawn Timers")]
    [Tooltip("Time in seconds before the first wave spawns. Set to 0f for instant loading!")]
    [SerializeField] private float initialSpawnDelay = 0f;
    [Tooltip("Time in seconds between subsequent wave spawns.")]
    [SerializeField] private float waveIntervalRate = 3f;

    [Header("Wave Amounts")]
    [SerializeField] private int minSpawnAmount = 2;
    [SerializeField] private int maxSpawnAmount = 5;
    [SerializeField] private int maxTotalAsteroids = 20;

    [Header("Safety Buffer Settings")]
    [SerializeField] private float safetyRadius = 5f;
    [SerializeField] private int maxSpawnAttempts = 10;

    private void Start()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        InvokeRepeating(nameof(SpawnAsteroidWave), initialSpawnDelay, waveIntervalRate);
    }

    private void SpawnAsteroidWave()
    {
        if (asteroidPrefab == null) return;

        int currentAsteroidCount = GameObject.FindGameObjectsWithTag("Asteroid").Length;
        if (currentAsteroidCount >= maxTotalAsteroids) return;

        int currentWaveAmount = Random.Range(minSpawnAmount, maxSpawnAmount + 1);

        for (int i = 0; i < currentWaveAmount; i++)
        {
            Vector3 finalSpawnPosition = Vector3.zero;
            bool validPositionFound = false;
            int attempts = 0;

            while (!validPositionFound && attempts < maxSpawnAttempts)
            {
                attempts++;

                float randomX = Random.Range(-spawnRange.x, spawnRange.x);
                float randomZ = Random.Range(-spawnRange.z, spawnRange.z);
                Vector3 potentialPosition = new Vector3(
                    transform.position.x + randomX,
                    transform.position.y,
                    transform.position.z + randomZ
                );

                if (playerTransform == null)
                {
                    finalSpawnPosition = potentialPosition;
                    validPositionFound = true;
                    break;
                }

                float distanceToPlayer = Vector3.Distance(potentialPosition, playerTransform.position);
                if (distanceToPlayer >= safetyRadius)
                {
                    finalSpawnPosition = potentialPosition;
                    validPositionFound = true;
                }
            }

            if (validPositionFound)
            {
                Instantiate(asteroidPrefab, finalSpawnPosition, Quaternion.identity);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerTransform.position, safetyRadius);
        }

        Gizmos.color = Color.red;
        Vector3 size = new Vector3(spawnRange.x * 2f, 1f, spawnRange.z * 2f);
        Gizmos.DrawWireCube(transform.position, size);
    }
}
