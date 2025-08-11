using PropMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLadderState : PlayerStateBase
{
    [HideInInspector]
    public Ladder ladder;

    private enum EClimbType { Idle, ClimbUp, ClimbDown }

    [SerializeField] private float _climbSpeed = 2f;
    [SerializeField] private float _startHeight = .2f;
    [SerializeField] private float _groundCheckDisableDuration = .2f;
    [SerializeField] private Transform _trTopCheck;
    [SerializeField] private float _topCheckOffsetY = 1f;
    [SerializeField] private float _topCheckRadius = 1f;
    [SerializeField] private LayerMask _topCheckLayer;

    private Animator mAnimator;
    private float mGroundCheckDisableTimer = 0f;
    private bool mbLadderTop = false;
    private List<Vector3> mStepPositions;
    private int mCurrentStepNum = 0;
    private int mTopStepNum;

    private float mStepNormalizedTime = 0f;
    private bool mbClimbing = false;
    private float mClimbMultiplier = 0f;
    private EClimbType mClimbType = EClimbType.Idle;
    private bool mbIsHandDefault = true;
    private PlayerMovement.EDirection mLadderDirection;

    // IK
    private bool mbActiveIK = false;

    private int mLeftHandStepNum = 5;
    private int mRightHandStepNum = 5;

    private float mLeftHandIKWeight = 1f;
    private float mRightHandIKWeight = 1f;

    public override void EnterState()
    {
        // mController.Animator.enableRootMotion = true;
        mController.Movement.StartClimbLadder();
        // mController.Animator.SetLadderTop(false);

        //Vector3 position = mController.Movement.Position;
        //mController.Movement.SetPosition(position.x, position.y + _startHeight, position.z);

        mAnimator = mController.Animator.Animator;
        mGroundCheckDisableTimer = 0f;
        mStepNormalizedTime = 0f;
        // mCurrentStepNum = 0;
        mbClimbing = false;
        mbLadderTop = false;

        mbActiveIK = true;
        //mLeftHandStepNum = 5;
        //mRightHandStepNum = 5;
        //mLeftHandIKWeight = 1f;
        //mRightHandIKWeight = 1f;

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

        if (mbLadderTop)
        {
            transform.position += mController.Animator.Animator.deltaPosition;
            return;
        }

        if (animatorStateInfo.IsTag("StartFromTop"))
        {
            // mController.Movement.SetDirection(mLadderDirection);
            Vector3 deltaPosition = mController.Animator.Animator.deltaPosition;
            deltaPosition.x *= (transform.position.x > mStepPositions[mCurrentStepNum].x - .5f) ? 2f : 0f;
            deltaPosition.y *= (transform.position.y > mStepPositions[mCurrentStepNum].y) ? 2f : 0f;
            deltaPosition.z = 0f;
            transform.position += deltaPosition;
            // transform.rotation *= mController.Animator.Animator.deltaRotation;
            //transform.rotation = Quaternion.Lerp(mController.Movement.DirectionToRotation(mController.Movement.Direction),
            //                                    mController.Movement.DirectionToRotation(mController.Movement.OppositeDirection),
            //                                    animatorStateInfo.normalizedTime);
            mController.Movement.RotateTo(mController.Movement.DirectionToRotation(mController.Movement.Direction),
                                        mController.Movement.OppositeDirection,
                                        animatorStateInfo.normalizedTime);

            mController.Animator.SetLadderTop(false);

            Debug.Log(transform.rotation.eulerAngles);

            return;
        }

        if (!animatorStateInfo.IsTag("ClimbUp"))
            return;

        // mController.Movement.ClimbLadder(mController.InputHandler.MoveInput, _climbSpeed);
        mController.Animator.SetInputYMagnitude(Mathf.Abs(mController.InputHandler.MoveInput.y));

        if (!mbClimbing)
        {
            if (mController.InputHandler.MoveInput.y > .1f)
            {
                mbClimbing = true;
                mbIsHandDefault = !mbIsHandDefault;
                mClimbMultiplier = 1f;
                mStepNormalizedTime += .5f;
                mClimbType = EClimbType.ClimbUp;
                mCurrentStepNum++;

                if (mCurrentStepNum % 2 == 0)
                {
                    mRightHandStepNum += 2;
                    mRightHandIKWeight = 0f;
                }
                else
                {
                    mLeftHandStepNum += 2;
                    mLeftHandIKWeight = 0f;
                }
            }
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
        //if ((!mbIsHandDefault && (mCurrentStepNum + 5) == (mStepPositions.Count))
        //    || (mbIsHandDefault && (mCurrentStepNum + 5) == (mStepPositions.Count)))
        if(mCurrentStepNum > mTopStepNum)
        {
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

        if (mbClimbing)
        {
            mController.Animator.SetVertical(mClimbMultiplier);
            // var animatorStateInfo = mController.Animator.Animator.GetCurrentAnimatorStateInfo(0);

            Vector3 targetPos = transform.position;
            targetPos.y = mStepPositions[mCurrentStepNum].y;
            //// transform.position = targetPos;
            //transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * _climbSpeed);

            if ((mClimbType == EClimbType.ClimbUp && transform.position.y < mStepPositions[mCurrentStepNum].y)
             || (mClimbType == EClimbType.ClimbDown && transform.position.y > mStepPositions[mCurrentStepNum].y))
            {
                transform.position += mController.Animator.Animator.deltaPosition;
            }

            if(mClimbType == EClimbType.ClimbUp)
            {
                if (mCurrentStepNum % 2 == 0)
                {
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
        this.ladder = ladder;

        mStepPositions = ladder.GetStepPositions();
        // mCurrentStepNum = 0;

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

            Vector3 position = mController.Movement.Position;
            mController.Movement.SetPosition(position.x, mStepPositions[mCurrentStepNum].y, position.z);
        }
        else
        {
            mLadderDirection = mController.Movement.OppositeDirection;

            // Step
            mCurrentStepNum = (mStepPositions.Count - 1) - 5;
            mTopStepNum = (mStepPositions.Count - 1) - 5;

            // IK
            mLeftHandStepNum = (mStepPositions.Count - 1);
            mRightHandStepNum = (mStepPositions.Count - 1);
            mLeftHandIKWeight = 1f;
            mRightHandIKWeight = 1f;

            mController.Animator.SetLadderTop(true);
        }

    }

    public void EndToPlatform()
    {
        mController.StateMachine.SwitchState(PlayerStateMachine.EState.Move);
    }

    public bool checkLadderTop(out Collider collider)
    {
        Vector3 position = mController.Movement.Position + Vector3.up * _topCheckOffsetY;
        Collider[] topColliders = Physics.OverlapSphere(_trTopCheck.position, _topCheckRadius, _topCheckLayer);

        if (topColliders.Length > 0)
        {
            foreach(var topCollider in  topColliders)
            {
                if (topCollider.CompareTag("LadderTop"))
                {
                    collider = topCollider;
                    return true;
                }
            }
        }

        collider = null;
        return false;
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

        //Gizmos.color = Color.red;
        //Vector3 position = mController.Movement.Position + Vector3.up * _topCheckOffsetY;
        //Gizmos.DrawWireSphere(_trTopCheck.position, _topCheckRadius);

        //Gizmos.DrawWireSphere(mStepPositions[mCurrentStepNum], .1f);
        //Gizmos.color = Color.blue;
        //Gizmos.DrawWireSphere(mStepPositions[mCurrentStepNum + 6], .1f);
    }
}
