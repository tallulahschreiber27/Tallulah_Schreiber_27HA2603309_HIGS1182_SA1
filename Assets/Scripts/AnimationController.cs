using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    private Animator animator;

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

    public void UpdateLocomotion(float horizontal, float vertical)
    {
        // These two values drive the Idle / Forward / Backward / Banking Blend Tree transitions
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
