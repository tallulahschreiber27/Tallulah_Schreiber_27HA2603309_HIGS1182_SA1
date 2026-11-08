using UnityEngine;

public class PowerUpPickup : MonoBehaviour
{
    [Header("Power-Up Settings")]
    [Tooltip("Select which specific buff token this object instance grants upon contact.")]
    [SerializeField] private PowerUpManager.PowerUpType powerUpType;

    [Header("Visual Effects")]
    [Tooltip("How fast the item spins around in space.")]
    [SerializeField] private float rotationSpeed = 50f;

    private void Update()
    {
        // Allows item to rotate
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only player can collect and laser ignores it
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[Inventory] Player collected power-up token: {powerUpType}");

            if (PowerUpManager.Instance != null)
            {
                PowerUpManager.Instance.HandlePowerUp(powerUpType);
            }
            else
            {
                Debug.LogWarning("PowerUpPickup: PowerUpManager Instance is missing from the active frame hierarchy!");
            }

            // Powerup sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.powerUpPickup);
            }

            Destroy(gameObject);
        }
    }
}
