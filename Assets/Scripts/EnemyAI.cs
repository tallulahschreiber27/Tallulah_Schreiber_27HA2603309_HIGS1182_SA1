using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement Configuration")]
    [SerializeField] private float basePatrolSpeed = 4f;
    [SerializeField] private float baseChaseSpeed = 8f;

    [Header("Detection Settings")]
    [SerializeField] private Transform playerTarget;
    [Tooltip("The range at which the enemy drone can spot your drone.")]
    [SerializeField] private float detectionRange = 25f;

    [Header("Rigid Patrol Boundaries")]
    [Tooltip("The left (X min) and right (X max) turning points.")]
    [SerializeField] private Vector2 xPatrolRange = new Vector2(-198f, 173f);

    [Header("Infinite Respawn Settings")]
    [Tooltip("Drag your Enemy Drone prefab asset here so it can replace itself when destroyed!")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnZCoordinate = 150f;

    private float activePatrolSpeed;
    private float activeChaseSpeed;

    private bool isChasingPlayer = false;
    private bool movingRight = true;
    private bool isQuitting = false;
    private Rigidbody rb;

    private void Start()
    {
        activePatrolSpeed = basePatrolSpeed;
        activeChaseSpeed = baseChaseSpeed;

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }

        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTarget = playerObj.transform;
        }

        // If this clone spawns into the world while difficulty is already high, buff speed instantly
        if (GameManager.Instance != null && GameManager.Instance.HasDifficultyScaledThisRun)
        {
            IncreaseAISpeed(1.3f);
        }
    }

    private void FixedUpdate()
    {
        HandleAIBehaviourStates();
    }

    private void HandleAIBehaviourStates()
    {
        if (playerTarget == null)
        {
            RigidPatrolLoop();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer <= detectionRange)
        {
            if (!isChasingPlayer)
            {
                isChasingPlayer = true;
                Debug.Log("[AI Engine] Player spotted! Charging straight down.");
            }
            RigidDownwardsChaseLoop();
        }
        else
        {
            if (isChasingPlayer)
            {
                isChasingPlayer = false;
                Debug.Log("[AI Engine] Threat lost. Resuming horizontal border pacing.");
            }
            RigidPatrolLoop();
        }
    }

    private void RigidPatrolLoop()
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

        if (rb != null) rb.linearVelocity = moveDirection * activePatrolSpeed;

        if (moveDirection != Vector3.zero) transform.forward = moveDirection;
    }

    private void RigidDownwardsChaseLoop()
    {
        float xDirection = 0f;

        if (playerTarget.position.x > transform.position.x + 1f) xDirection = 1f;
        else if (playerTarget.position.x < transform.position.x - 1f) xDirection = -1f;

        Vector3 moveDirection = new Vector3(xDirection, 0f, -1f).normalized;

        if (rb != null) rb.linearVelocity = moveDirection * activeChaseSpeed;

        // Force orientation straight down the grid map (no weird rotation tilting)
        transform.forward = Vector3.back;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (GameManager.Instance != null) GameManager.Instance.TriggerGameOver();
        }
    }

    public void IncreaseAISpeed(float factor)
    {
        activePatrolSpeed = basePatrolSpeed * factor;
        activeChaseSpeed = baseChaseSpeed * factor;

        if (rb != null)
        {
            rb.linearVelocity = rb.linearVelocity * factor;
        }

        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }

        Debug.Log($"[AI Engine] {gameObject.name} speed boosted! Patrol: {activePatrolSpeed}, Chase: {activeChaseSpeed}");
    }

    private void OnDestroy()
    {
        if (isQuitting || !Application.isPlaying) return;

        if (enemyPrefab != null)
        {
            float randomX = Random.Range(xPatrolRange.x, xPatrolRange.y);
            Vector3 respawnPosition = new Vector3(randomX, transform.position.y, spawnZCoordinate);
            Instantiate(enemyPrefab, respawnPosition, Quaternion.identity);
            Debug.Log("[AI Engine] Enemy destroyed. Replacement drone spawned.");
        }
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
