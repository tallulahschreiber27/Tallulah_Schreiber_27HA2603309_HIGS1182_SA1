using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [Header("Prefabs & References")]
    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private Transform playerTransform;

    [Header("Spawn Settings")]
    [SerializeField] private Vector3 spawnRange = new Vector3(20f, 0f, 20f);

    // --- UPDATED TIMER VARIABLES ---
    [Tooltip("Time in seconds before the first wave spawns after the game starts.")]
    [SerializeField] private float initialSpawnDelay = 0f; // Set to 0f for instant loading!

    [Tooltip("Time in seconds between subsequent asteroid wave spawns.")]
    [SerializeField] private float waveIntervalRate = 3f;
    // -------------------------------

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
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        // --- NEW INITIALIZATION LOGIC ---
        // Instantly loads the first batch if initialSpawnDelay is 0, 
        // otherwise waits for the specified timer duration before looping.
        InvokeRepeating(nameof(SpawnAsteroidWave), initialSpawnDelay, waveIntervalRate);
        Debug.Log($"Spawner: Initialized with a startup delay of {initialSpawnDelay}s and an interval of {waveIntervalRate}s.");
    }

    private void SpawnAsteroidWave()
    {
        if (asteroidPrefab == null) return;

        // Global Cap check to keep your scene optimized
        int currentAsteroidCount = GameObject.FindGameObjectsWithTag("Asteroid").Length;
        if (currentAsteroidCount >= maxTotalAsteroids)
        {
            Debug.Log("Spawner: Maximum asteroid limit reached on screen. Skipping wave spawn.");
            return;
        }

        int currentWaveAmount = Random.Range(minSpawnAmount, maxSpawnAmount + 1);
        Debug.Log($"Spawner: Instantiating a wave of {currentWaveAmount} asteroids.");

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
                Vector3 potentialPosition = new Vector3(randomX, 0f, randomZ);

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
            else
            {
                Debug.LogWarning($"Spawner: Asteroid #{i + 1} in wave skipped—could not find a safe position.");
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
    }
}
