using System; 
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public static event Action OnFuelCollected;

    [Header("Universal Settings")]
    [Tooltip("Allows you to classify what kind of item this is in the inspector.")]
    [SerializeField] private string collectibleType = "Fuel";

    private bool isCollected = false;

    private void Start()
    {
        Debug.Log($"[Spawn] {collectibleType} canister initialized and anchored safely.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            isCollected = true;

            Debug.Log($"[Collection] Player retrieved a {collectibleType} unit!");

            //Sound plays when a fuel tag is collected
            if (collectibleType == "Fuel")
            {
                OnFuelCollected?.Invoke();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.collectiblePickup);
            }
            else
            {
                Debug.LogWarning("AudioManager Instance is missing from the scene!");
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(1);
            }
            else
            {
                Debug.LogWarning("GameManager Instance is missing from the scene!");
            }

            Destroy(gameObject);
        }
    }

    public string GetCollectibleType()
    {
        return collectibleType;
    }
}
