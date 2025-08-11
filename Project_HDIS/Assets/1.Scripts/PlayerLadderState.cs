using PropMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLadderState : PlayerStateBase
{
    // Idle�� ���� ������ ����
    private enum EClimbType { Idle, ClimbUp, ClimbDown }

    [SerializeField] private float _startHeight = .2f;

    private Animator mAnimator;
    private Ladder mLadder;

    private bool mbLadderTop = false;
    private List<Vector3> mStepPositions;
    private int mCurrentStepNum = 0;
    private int mTopStepNum;

    private float mStepNormalizedTime = 0f;     // �ִϸ��̼� normalizedTime�� ���ϱ� ���� ��
    private bool mbClimbing = false;
    private float mClimbMultiplier = 0f;        // �ִϸ��̼� Speed Multiplier
    private EClimbType mClimbType = EClimbType.Idle;
    private bool mbIsHandDefault = true;        // Hand Default : �� ���� ���� Step�� �ִ� ����
    private PlayerMovement.EDirection mLadderDirection;     // ��ٸ� ���� ��ٸ� Ÿ�� ���� �� ĳ���� ���� ó�� ��

    // IK
    private bool mbActiveIK = false;

    private int mLeftHandStepNum = 5;
    private int mRightHandStepNum = 5;

    // Hand IK Weight�� �� ������ ���� SerializeField�� �����ϱ�
    private float mLeftHandIKWeight = 1f;
    private float mRightHandIKWeight = 1f;

    public override void Initialize(PlayerController controller)
    {
        base.Initialize(controller);

        mAnimator = mController.Animator.Animator;
    }

    public override void EnterState()
    {
        mController.Movement.StartClimbLadder();
        // mController.Animator.SetLadderTop(false);        // �� ������ ���� �� LadderTop�� �Բ� ������

        mStepNormalizedTime = 0f;                           // Top���� ���� �� � ������ �����ϴ� �� Ȯ���� �ʿ� ����
        mbClimbing = false;
        mbLadderTop = false;

        mbActiveIK = true;

        mController.Animator.onAnimationIK -= updateAnimatorIK;
        mController.Animator.onAnimationIK += updateAnimatorIK;
    }

    public override void ExitState()
    {
        mController.Movement.StopClimbLadder();
        mController.Animator.SetLadderTop(false);

        mbActiveIK = false;
        mController.Animator.onAnimationIK -= updateAnimatorIK;
    }

    public override void Tick()
    {
        var animatorStateInfo = mAnimator.GetCurrentAnimatorStateInfo(0);

        // return���� �������鼭 �ڵ尡 ���������� Coroutine���� ���� ������ �� ��
        // ��ٸ� Ÿ�� �߿� Top�� ������ ��
        if (mbLadderTop)
        {
            transform.position += mController.Animator.Animator.deltaPosition;
            return;
        }

        // Start Climb Down �ִϸ��̼� delta ���
        if (animatorStateInfo.IsTag("StartFromTop"))
        {
            // mController.Movement.SetDirection(mLadderDirection);

            // deltaPosition
            Vector3 deltaPosition = mController.Animator.Animator.deltaPosition;
            // ���� ��ġ���� �̵� ��Ű�� ���ؼ� ���� ��ġ �� ������ deltaPosition�� ��� ó��
            deltaPosition.x *= (transform.position.x > mStepPositions[mCurrentStepNum].x - .5f) ? 2f : 0f;
            deltaPosition.y *= (transform.position.y > mStepPositions[mCurrentStepNum].y) ? 2f : 0f;
            deltaPosition.z = 0f;
            transform.position += deltaPosition;

            // deltaRotation
            // ���� ���⿡�� �ݴ� ������� �ִϸ��̼� normalizedTime�� ���缭 ȸ��
            mController.Movement.RotateTo(mController.Movement.DirectionToRotation(mController.Movement.Direction),
                                        mController.Movement.OppositeDirection,
                                        animatorStateInfo.normalizedTime);

            // LadderTop�� ���ؼ� �̹� �ִϸ��̼��� ����Ʊ� ������ False ó��
            mController.Animator.SetLadderTop(false);

            // Debug.Log(transform.rotation.eulerAngles);

            return;
        }

        // Climb Up �ִϸ��̼��� �ƴϸ� �Ʒ��� ������� ����
        if (!animatorStateInfo.IsTag("ClimbUp"))
            return;

        mController.Animator.SetInputYMagnitude(Mathf.Abs(mController.InputHandler.MoveInput.y));

        // Idle ���¿��� Ű �Է��� ���� �� ó��
        if (!mbClimbing)
        {
            // �� ����
            if (mController.InputHandler.MoveInput.y > .1f)     // �ּ� Input ���� �����ϱ� ���� ������ �Է¸� üũ�ϴ� �Լ� �ۼ�?
            {
                mbClimbing = true;
                mbIsHandDefault = !mbIsHandDefault;
                mClimbMultiplier = 1f;
                mStepNormalizedTime += .5f;
                mClimbType = EClimbType.ClimbUp;
                mCurrentStepNum++;

                // Step ��ġ�� ���� Hand IK
                if (mCurrentStepNum % 2 == 0)
                {
                    mRightHandStepNum += 2;
                    mRightHandIKWeight = 0f;    // 0���� 1���� �ڿ������� �÷��ֱ����� 0 ����
                }
                else
                {
                    mLeftHandStepNum += 2;
                    mLeftHandIKWeight = 0f;
                }
            }
            // �Ʒ� ����
            else if (mController.InputHandler.MoveInput.y < -.1f)
            {
                mbClimbing = true;
                mbIsHandDefault = !mbIsHandDefault;
                mClimbMultiplier = -1f;
                mStepNormalizedTime -= .5f;
                mClimbType = EClimbType.ClimbDown;
                mCurrentStepNum--;

                if (mCurrentStepNum % 2 == 0)
                {
                    mLeftHandStepNum -= 2;
                    mLeftHandIKWeight = 0f;
                }
                else
                {
                    mRightHandStepNum -= 2;
                    mRightHandIKWeight = 0f;
                }
            }
        }

        // Ladder Bottom
        if (mCurrentStepNum < 0)
        {
            mController.StateMachine.SwitchState(PlayerStateMachine.EState.Move);
            return;
        }

        // Ladder Top
        if(mCurrentStepNum > mTopStepNum)
        {
            // Top�� �������� �� �� ��ġ�� ���� ó�����ִ� �ڵ��ε�
            // �������� �� �����ϸ� ��ٸ� ��ü�� Step ���� ¦���� Ȧ���� �������ִ� �������� �ص��� ��
            // if (mCurrentStepNum % 2 == 0)
            if (mbIsHandDefault)
            {
                Vector3 topPos = transform.position;
                topPos.y = mStepPositions[mCurrentStepNum].y;
                transform.position = topPos;
            }

            mbLadderTop = true;
            mbActiveIK = false;
            mController.Animator.SetLadderTop(true);

            return;
        }

        // Ű�Է��� ���ͼ� Climb Up�̵� Down�̵� �ϰ� �ִ� ����
        // Ű�Է� �� ���� �� ������ �������� ���
        // �ڵ� ���⼺ ������ ũ�� Climb Up, Down �б�� ������ߵ� ��
        if (mbClimbing)
        {
            mController.Animator.SetVertical(mClimbMultiplier);

            // ���� ��ġ�� ���� Step ��ġ�� �Ǳ� ������ deltaPosition ó��
            if ((mClimbType == EClimbType.ClimbUp && transform.position.y < mStepPositions[mCurrentStepNum].y)
             || (mClimbType == EClimbType.ClimbDown && transform.position.y > mStepPositions[mCurrentStepNum].y))
            {
                transform.position += mController.Animator.Animator.deltaPosition;
            }

            // Hand IK Weight�� �ڿ������� 0���� 1���� ���
            if(mClimbType == EClimbType.ClimbUp)
            {
                if (mCurrentStepNum % 2 == 0)
                {
                    // �� Step�� normalizedTime���� .5f�̱� ������ �и� .5�� ���
                    mRightHandIKWeight = (.5f - (mStepNormalizedTime - animatorStateInfo.normalizedTime)) / .5f;
                }
                else
                {
                    mLeftHandIKWeight = (.5f - (mStepNormalizedTime - animatorStateInfo.normalizedTime)) / .5f;
                }
            }
            else if(mClimbType == EClimbType.ClimbDown)
            {
                if (mCurrentStepNum % 2 == 0)
                {
                    mLeftHandIKWeight = (.5f - (animatorStateInfo.normalizedTime - mStepNormalizedTime)) / .5f;
                }
                else
                {
                    mRightHandIKWeight = (.5f - (animatorStateInfo.normalizedTime - mStepNormalizedTime)) / .5f;
                }
            }

            // normalizedTime�� �� ���ܸ�ŭ ��ȭ�ϸ� Idle�� ��ȯ
            if ((mClimbType == EClimbType.ClimbUp && animatorStateInfo.normalizedTime > mStepNormalizedTime)
             || (mClimbType == EClimbType.ClimbDown && animatorStateInfo.normalizedTime < mStepNormalizedTime))
            {

                mbClimbing = false;
                mController.Animator.SetVertical(0f);
                // Debug.Log($"Step!! [NormalizedTime : {animatorStateInfo.normalizedTime.ToString("F1")}]");
            }
        }
    }

    public void SetLadder(Ladder ladder, bool startFromBottom)
    {
        mLadder = ladder;
        mStepPositions = ladder.GetStepPositions();

        if(startFromBottom)
        {
            // Step
            mCurrentStepNum = 0;
            mTopStepNum = (mStepPositions.Count - 1) - 6;

            // IK
            mLeftHandStepNum = 5;
            mRightHandStepNum = 5;
            mLeftHandIKWeight = 1f;
            mRightHandIKWeight = 1f;

            // Start Climb Up �ִϸ��̼� ���� �����ϱ� ������ ��ġ ��� ����
            // �ڿ��������� ���ؼ��� Lerp ó���ϴ��� �ؾ���
            Vector3 position = mController.Movement.Position;
            mController.Movement.SetPosition(position.x, mStepPositions[mCurrentStepNum].y, position.z);
        }
        else
        {
            // ��ٸ� Ÿ�� ���� Ÿ���� ĳ���� ������ �ٸ��� ������ ���� ����
            mLadderDirection = mController.Movement.OppositeDirection;

            // Step
            mCurrentStepNum = (mStepPositions.Count - 1) - 5;
            mTopStepNum = (mStepPositions.Count - 1) - 5;

            // IK
            mLeftHandStepNum = (mStepPositions.Count - 1);
            mRightHandStepNum = (mStepPositions.Count - 1);
            mLeftHandIKWeight = 1f;
            mRightHandIKWeight = 1f;

            // Start Climb Down �ִϸ��̼� �� ��ġ �����ϱ� ������ LadderTop�� true�� ��
            mController.Animator.SetLadderTop(true);
        }

    }

    public void EndToPlatform()
    {
        mController.StateMachine.SwitchState(PlayerStateMachine.EState.Move);
    }

    private void updateAnimatorIK()
    {
        if (!mbActiveIK)
            return;

        Vector3 leftHandPosition = mAnimator.GetIKPosition(AvatarIKGoal.LeftHand);
        leftHandPosition.y = mStepPositions[mLeftHandStepNum].y;
        mAnimator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandPosition);
        mAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, mLeftHandIKWeight);

        Vector3 rightHandPosition = mAnimator.GetIKPosition(AvatarIKGoal.RightHand);
        rightHandPosition.y = mStepPositions[mRightHandStepNum].y;
        mAnimator.SetIKPosition(AvatarIKGoal.RightHand, rightHandPosition);
        mAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, mRightHandIKWeight);
    }

    private void OnDrawGizmosSelected()
    {
        //if (mController.StateMachine.CurrentState != PlayerStateMachine.EState.Ladder)
        //{
        //    return;
        //}

        // ���� Step�� ���� ���� ���� ��ġ�� ǥ��
        //Gizmos.DrawWireSphere(mStepPositions[mCurrentStepNum], .1f);
        //Gizmos.color = Color.blue;
        //Gizmos.DrawWireSphere(mStepPositions[mCurrentStepNum + 6], .1f);
    }
}
