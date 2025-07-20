using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private readonly int HorizontalHash = Animator.StringToHash("Horizontal");
    private readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private readonly int JumpHash = Animator.StringToHash("Jump");
    private readonly int PushPullHash = Animator.StringToHash("PushPull");
    private readonly int ClimbHash = Animator.StringToHash("Climb");
    private readonly int ClimbDownHash = Animator.StringToHash("ClimbDown");

    private Animator mAnimator;

    public void SetHorizontal(float value)
    {
        mAnimator.SetFloat(HorizontalHash, value);
    }

    public void SetIsGrounded(bool value)
    {
        mAnimator.SetBool(IsGroundedHash, value);
    }

    public void SetJump()
    {
        mAnimator.SetTrigger(JumpHash);
    }

    public void SetPushPull(bool value)
    {
        mAnimator.SetBool(PushPullHash, value);
    }

    public void SetClimb()
    {
        mAnimator.SetTrigger(ClimbHash);
    }

    public void SetClimbDown()
    {
        mAnimator.SetTrigger(ClimbDownHash);
    }

    private void Awake()
    {
        mAnimator = GetComponent<Animator>();
    }
}
