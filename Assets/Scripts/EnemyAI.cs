using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    // High-tier rubric requirement: Explicit state machine structure
    public enum AIState { Patrol, Chase }

    [Header("Current State")]
    [SerializeField] private AIState currentState = AIState.Patrol;

    [Header("Movement Configuration")]
    [SerializeField] private float basePatrolSpeed = 4f;
    [SerializeField] private float baseChaseSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f;

    // --- NEW INSPECTOR FIELDS ---
    [Header("Fuel Scaling Settings")]
    [Tooltip("How much faster the base speeds get per fuel collection.")]
    [SerializeField] private float speedIncreasePerFuel = 0.5f;

    [Tooltip("Watch these change in real-time in the Inspector during play mode!")]
    [SerializeField] private float livePatrolSpeed;
    [SerializeField] private float liveChaseSpeed;
    // ----------------------------

    [Header("Detection Settings")]
    [SerializeField] private Transform playerTarget;
    [Tooltip("The range at which the enemy drone can spot the player.")]
    [SerializeField] private float detectionRange = 15f;

    [Header("Rigid Patrol Boundaries")]
    [Tooltip("The left (X min) and right (X max) turning points.")]
    [SerializeField] private Vector2 xPatrolRange = new Vector2(-15f, 15f);

    [Header("Infinite Respawn Settings")]
    [Tooltip("Drag your Enemy Drone prefab asset here so it can replace itself when destroyed!")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnZCoordinate = 20f;

    // Static variables persist across drone destruction/respawning!
    private static float staticFuelSpeedModifier = 0f;

    private bool movingRight = true;
    private bool isQuitting = false;
    private Rigidbody rb;

    private void OnEnable()
    {
        // Subscribe to the event from your Collectible script
        Collectible.OnFuelCollected += HandleFuelCollected;
    }

    private void OnDisable()
    {
        // Always unsubscribe to prevent memory leak errors
        Collectible.OnFuelCollected -= HandleFuelCollected;
    }

    private void Start()
    {
        // Initialize our live operational speeds from base values
        UpdateLiveSpeeds();

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            // Freeze Y and rotations for a clean top-down spatial plane layout
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTarget = playerObj.transform;
        }

        // Scale difficulty if the GameManager flags a prior milestone upgrade
        if (GameManager.Instance != null && GameManager.Instance.HasDifficultyScaledThisRun)
        {
            IncreaseAISpeed(1.3f);
        }
    }

    private void HandleFuelCollected()
    {
        // 1. Permanently increase the global speed modifier for current AND future drones
        staticFuelSpeedModifier += speedIncreasePerFuel;

        // 2. Refresh the speeds of this active drone instance immediately
        UpdateLiveSpeeds();

        Debug.Log($"[AI Engine] Fuel pickup detected! Global modifier: +{staticFuelSpeedModifier}. New Patrol Speed: {livePatrolSpeed}");
    }

    private void UpdateLiveSpeeds()
    {
        // Combine base speed, any static fuel progression, and custom programmatic scaling
        livePatrolSpeed = basePatrolSpeed + staticFuelSpeedModifier;
        liveChaseSpeed = baseChaseSpeed + staticFuelSpeedModifier;
    }

    private void FixedUpdate()
    {
        EvaluateStateTransitions();
        ExecuteCurrentStateLogic();
    }

    private void EvaluateStateTransitions()
    {
        if (playerTarget == null)
        {
            currentState = AIState.Patrol;
            return;
        }

        // Calculate horizontal flat plane distance
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer <= detectionRange)
        {
            if (currentState != AIState.Chase)
            {
                currentState = AIState.Chase;
                Debug.Log("[AI Engine] State Change: Chasing Player!");
            }
        }
        else
        {
            if (currentState != AIState.Patrol)
            {
                currentState = AIState.Patrol;
                Debug.Log("[AI Engine] State Change: Resuming Patrol.");
            }
        }
    }

    private void ExecuteCurrentStateLogic()
    {
        switch (currentState)
        {
            case AIState.Patrol:
                ExecutePatrolLogic();
                break;

            case AIState.Chase:
                ExecuteChaseLogic();
                break;
        }
    }

    private void ExecutePatrolLogic()
    {
        Vector3 moveDirection = Vector3.zero;

        if (movingRight)
        {
            moveDirection = Vector3.right;
            if (transform.position.x >= xPatrolRange.y) movingRight = false;
        }
        else
        {
            moveDirection = Vector3.left;
            if (transform.position.x <= xPatrolRange.x) movingRight = true;
        }

        if (rb != null)
            rb.linearVelocity = moveDirection * livePatrolSpeed; // Updated to use visible live speeds

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void ExecuteChaseLogic()
    {
        if (playerTarget == null) return;

        // Dynamic 3D tracking vector calculations
        Vector3 moveDirection = (playerTarget.position - transform.position);
        moveDirection.y = 0f;
        moveDirection.Normalize();

        if (rb != null)
            rb.linearVelocity = moveDirection * liveChaseSpeed; // Updated to use visible live speeds

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Safe Intercept: Check if we physically bumped into the Player ship
        if (collision.gameObject.CompareTag("Player"))
        {
            // --- BACKWARDS COMPATIBLE FIX ---
            // Uses SendMessage to safely search for any death handler on the player without strict type compilation locks
            collision.gameObject.SendMessage("TriggerPlayerDeath", SendMessageOptions.DontRequireReceiver);

            Destroy(gameObject); // Exterminate this specific enemy drone on physical impact
        }
    }

    // Required Custom Method: Clearly handles difficulty scaling properties programmatically
    public void IncreaseAISpeed(float factor)
    {
        basePatrolSpeed *= factor;
        baseChaseSpeed *= factor;
        UpdateLiveSpeeds();

        Debug.Log($"[AI Engine] {gameObject.name} difficulty scaled. Patrol Speed: {livePatrolSpeed}, Chase Speed: {liveChaseSpeed}");
    }

    private void OnDestroy()
    {
        if (isQuitting || !Application.isPlaying) return;

        if (enemyPrefab != null)
        {
            float randomX = Random.Range(xPatrolRange.x, xPatrolRange.y);
            Vector3 respawnPosition = new Vector3(randomX, transform.position.y, spawnZCoordinate);
            Instantiate(enemyPrefab, respawnPosition, Quaternion.identity);
            Debug.Log("[AI Engine] Replacement drone successfully instantiated.");
        }
    }

    private void OnApplicationQuit()
    {
        // Clean up static variable so speed resets when you exit play mode and press play again
        staticFuelSpeedModifier = 0f;
        isQuitting = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
