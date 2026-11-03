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

    // Reference cache for your dedicated animation script
    private AnimationController animController;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        animController = GetComponent<AnimationController>(); // Efficient component access

        if (rb == null)
        {
            Debug.LogError("PlayerController: Missing required Rigidbody component!");
        }

        if (animController == null)
        {
            Debug.LogWarning("PlayerController: Missing required AnimationController component on this GameObject!");
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

        // Power-Up Speed Check: Accelerate speed if the thruster boost is running
        float currentSpeed = moveSpeed;
        if (PowerUpManager.Instance != null && PowerUpManager.Instance.isSpeedBoosted)
        {
            currentSpeed = moveSpeed * 1.6f; // Boosts baseline speed by 60%
        }

        Vector3 movement = new Vector3(moveX, 0f, moveZ).normalized * currentSpeed;
        rb.linearVelocity = movement;

        // --- ANIMATION INTERACTION LOOP ---
        if (animController != null)
        {
            // Tracks if there is any movement engine thrust being supplied
            bool isMoving = (moveX != 0 || moveZ != 0);
            animController.SetMovementAnimation(isMoving);

            // Directly maps your A/D values (-1 to 1) onto your Drone_Left and Drone_Right animations
            animController.SetHorizontalAnimation(moveX);
        }

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

    private void ExecutePrefabShoot()
    {
        if (laserPrefab == null)
        {
            Debug.LogWarning("PlayerController: Laser Prefab is missing in the Inspector!");
            return;
        }

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        Quaternion spawnRotation = firePoint != null ? firePoint.rotation : transform.rotation;

        // 1. Fire Main Center Laser Projectile
        SpawnIndividualLaser(spawnPosition, spawnRotation);

        // Power-Up Scatter Shot Check: Fires 2 additional angled lasers
        if (PowerUpManager.Instance != null && PowerUpManager.Instance.isScatterActive)
        {
            Quaternion leftRotation = spawnRotation * Quaternion.Euler(0f, -25f, 0f);
            Quaternion rightRotation = spawnRotation * Quaternion.Euler(0f, 25f, 0f);

            SpawnIndividualLaser(spawnPosition, leftRotation);
            SpawnIndividualLaser(spawnPosition, rightRotation);
            Debug.Log("[Combat Engine] Scatter-Shot spread matrix unleashed.");
        }

        Debug.Log("Player spawned laser configuration.");

        // Trigger the custom weapon animation clip via the controller
        if (animController != null)
        {
            animController.PlayShootAnimation();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.laserFire);
        }
    }

    private void SpawnIndividualLaser(Vector3 position, Quaternion rotation)
    {
        GameObject projectile = Instantiate(laserPrefab, position, rotation);
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

        if (projectileRb != null)
        {
            projectileRb.linearVelocity = rotation * Vector3.forward * laserSpeed;
        }
        else
        {
            Debug.LogWarning("Projectile lacks a Rigidbody component! Forcing straight translation.");
            Destroy(projectile, 5f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Asteroid") || other.CompareTag("Enemy"))
        {
            Debug.Log($"[Collision Trigger] Ship impacted threat: {other.gameObject.name}");
            TriggerPlayerDeath();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Asteroid") || collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log($"[Physical Impact] Ship crashed into: {collision.gameObject.name}");
            TriggerPlayerDeath();
        }
    }

    private void TriggerPlayerDeath()
    {
        // Power-Up Invincibility Check: Intercepts destruction vector if shield is running
        if (PowerUpManager.Instance != null && PowerUpManager.Instance.isInvincible)
        {
            Debug.Log("[Shield Defense] Damage completely neutralized by the active Invincibility Power-Up!");
            return;
        }

        // Trigger the visual impact or explosion animation before destroying
        if (animController != null)
        {
            animController.PlayHitAnimation();
        }

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
