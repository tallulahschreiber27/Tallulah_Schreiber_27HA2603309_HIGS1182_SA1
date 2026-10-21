using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Universal Settings")]
    [Tooltip("Allows you to classify what kind of item this is in the inspector.")]
    [SerializeField] private string collectibleType = "Fuel";

    // Safety Gate: Prevents double-counting from multi-script or multi-frame physics triggers
    private bool isCollected = false;

    private void Start()
    {
        Debug.Log($"[Spawn] {collectibleType} canister initialized and anchored safely.");
    }

    private void OnTriggerEnter(Collider other)
    {
        //Check if the collider is the player AND ensure it hasn't been processed yet
        if (other.CompareTag("Player") && !isCollected)
        {
            isCollected = true;

            Debug.Log($"[Collection] Player retrieved a {collectibleType} unit!");

            //Update the score exactly once (there was a bug where it doubled)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(1);
            }
            else
            {
                Debug.LogWarning("GameManager Instance is missing from the scene!");
            }

            //Remove the canister from the game space
            Destroy(gameObject);
        }
    }

    public string GetCollectibleType()
    {
        return collectibleType;
    }
}
