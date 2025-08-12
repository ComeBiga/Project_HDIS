using PropMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLadderState : PlayerStateBase
{
    // Idle�� ���� ������ ����
    private enum EClimbType { Idle, ClimbUp, ClimbDown }

    private int TopStepIndex => mStepPositions.Count - 1;

    [SerializeField] private float _startHeight = .2f;
    [SerializeField] private float _endToPlatformXSpeed = 2f;
    [SerializeField] private float _startClimbDownYSpeed = 1.5f;
    [SerializeField] private float _rotationSpeed = 2f;

    private Animator mAnimator;
    private LadderHandler mLadderHandler;

    private bool mbLadderTop = false;
    private List<Vector3> mStepPositions;
    private int mCurrentStepIndex = 0;
    private int mMaxStepIndex;        // �Ŵ޷� ���� �� �ִ� ���� ���� StepIndex

    private float mStepNormalizedTime = 0f;     // �ִϸ��̼� normalizedTime�� ���ϱ� ���� ��
    private bool mbClimbing = false;
    private float mClimbMultiplier = 0f;        // �ִϸ��̼� Speed Multiplier
    private EClimbType mClimbType = EClimbType.Idle;
    private bool mbIsHandDefault = true;        // Hand Default : �� ���� ���� Step�� �ִ� ����
    private PlayerMovement.EDirection mPreviousDirection;
    private PlayerMovement.EDirection mLadderDirection;     // ��ٸ� ���� ��ٸ� Ÿ�� ���� �� ĳ���� ���� ó�� ��
    private float mRotatedAngles = 0f;

    // IK
    private bool mbActiveIK = false;

    private int mLeftHandStepNum = 5;
    private int mRightHandStepNum = 5;

    // Hand IK Weight�� �� ������ ���� SerializeField�� �����ϱ�
    private float mLeftHandIKWeight = 1f;
    private float mRightHandIKWeight = 1f;

    private const int DISTANCE_FOOT_TO_HAND_DEFAULT = 5;
    private const int DISTANCE_FOOT_TO_HAND_STRETCH = 6;

    public override void Initialize(PlayerController controller)
    {
        base.Initialize(controller);

        mAnimator = mController.Animator.Animator;
    }

    public override void EnterState()
    {
        mController.Movement.StartClimbLadder();
        // mController.Animator.SetLadderTop(false);        // �� ������ ���� �� LadderTop�� �Բ� ������

        // Top���� ���� �� � ������ �����ϴ� �� Ȯ���� �ʿ� ����
        // => animationStateInfo.normalizedTimed�� Top, Bottom ������� �ִϸ��̼� ���۵� �� 0���� ����
        mStepNormalizedTime = 0f;                           
        mbClimbing = false;
        mbLadderTop = false;
        mbIsHandDefault = true;
        mRotatedAngles = 0f;

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
            Vector3 deltaPosition = mController.Animator.Animator.deltaPosition;
            if (animatorStateInfo.normalizedTime > .6f)
                deltaPosition.x *= (transform.position.x < mStepPositions[TopStepIndex].x + .5f) ? _endToPlatformXSpeed : 0f;
            // deltaPosition.y *= (transform.position.y < mStepPositions[TopStepIndex].y) ? 2f : 0f;
            deltaPosition.z = 0f;
            transform.position += deltaPosition;

            return;
        }

        // Start Climb Down �ִϸ��̼� delta ���
        if (animatorStateInfo.IsTag("StartFromTop"))
        {
            // deltaPosition
            Vector3 deltaPosition = mController.Animator.Animator.deltaPosition;
            // ���� ��ġ���� �̵� ��Ű�� ���ؼ� ���� ��ġ �� ������ deltaPosition�� ��� ó��
            deltaPosition.x *= (transform.position.x > mStepPositions[mCurrentStepIndex].x - .5f) ? 2f : 0f;
            if(animatorStateInfo.normalizedTime > .6f)
                deltaPosition.y *= (transform.position.y > mStepPositions[mCurrentStepIndex].y) ? _startClimbDownYSpeed : 0f;
            deltaPosition.z = 0f;
            transform.position += deltaPosition;

            // deltaRotation
            // ���� ���⿡�� �ݴ� ������� �ִϸ��̼� normalizedTime�� ���缭 ȸ��
            //mController.Movement.RotateTo(mPreviousDirection,
            //                            mLadderDirection,
            //                            animatorStateInfo.normalizedTime);
            Vector3 eulerAngles = mController.Animator.Animator.deltaRotation.eulerAngles;
            eulerAngles.y *= _rotationSpeed;
            mRotatedAngles += eulerAngles.y;
            if (Mathf.Abs(mRotatedAngles) < 180f)
            {
                transform.rotation *= Quaternion.Euler(eulerAngles);
            }
            else
            {
                transform.rotation = mController.Movement.DirectionToRotation(mLadderDirection);
            }

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
                mCurrentStepIndex++;

                // Step ��ġ�� ���� Hand IK
                if (mCurrentStepIndex % 2 == 0)
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
                mCurrentStepIndex--;

                if (mCurrentStepIndex % 2 == 0)
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
        if (mCurrentStepIndex < 0)
        {
            mController.StateMachine.SwitchState(PlayerStateMachine.EState.Move);
            return;
        }

        // Ladder Top
        if(mCurrentStepIndex > mMaxStepIndex)
        {
            // Top�� �������� �� �� ��ġ�� ���� ó�����ִ� �ڵ��ε�
            // �������� �� �����ϸ� ��ٸ� ��ü�� Step ���� ¦���� Ȧ���� �������ִ� �������� �ص��� ��
            // if (mCurrentStepNum % 2 == 0)
            if (mbIsHandDefault)
            {
                Vector3 topPos = transform.position;
                topPos.y = mStepPositions[mCurrentStepIndex].y;
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
            if ((mClimbType == EClimbType.ClimbUp && transform.position.y < mStepPositions[mCurrentStepIndex].y)
             || (mClimbType == EClimbType.ClimbDown && transform.position.y > mStepPositions[mCurrentStepIndex].y))
            {
                transform.position += mController.Animator.Animator.deltaPosition;
            }

            // Hand IK Weight�� �ڿ������� 0���� 1���� ���
            if(mClimbType == EClimbType.ClimbUp)
            {
                if (mCurrentStepIndex % 2 == 0)
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
                if (mCurrentStepIndex % 2 == 0)
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

    public bool IsInRange(LadderHandler ladderHandler)
    {
        List<Vector3> stepPositions = ladderHandler.GetStepPositions();

        int topStepIndex = stepPositions.Count - 1;
        int maxStepIndex = topStepIndex - DISTANCE_FOOT_TO_HAND_STRETCH;

        Vector3 minStepPos = stepPositions[0];
        Vector3 maxStepPos = stepPositions[maxStepIndex];

        if(transform.position.y > minStepPos.y && transform.position.y < maxStepPos.y)
        {
            return true;
        }

        return false;
    }
    
    public bool IsOverRange(LadderHandler ladderHandler)
    {
        List<Vector3> stepPositions = ladderHandler.GetStepPositions();

        int topStepIndex = stepPositions.Count - 1;
        int maxStepIndex = topStepIndex - DISTANCE_FOOT_TO_HAND_STRETCH;

        // Vector3 minStepPos = stepPositions[0];
        Vector3 maxStepPos = stepPositions[maxStepIndex];

        if(transform.position.y > maxStepPos.y)
        {
            return true;
        }

        return false;
    }

    public void SetLadder(LadderHandler ladderHandler, bool startFromBottom)
    {
        mLadderHandler = ladderHandler;
        mStepPositions = mLadderHandler.GetStepPositions();

        mLadderDirection = mLadderHandler.GetLadderDirection();
        mPreviousDirection = mController.Movement.Direction;
        mController.Movement.SetDirection(mLadderDirection);

        if(startFromBottom)
        {
            // Step
            mCurrentStepIndex = 0;
            mMaxStepIndex = TopStepIndex - DISTANCE_FOOT_TO_HAND_STRETCH;

            // IK
            mLeftHandStepNum = mCurrentStepIndex + DISTANCE_FOOT_TO_HAND_DEFAULT;
            mRightHandStepNum = mCurrentStepIndex + DISTANCE_FOOT_TO_HAND_DEFAULT;
            mLeftHandIKWeight = 1f;
            mRightHandIKWeight = 1f;

            // Start Climb Up �ִϸ��̼� ���� �����ϱ� ������ ��ġ ��� ����
            // �ڿ��������� ���ؼ��� Lerp ó���ϴ��� �ؾ���
            Vector3 position = mController.Movement.Position;
            mController.Movement.SetPosition(position.x, mStepPositions[mCurrentStepIndex].y, position.z);
        }
        else
        {
            // Step
            // Hand Default ���¿��� ���� ���� ��ġ�� ���� ��ġ�� �����ϱ� ���� -1�� ����
            mCurrentStepIndex = TopStepIndex - DISTANCE_FOOT_TO_HAND_STRETCH - 1;
            mMaxStepIndex = TopStepIndex - DISTANCE_FOOT_TO_HAND_STRETCH;

            // IK
            mLeftHandStepNum = mCurrentStepIndex + DISTANCE_FOOT_TO_HAND_DEFAULT;
            mRightHandStepNum = mCurrentStepIndex + DISTANCE_FOOT_TO_HAND_DEFAULT;
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
