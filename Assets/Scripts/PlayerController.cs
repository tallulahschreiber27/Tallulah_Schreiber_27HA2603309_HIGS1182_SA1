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

    [Header("Raycast Laser Shooting Settings")]
    [SerializeField] private Transform firePoint;
    [Tooltip("How far the invisible laser ray can travel to hit targets.")]
    [SerializeField] private float attackRange = 150f;
    [Tooltip("Assign your 'Threats' Layer here in the Inspector so lasers pass right through collectibles!")]
    [SerializeField] private LayerMask shootableLayers;

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
        // GAME OVER / WIN PROTECTION: Uses our new safe public property
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
        {
            return;
        }

        // PAUSE PROTECTION: Stops inputs if the game is paused
        if (Mathf.Approximately(Time.timeScale, 0f))
        {
            return;
        }

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

        // --- UPGRADED BLEND TREE LOCOMOTION FEED ---
        if (animController != null)
        {
            // Passes raw horizontal (-1 to 1) and vertical (-1 to 1) inputs straight to the Animator
            animController.UpdateLocomotion(moveX, moveZ);
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
            ExecuteRaycastShoot();
        }
    }

    // --- Airtight Multi-Shot Distribution Core ---
    private void ExecuteRaycastShoot()
    {
        Debug.Log("Player fired a laser bolt.");

        Vector3 rayOrigin = firePoint != null ? firePoint.position : transform.position;
        Vector3 rayDirection = firePoint != null ? firePoint.forward : transform.forward;

        // 1. Core Center Laser (Forces Tag Filter Check)
        PerformSingleRaycastLaser(rayOrigin, rayDirection);

        // 2. Scatter Power-Up Side Lasers (Forces Same Tag Filter Check)
        if (PowerUpManager.Instance != null && PowerUpManager.Instance.isScatterActive)
        {
            Vector3 leftDirection = Quaternion.Euler(0f, -25f, 0f) * rayDirection;
            Vector3 rightDirection = Quaternion.Euler(0f, 25f, 0f) * rayDirection;

            PerformSingleRaycastLaser(rayOrigin, leftDirection);
            PerformSingleRaycastLaser(rayOrigin, rightDirection);
            Debug.Log("[Combat Engine] Raycast scatter split vectors triggered safely.");
        }

        if (animController != null) animController.PlayShootAnimation();
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.laserFire);
    }

    private void PerformSingleRaycastLaser(Vector3 origin, Vector3 direction)
    {
        RaycastHit hit;

        // Uses your original shootableLayers setting
        if (Physics.Raycast(origin, direction, out hit, attackRange, shootableLayers, QueryTriggerInteraction.Collide))
        {
            GameObject hitObject = hit.collider.gameObject;

            // --- THE CRITICAL SHIELD: Wipes out the hit if it encounters your "Collectible" tag ---
            if (hitObject.CompareTag("Collectible") || hitObject.CompareTag("Scrap") || hitObject.name.Contains("canister"))
            {
                Debug.Log("[Combat Safeguard] Laser intercepted. Collectible item unharmed.");
                return; // Drops execution instantly before points can be added!
            }

            Debug.Log($"Raycast Hit Target: {hitObject.name}");

            // Handle hitting an Asteroid (DESTRUCTION ONLY - NO SCORE)
            if (hit.collider.CompareTag("Asteroid"))
            {
                Destroy(hit.collider.gameObject);
                Debug.Log("[Combat] Asteroid successfully shattered via Raycast! (0 points awarded)");
            }
            // Handle hitting an Enemy (DESTRUCTION ONLY - NO SCORE)
            else if (hit.collider.CompareTag("Enemy"))
            {
                Destroy(hit.collider.gameObject);
                Debug.Log("[Combat] Hostile Scavenger Drone neutralized via weapon Raycast! (0 points awarded)");
            }
        }

        Debug.DrawRay(origin, direction * attackRange, Color.cyan, 0.15f);
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

    public void TriggerPlayerDeath()
    {
        // 1. Check for Invincibility Shield Protection
        if (PowerUpManager.Instance != null && PowerUpManager.Instance.isInvincible)
        {
            Debug.Log("[Shield Defense] Damage completely neutralized by the active Invincibility Power-Up!");
            return;
        }

        // 2. Play Audio instantly
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.gameOver);
        }

        // 3. Play Visual Animation cleanly
        if (animController != null)
        {
            animController.PlayDeathAnimation();
        }

        // 4. Safely alert the Game Loop
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }
        else
        {
            Debug.LogWarning("GameManager Instance is missing from the active runtime execution!");
        }

        // 5. THE ANIMATION DELAY FIX:
        // Disable physics immediately so you don't take multiple hits while dying
        Collider playerCollider = GetComponent<Collider>();
        if (playerCollider != null) playerCollider.enabled = false;

        if (rb != null) rb.linearVelocity = Vector3.zero;

        // Give the object 0.4 seconds to let the animator show the explosion/hit frames before vanishing!
        Destroy(gameObject, 0.4f);
    }
}
