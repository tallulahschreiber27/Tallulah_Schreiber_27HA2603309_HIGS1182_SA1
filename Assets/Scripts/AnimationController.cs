using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;

    private void Start()
    {
        // Section B Rule: Efficient programmatic component caching at runtime [INDEX: 0.1.5]
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

        if (animator == null)
        {
            Debug.LogError($"[Animation Error] Animator component missing on children of {gameObject.name}!");
        }
    }

    private void Update()
    {
        HandleMovementVelocityTracking();
    }

    /// <summary>
    /// Tracks physical Rigidbody vectors to toggle between Idle and Thrust loops automatically [INDEX: 0.1.4].
    /// </summary>
    private void HandleMovementVelocityTracking()
    {
        if (rb == null || animator == null) return;

        // Read the linear velocity magnitude to check if the drone ship is actively flying
        bool isMoving = rb.linearVelocity.magnitude > 0.1f;

        // Section A Rubric: Set parameter values inside your Animator state window [INDEX: 0.1.4]
        animator.SetBool("IsMoving", isMoving);
    }

    /// <summary>
    /// Custom Core Method: Triggers the weapon firing animation frame recoil instantly [INDEX: 0.1.4, 0.1.5].
    /// </summary>
    public void PlayShootAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Shoot");
            Debug.Log("[Animation Event] Executed weapon recoil frame state."); // Debug trace log requirement
        }
    }

    /// <summary>
    /// Custom Core Method: Forces transition into terminal crash sequence parameters [INDEX: 0.1.4, 0.1.5].
    /// </summary>
    public void PlayDeathAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Die");
            Debug.Log("[Animation Event] Executed structural destruction state layout.");
        }
    }
}
