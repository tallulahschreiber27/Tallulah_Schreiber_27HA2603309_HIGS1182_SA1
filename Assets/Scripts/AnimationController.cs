using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Updates the movement state animation based on whether the player is moving forward/back.
    /// </summary>
    public void SetMovementAnimation(bool moving)
    {
        animator.SetBool("isMoving", moving);
    }

    /// <summary>
    /// Updates the horizontal banking/leaning animation using A/D input.
    /// </summary>
    /// <param name="horizontalValue">Value from -1 (Left) to 1 (Right).</param>
    public void SetHorizontalAnimation(float horizontalValue)
    {
        // Passes the raw float directly to control Blend Tree blending or transitions
        animator.SetFloat("horizontalInput", horizontalValue);
    }

    public void PlayShootAnimation()
    {
        animator.SetTrigger("shoot");
        Debug.Log("AnimationController: Shoot animation triggered.");
    }

    public void PlayHitAnimation()
    {
        animator.SetTrigger("hit");
        Debug.Log("AnimationController: Hit/Death animation triggered.");
    }
}
