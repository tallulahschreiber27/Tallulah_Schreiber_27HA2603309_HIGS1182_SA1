using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Lifespan")]
    [Tooltip("How many seconds the laser lives before self-destructing if it misses everything.")]
    [SerializeField] private float lifetime = 4f;

    private void Start()
    {
        // Automatically clean up the laser projectile so it doesn't leak memory
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. Safety Gate: Completely ignore the Player Ship when spawning or passing through
        if (other.CompareTag("Player"))
        {
            return;
        }

        // 2. Handle hitting an Asteroid
        if (other.CompareTag("Asteroid") || LayerMask.LayerToName(other.gameObject.layer) == "Asteroid")
        {
            HandleAsteroidDestruction(other.gameObject);
        }
        // 3. Handle hitting an Enemy
        else if (other.CompareTag("Enemy") || LayerMask.LayerToName(other.gameObject.layer) == "Enemy")
        {
            HandleEnemyDestruction(other.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Solid physics fallback just in case some colliders are not set to 'Is Trigger'
        if (collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        if (collision.gameObject.CompareTag("Asteroid") || LayerMask.LayerToName(collision.gameObject.layer) == "Asteroid")
        {
            HandleAsteroidDestruction(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Enemy") || LayerMask.LayerToName(collision.gameObject.layer) == "Enemy")
        {
            HandleEnemyDestruction(collision.gameObject);
        }
    }

    private void HandleAsteroidDestruction(GameObject asteroid)
    {
        Debug.Log($"[Combat] Laser successfully shattered asteroid: {asteroid.name}");

        // Play asteroid explosion audio cue
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.asteroidDestroyed);
        }

        Destroy(asteroid);
        Destroy(gameObject); // Destroy the laser bolt
    }

    private void HandleEnemyDestruction(GameObject enemy)
    {
        Debug.Log($"[Combat] Laser successfully neutralized enemy drone: {enemy.name}");

        // Play dedicated enemy explosion audio cue
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.enemyDestroyed);
        }

        Destroy(enemy);
        Destroy(gameObject); // Destroy the laser bolt
    }
}
