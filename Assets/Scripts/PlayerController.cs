using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 35f;

    [SerializeField] private Vector2 xBounds = new Vector2(-198f, 173f);
    [SerializeField] private Vector2 zBounds = new Vector2(-157f, 168f);

    [Header("Aiming Settings")]
    [Tooltip("Controls how fast the ship rotates towards the mouse. Lower values make it smoother.")]
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Aiming Restrictions")]
    [Tooltip("Maximum degrees the drone can pivot left or right from straight forward.")]
    [SerializeField] private float maxAimAngle = 60f;

    [Header("Center Return Settings")]
    [Tooltip("Distance threshold from the drone to your cursor. If the mouse is closer than this, the drone snaps straight forward.")]
    [SerializeField] private float centerDeadzone = 2.0f;

    [Header("Shooting Settings")]
    [Tooltip("Drag your laser/projectile prefab here!")]
    [SerializeField] private GameObject laserPrefab;
    [Tooltip("Speed of the spawned projectile.")]
    [SerializeField] private float laserSpeed = 80f;
    [SerializeField] private Transform firePoint;

    private Rigidbody rb;
    private Camera mainCamera;
    private Vector3 lastMousePosition;
    private Quaternion targetRotation;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;

        if (rb == null)
        {
            Debug.LogError("PlayerController: Missing required Rigidbody component!");
        }

        lastMousePosition = Input.mousePosition;
        targetRotation = transform.rotation;
    }

    private void Update()
    {
        HandleMovementInput();
        HandleMouseAiming();
        HandleShootingInput();
    }

    private void HandleMovementInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(moveX, 0f, moveZ).normalized * moveSpeed;
        rb.linearVelocity = movement;

        float clampedX = Mathf.Clamp(transform.position.x, xBounds.x, xBounds.y);
        float clampedZ = Mathf.Clamp(transform.position.z, zBounds.x, zBounds.y);
        transform.position = new Vector3(clampedX, 0f, clampedZ);
    }

    private void HandleMouseAiming()
    {
        if (Input.mousePosition != lastMousePosition)
        {
            lastMousePosition = Input.mousePosition;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float rayDistance;

            if (groundPlane.Raycast(ray, out rayDistance))
            {
                Vector3 lookPoint = ray.GetPoint(rayDistance);
                Vector3 targetDirection = new Vector3(lookPoint.x, 0f, lookPoint.z) - transform.position;

                if (targetDirection.magnitude < centerDeadzone)
                {
                    targetDirection = Vector3.forward;
                }

                if (targetDirection != Vector3.zero)
                {
                    float angle = Vector3.SignedAngle(Vector3.forward, targetDirection, Vector3.up);
                    angle = Mathf.Clamp(angle, -maxAimAngle, maxAimAngle);

                    targetRotation = Quaternion.Euler(0f, angle, 0f);
                }
            }
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
    }

    private void HandleShootingInput()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            ExecutePrefabShoot();
        }
    }

    /// <summary>
    /// Spawns a projectile prefab at the firePoint, pushes it forward, and plays the audio cue.
    /// </summary>
    private void ExecutePrefabShoot()
    {
        if (laserPrefab == null)
        {
            Debug.LogWarning("PlayerController: Laser Prefab is missing in the Inspector!");
            return;
        }

        // Determine launch orientation rules
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        Quaternion spawnRotation = firePoint != null ? firePoint.rotation : transform.rotation;

        // 1. Instantiate the laser projectile
        GameObject projectile = Instantiate(laserPrefab, spawnPosition, spawnRotation);

        // 2. Add forward linear velocity to the projectile's Rigidbody
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        if (projectileRb != null)
        {
            projectileRb.linearVelocity = spawnRotation * Vector3.forward * laserSpeed;
        }
        else
        {
            // Fallback movement modifier if the projectile lacks a Rigidbody component
            Debug.LogWarning("Projectile lacks a Rigidbody component! Forcing straight translation.");
            Destroy(projectile, 5f); // Auto-cleanup fallback
        }

        Debug.Log("Player spawned a laser projectile.");

        // ==========================================
        // PLAY LASER FIRE AUDIO HERE
        // ==========================================
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.laserFire);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Asteroid"))
        {
            Debug.Log("3D Asteroid hit the Player via Trigger zone!");
            TriggerPlayerDeath();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Asteroid"))
        {
            Debug.Log("The drone smashed into an Asteroid. Game Over!");
            TriggerPlayerDeath();
        }
    }

    private void TriggerPlayerDeath()
    {
        // ==========================================
        // PLAY GAME OVER AUDIO HERE
        // ==========================================
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.gameOver);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }
        else
        {
            Debug.LogWarning("GameManager Instance is missing from the active runtime execution!");
        }

        Destroy(gameObject);
    }
}
