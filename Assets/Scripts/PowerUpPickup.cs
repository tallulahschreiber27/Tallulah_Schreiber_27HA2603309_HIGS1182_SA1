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
        // Give the floating item a nice aesthetic rotational roll in space
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Filter collision check: Only let the Player pick it up (Lasers pass straight through)
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[Inventory] Player collected power-up token: {powerUpType}");

            // 1. Pass the enum data directly down to your PowerUpManager component [INDEX: 0.1.3]
            if (PowerUpManager.Instance != null)
            {
                PowerUpManager.Instance.HandlePowerUp(powerUpType);
            }
            else
            {
                Debug.LogWarning("PowerUpPickup: PowerUpManager Instance is missing from the active frame hierarchy!");
            }

            // 2. Play the new dedicated power-up sound clip via the AudioManager [INDEX: 0.1.4]
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.powerUpPickup);
            }

            // 3. Cleanly wipe the item from space to avoid double triggering collisions
            Destroy(gameObject);
        }
    }
}
