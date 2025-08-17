using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerMovement;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    public Animator Animator => mAnimator;

    // RootMotion�� PlayerAnimator Ŭ�������� ��������� �ʾƵ� �Ǳ� ������
    // ���� ��ġ���� ����ϰ� �ְ� ������ ������ �ʰ� ����
    // public bool enableRootMotion = false;
    public event Action onAnimationIK = null;

    //[SerializeField]
    //private float _rootMotionSpeed = 1f;    // ������ ������ �ʰ� ����

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

    // �� �Լ��� ������ ���� Animator�� ��� �޶����� �� Ȯ�� �ʿ�
    // �� �Լ��� ������ RootMotion�� ���� ��� �Ǵ� �� ����
    // ��꿡 ������ ������ ���ܵ� �ʿ䰡 ����
    private void OnAnimatorMove()
    {
        // RootMotion�� PlayerAnimator Ŭ�������� ��������� �ʾƵ� �Ǳ� ������
        // ������ ������ �ʰ� ����
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
