using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerMovement;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    public Animator Animator => mAnimator;

    // RootMotion은 PlayerAnimator 클래스에서 계산해주지 않아도 되기 때문에
    // 각각 위치에서 계산하고 있고 지금은 사용되지 않고 있음
    // public bool enableRootMotion = false;
    public event Action onAnimationIK = null;

    //[SerializeField]
    //private float _rootMotionSpeed = 1f;    // 지금은 사용되지 않고 있음

    private readonly int StateHash = Animator.StringToHash("State");
    private readonly int HorizontalHash = Animator.StringToHash("Horizontal");
    private readonly int VerticalHash = Animator.StringToHash("Vertical");
    private readonly int InputXMagnitudeHash = Animator.StringToHash("InputXMagnitude");
    private readonly int InputYMagnitudeHash = Animator.StringToHash("InputYMagnitude");
    private readonly int JumpHash = Animator.StringToHash("Jump");
    private readonly int LandingHash = Animator.StringToHash("Landing");
    private readonly int TurnLHash = Animator.StringToHash("TurnL");
    private readonly int TurnRHash = Animator.StringToHash("TurnR");
    private readonly int LadderTopHash = Animator.StringToHash("LadderTop");

    private Animator mAnimator;

    public void SetState(int value)
    {
        mAnimator.SetInteger(StateHash, value);
    }

    public void SetHorizontal(float value)
    {
        mAnimator.SetFloat(HorizontalHash, value);
    }

    public void SetVertical(float value)
    {
        mAnimator.SetFloat(VerticalHash, value);
    }

    public void SetInputXMagnitude(float value)
    {
        mAnimator.SetFloat(InputXMagnitudeHash, value);
    }

    public void SetInputYMagnitude(float value)
    {
        mAnimator.SetFloat(InputYMagnitudeHash, value);
    }

    public void TurnL(bool value)
    {
        mAnimator.SetBool(TurnLHash, value);
    }
    
    public void TurnR(bool value)
    {
        mAnimator.SetBool(TurnRHash, value);
    }

    public void SetJump()
    {
        mAnimator.SetTrigger(JumpHash);
    }

    public void SetLanding()
    {
        mAnimator.SetTrigger(LandingHash);
    }

    public void SetLadderTop(bool value)
    {
        mAnimator.SetBool(LadderTopHash, value);
    }

    private void Awake()
    {
        mAnimator = GetComponent<Animator>();
    }

    // 이 함수의 유무에 따라 Animator가 어떻게 달라지는 지 확인 필요
    // 이 함수가 없으면 RootMotion이 직접 계산 되는 것 같음
    // 계산에 문제가 없도록 남겨둘 필요가 있음
    private void OnAnimatorMove()
    {
        // RootMotion은 PlayerAnimator 클래스에서 계산해주지 않아도 되기 때문에
        // 지금은 사용되지 않고 있음
        //if (mAnimator.applyRootMotion && enableRootMotion)
        //{
        //    //transform.position += mAnimator.deltaPosition;
        //    //transform.rotation *= mAnimator.deltaRotation;
        //    transform.parent.position += mAnimator.deltaPosition * _rootMotionSpeed;
        //    transform.parent.rotation *= mAnimator.deltaRotation;
        //}
    }

    private void OnAnimatorIK(int layerIndex)
    {
        onAnimationIK?.Invoke();
    }
}
