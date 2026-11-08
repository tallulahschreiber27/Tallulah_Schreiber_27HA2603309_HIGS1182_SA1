using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    private Animator animator;

    // Cached Animator Parameter Hashes (Massive performance boost over raw strings!)
    private int horizontalInputHash;
    private int verticalInputHash;
    private int shootTriggerHash;
    private int deathTriggerHash;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        // Convert strings to integer hashes in Awake to avoid frame-rate stuttering
        horizontalInputHash = Animator.StringToHash("horizontalInput");
        verticalInputHash = Animator.StringToHash("verticalInput");
        shootTriggerHash = Animator.StringToHash("shoot");
        deathTriggerHash = Animator.StringToHash("death");
    }

    /// <summary>
    /// Feed your flat movement input values directly into the 2D Blend Tree.
    /// </summary>
    /// <param name="horizontal">Input.GetAxis("Horizontal") -> Left (-1) to Right (1)</param>
    /// <param name="vertical">Input.GetAxis("Vertical") -> Backward (-1) to Forward (1)</param>
    public void UpdateLocomotion(float horizontal, float vertical)
    {
        // These two values drive your Idle / Forward / Backward / Banking Blend Tree transitions
        animator.SetFloat(horizontalInputHash, horizontal);
        animator.SetFloat(verticalInputHash, vertical);
    }

    public void PlayShootAnimation()
    {
        animator.SetTrigger(shootTriggerHash);
        Debug.Log("AnimationController: Shoot animation triggered.");
    }

    public void PlayDeathAnimation()
    {
        // Separated into an explicit Death trigger to handle player destruction cleanly
        animator.SetTrigger(deathTriggerHash);
        Debug.Log("AnimationController: Death animation triggered.");
    }
}
