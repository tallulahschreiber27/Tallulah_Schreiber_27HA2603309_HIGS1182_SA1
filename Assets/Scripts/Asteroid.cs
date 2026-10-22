using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Base speed at which the asteroid drifts down the screen.")]
    [SerializeField] private float driftSpeed = 5f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;

            float randomSpeed = Random.Range(driftSpeed * 0.7f, driftSpeed * 1.3f);
            float randomHorizontalDrift = Random.Range(-1.5f, 1.5f);

            Vector3 uniqueDirection = new Vector3(randomHorizontalDrift, 0f, -1f).normalized;
            rb.linearVelocity = uniqueDirection * randomSpeed;

            float randomSpin = Random.Range(-50f, 50f);
            rb.angularVelocity = new Vector3(0f, randomSpin, 0f);
        }
        else
        {
            Debug.LogError("Asteroid Error: Rigidbody component is missing!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. Safety Gate: Explicitly ignore lasers and projectiles
        if (other.CompareTag("Projectile") || LayerMask.LayerToName(other.gameObject.layer) == "Projectile" || other.GetComponent<Projectile>() != null)
        {
            return;
        }

        // 2. Check if the object that entered the trigger is genuinely the Player
        if (other.CompareTag("Player") && LayerMask.LayerToName(other.gameObject.layer) == "Player")
        {
            // Extract the player component to let the ship verify its own invincibility status [INDEX: 0.1.4]
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // Send collision events up to the Player Controller logic loops safely
                player.SendMessage("TriggerPlayerDeath", SendMessageOptions.DontRequireReceiver);

                // Intercept check: If the player shield blocks death, do not destroy this asteroid either! [INDEX: 0.1.3]
                if (PowerUpManager.Instance != null && PowerUpManager.Instance.isInvincible)
                {
                    Debug.Log("[Combat] Asteroid harmlessly deflected off the player's invincibility shields.");
                    return;
                }
            }

            // Fallback terminal execution paths if the player does not have an active invincibility shield
            Debug.Log("[Combat] Asteroid physically crushed the Player!");

            // Play game over audio using your working architecture clip references
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.gameOver);
            }
            else
            {
                Debug.LogWarning("AudioManager Instance is missing from the scene!");
            }

            // Call the game over screen via your custom GameManager script framework
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
            else
            {
                Debug.LogWarning("GameManager Instance is missing from the scene!");
            }

            // Destroy elements safely only when shields are inactive
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Clean up when the asteroid safely exits the large screen boundary box
        if (other.CompareTag("GameplayBoundary"))
        {
            Debug.Log("[Cleanup] Asteroid left the arena and cleanly self-destructed.");
            Destroy(gameObject);
        }
    }
}
