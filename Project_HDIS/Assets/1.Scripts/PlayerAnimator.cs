using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerMovement;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    public Animator Animator => mAnimator;

    public bool enableRootMotion = false;

    private readonly int HorizontalHash = Animator.StringToHash("Horizontal");
    private readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private readonly int JumpHash = Animator.StringToHash("Jump");
    private readonly int PushPullHash = Animator.StringToHash("PushPull");
    private readonly int ClimbHash = Animator.StringToHash("Climb");
    private readonly int ClimbDownHash = Animator.StringToHash("ClimbDown");
    private readonly int ChangeDirectionHash = Animator.StringToHash("ChangeDirection");
    private readonly int TurningHash = Animator.StringToHash("Turning");
    private readonly int TurnLHash = Animator.StringToHash("TurnL");
    private readonly int TurnRHash = Animator.StringToHash("TurnR");

    private Animator mAnimator;

    public void SetHorizontal(float value)
    {
        mAnimator.SetFloat(HorizontalHash, value);
    }

    public void ChangeDirection()
    {
        mAnimator.SetTrigger(ChangeDirectionHash);
    }

    public void Turning(bool value)
    {
        mAnimator.SetBool(TurningHash, value);
    }

    public void TurnL(bool value)
    {
        mAnimator.SetBool(TurnLHash, value);
    }
    
    public void TurnR(bool value)
    {
        mAnimator.SetBool(TurnRHash, value);
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

    public void ResetRootMotionRotation()
    {
        // mAnimator.applyRootMotion = false;
        // transform.localRotation = Quaternion.Euler(Vector3.zero);
        mAnimator.rootRotation = Quaternion.identity;
        //Debug.Log("body " + mAnimator.bodyRotation.eulerAngles);
        //Debug.Log("root " + mAnimator.rootRotation.eulerAngles);
        //Debug.Log("target " + mAnimator.targetRotation.eulerAngles);
        // mAnimator.applyRootMotion = true;
        // Debug.Log(transform.rotation.eulerAngles);
    }

    public void RotateTowards(PlayerMovement.EDirection targetDirection)
    {
        Quaternion targetRotation = Quaternion.identity;

        if (targetDirection == EDirection.Right)
        {
            targetRotation = Quaternion.Euler(0f, 90f, 0f);
        }
        else if (targetDirection == EDirection.Left)
        {
            targetRotation = Quaternion.Euler(0f, -90f, 0f);
        }

        transform.localRotation = targetRotation;
    }

    public void Flip(bool value)
    {
        Quaternion targetRotation = Quaternion.identity;

        if (value)
        {
            targetRotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else
        {
            targetRotation = Quaternion.Euler(0f, 0f, 0f);
        }

        transform.localRotation = targetRotation;
    }

    private void Awake()
    {
        mAnimator = GetComponent<Animator>();
    }

    private void OnAnimatorMove()
    {
        // Debug.Log("OnAnimatorMove" + transform.rotation.eulerAngles);
        if (mAnimator.applyRootMotion && enableRootMotion)
        {
            // transform.parent.position += mAnimator.deltaPosition;
            transform.parent.rotation *= mAnimator.deltaRotation;
            // Debug.Log("RootMotioning..");
        }
    }
}
