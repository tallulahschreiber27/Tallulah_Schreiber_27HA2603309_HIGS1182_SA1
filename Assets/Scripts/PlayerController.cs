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
    [SerializeField] private float attackRange = 150f;
    [Tooltip("Make sure to check both the Asteroid and Enemy layers in your inspector dropdown context!")]
    [SerializeField] private LayerMask shootableLayers;
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
            ExecuteRaycastShoot();
        }
    }

    /// <summary>
    /// Section A / B Modular Shooting Method.
    /// Neutralizes asteroids and enemies on impact without raising the scrap collection score metrics.
    /// </summary>
    private void ExecuteRaycastShoot()
    {
        Debug.Log("Player fired a laser bolt.");
        RaycastHit hit;

        Vector3 rayOrigin = firePoint != null ? firePoint.position : transform.position;
        Vector3 rayDirection = firePoint != null ? firePoint.forward : transform.forward;

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, attackRange, shootableLayers))
        {
            Debug.Log($"Raycast Hit Target: {hit.collider.name}");

            // Handle hitting an Asteroid
            if (hit.collider.CompareTag("Asteroid"))
            {
                Destroy(hit.collider.gameObject);
                Debug.Log("[Combat] Asteroid successfully shattered via Raycast!");
            }
            // Handle hitting an Enemy Drone
            else if (hit.collider.CompareTag("Enemy"))
            {
                Destroy(hit.collider.gameObject);
                Debug.Log("[Combat] Hostile Scavenger Drone neutralized via weapon Raycast!");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Asteroid"))
        {
            Debug.Log("3D Asteroid hit the Player via Trigger trigger zone!");
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
