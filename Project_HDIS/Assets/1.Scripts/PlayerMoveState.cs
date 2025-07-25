using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerStateBase
{
    private Vector3 mPreviousForward;
    private float mRotationDirection;
    private bool mbRotating = false;
    // private PlayerMovement.EDirection mPreviousDirection;

    public override void EnterState()
    {
        // mPreviousForward = transform.forward;
        mPreviousForward = mController.Movement.Direction == PlayerMovement.EDirection.Left ?
                           Vector3.left : Vector3.right;
        // mPreviousDirection = mController.Movement.Direction;
    }

    public override void ExitState() 
    {

    }

    public override void Tick()
    {
        mController.Movement.Move(mController.InputHandler.MoveInput);

        Vector3 currentForward = transform.forward;

        mRotationDirection = Vector3.SignedAngle(mPreviousForward, currentForward, Vector3.up);


        // if(mPreviousDirection != mController.Movement.Direction)
        if(!mbRotating)
        {
            if (mRotationDirection < -5f)
            {
                mController.Animator.TurnL(true);
                mController.Animator.TurnR(false);
                mController.Animator.Turning(false);
                mbRotating = true;
            }
            else if (mRotationDirection > 5f)
            {
                mController.Animator.TurnL(false);
                mController.Animator.TurnR(true);
                mController.Animator.Turning(false);
                mbRotating = true;
            }
            //else
            //{
            //    mController.Animator.TurnL(false);
            //    mController.Animator.TurnR(false);
            //}
        }

        if (mRotationDirection > -1f && mRotationDirection < 1f)
        {
            // mPreviousDirection = mController.Movement.Direction;
            mController.Animator.TurnL(false);
            mController.Animator.TurnR(false);
            mbRotating = false;
        }

        mPreviousForward = currentForward;

        //if (mController.InputHandler.MoveInput.x > .001f || mController.InputHandler.MoveInput.x < -.001f)
        //{
        //    PlayerMovement.EDirection inputDirection = mController.InputHandler.MoveInput.x > 0f ?
        //        PlayerMovement.EDirection.Right : PlayerMovement.EDirection.Left;

        //    if(inputDirection != mController.Movement.Direction)
        //    {
        //        mController.Animator.ChangeDirection();
        //        // mController.StateMachine.SwitchState(mController.StateMachine.ChangeDirectionState);
        //    }
        //}

        if (mController.InputHandler.JumpPressed)
        {
            mController.StateMachine.SwitchState(mController.StateMachine.JumpState);
            mController.InputHandler.ResetJump();
        }
    }
}
