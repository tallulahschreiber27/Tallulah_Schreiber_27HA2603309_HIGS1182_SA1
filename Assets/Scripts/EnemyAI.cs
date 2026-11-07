using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum AIState { Patrol, Chase }

    [Header("Current State")]
    [SerializeField] private AIState currentState = AIState.Patrol;

    [Header("Movement Configuration")]
    [SerializeField] private float basePatrolSpeed = 4f;
    [SerializeField] private float baseChaseSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Detection Settings")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private float detectionRange = 15f;

    [Header("Rigid Patrol Boundaries")]
    [SerializeField] private Vector2 xPatrolRange = new Vector2(-15f, 15f);

    [Header("Infinite Respawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnZCoordinate = 20f;

    private float activePatrolSpeed;
    private float activeChaseSpeed;
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
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTarget = playerObj.transform;
        }

        if (GameManager.Instance != null && GameManager.Instance.HasDifficultyScaledThisRun)
        {
            IncreaseAISpeed(1.3f);
        }
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
            case AIState.Patrol: ExecutePatrolLogic(); break;
            case AIState.Chase: ExecuteChaseLogic(); break;
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

        if (rb != null) rb.linearVelocity = moveDirection * activePatrolSpeed;

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void ExecuteChaseLogic()
    {
        if (playerTarget == null) return;

        Vector3 moveDirection = (playerTarget.position - transform.position);
        moveDirection.y = 0f;
        moveDirection.Normalize();

        if (rb != null) rb.linearVelocity = moveDirection * activeChaseSpeed;

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TriggerPlayerDeath();
            }
            Destroy(gameObject);
        }
    }

    public void IncreaseAISpeed(float factor)
    {
        activePatrolSpeed = basePatrolSpeed * factor;
        activeChaseSpeed = baseChaseSpeed * factor;
        Debug.Log($"[AI Engine] Speed updated by factor: {factor}");
    }

    private void OnDestroy()
    {
        if (isQuitting || !Application.isPlaying) return;

        if (enemyPrefab != null)
        {
            float randomX = Random.Range(xPatrolRange.x, xPatrolRange.y);
            Vector3 respawnPosition = new Vector3(randomX, transform.position.y, spawnZCoordinate);
            Instantiate(enemyPrefab, respawnPosition, Quaternion.identity);
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
