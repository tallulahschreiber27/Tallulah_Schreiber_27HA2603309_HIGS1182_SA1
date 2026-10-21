using System.Collections.Generic;
using UnityEngine;

public class AsteroidsSpawner : MonoBehaviour
{
    [Header("Prefab Assignment")]
    [SerializeField] private GameObject asteroidPrefab;

    [Header("Spawning Count")]
    [Tooltip("The exact number of asteroids that should always be present in the game.")]
    [SerializeField] private int maxAsteroidsCount = 15;

    [Header("Spawning Boundaries")]
    [Tooltip("The minimum and maximum X coordinates taken from your inspector bounds.")]
    [SerializeField] private Vector2 xSpawnRange = new Vector2(-198f, 173f);
    [Tooltip("The minimum and maximum Z coordinates taken from your inspector bounds.")]
    [SerializeField] private Vector2 zSpawnRange = new Vector2(-157f, 168f);

    [Header("Player Safety Buffer")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float safeDistanceZone = 10f;

    [Header("Scatter Settings")]
    [SerializeField] private Vector2 speedRange = new Vector2(5f, 15f);
    [SerializeField] private float maxSidewaysScatter = 5f;

    private List<GameObject> activeAsteroids = new List<GameObject>();

    // Debug counter tracking
    private int totalAsteroidsSpawnedCount = 0;

    private void Start()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        MaintainAsteroidPopulation();
    }

    private void Update()
    {
        MonitorActiveAsteroids();
        MaintainAsteroidPopulation();
    }

    private void MonitorActiveAsteroids()
    {
        for (int i = activeAsteroids.Count - 1; i >= 0; i--)
        {
            if (activeAsteroids[i] == null)
            {
                activeAsteroids.RemoveAt(i);
            }
        }
    }

    private void MaintainAsteroidPopulation()
    {
        while (activeAsteroids.Count < maxAsteroidsCount)
        {
            SpawnSingleAsteroid();
        }
    }

    public void SpawnSingleAsteroid()
    {
        Vector3 spawnPosition = Vector3.zero;
        bool validPositionFound = false;
        int attempts = 0;
        int maxAttempts = 20;

        while (!validPositionFound && attempts < maxAttempts)
        {
            float randomX = Random.Range(xSpawnRange.x, xSpawnRange.y);
            float randomZ = Random.Range(zSpawnRange.x, zSpawnRange.y);
            spawnPosition = new Vector3(randomX, 0f, randomZ);

            if (playerTransform != null)
            {
                float distanceToPlayer = Vector3.Distance(spawnPosition, playerTransform.position);
                if (distanceToPlayer >= safeDistanceZone)
                {
                    validPositionFound = true;
                }
            }
            else
            {
                validPositionFound = true;
            }

            attempts++;
        }

        if (validPositionFound)
        {
            GameObject spawnedAsteroid = Instantiate(asteroidPrefab, spawnPosition, Quaternion.identity);
            activeAsteroids.Add(spawnedAsteroid);

            //Informs you how many asteroids are currently in the scene
            totalAsteroidsSpawnedCount++;
            Debug.Log($"[Spawner Metrics] Asteroid spawned successfully. Current Active: {activeAsteroids.Count} | Total Session Spawns: {totalAsteroidsSpawnedCount}");

            // Accessing Rigidbody component at runtime using GetComponent<> (Section B requirement)
            Rigidbody asteroidRb = spawnedAsteroid.GetComponent<Rigidbody>();
            if (asteroidRb != null)
            {
                float randomSpeed = Random.Range(speedRange.x, speedRange.y);
                float randomHorizontalScatter = Random.Range(-maxSidewaysScatter, maxSidewaysScatter);

                float directionModifier = Random.value > 0.5f ? 1f : -1f;
                asteroidRb.linearVelocity = new Vector3(randomHorizontalScatter, 0f, randomSpeed * directionModifier);
            }
        }
        else
        {
            Debug.LogWarning("AsteroidsSpawner: Prevented an asteroid from spawning too close to the player drone!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(playerTransform.position, safeDistanceZone);
        }
    }
}
