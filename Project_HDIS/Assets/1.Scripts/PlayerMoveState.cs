using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerMovement;

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
        // Move
        mController.Movement.Move(mController.InputHandler.MoveInput);

        // Set Direction
        if (mController.InputHandler.MoveInput.x > .001f || mController.InputHandler.MoveInput.x < -.001f)
        {
            EDirection targetDirection = mController.InputHandler.MoveInput.x > 0f ? EDirection.Right : EDirection.Left;

            if (targetDirection != mController.Movement.Direction)
                mController.Animator.Turning(true);
            else
                mController.Animator.SetHorizontal(mController.InputHandler.MoveInput.x);

            mController.Movement.SetDirection(targetDirection);
        }

        // Turn Animation
        Vector3 currentForward = transform.forward;

        mRotationDirection = Vector3.SignedAngle(mPreviousForward, currentForward, Vector3.up);

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
            mController.Animator.TurnL(false);
            mController.Animator.TurnR(false);
            mbRotating = false;
        }

        mPreviousForward = currentForward;

        // Jump
        if (mController.InputHandler.JumpPressed)
        {
            //if(mController.InputHandler.MoveInput.x > .3f || mController.InputHandler.MoveInput.x < -.3f)
            //{
            //    mController.StateMachine.JumpState.type = PlayerJumpState.EType.Run;
            //}
            //else
            //{
            //    mController.StateMachine.JumpState.type = PlayerJumpState.EType.Idle;

            // if(mController.Animator.IsCurrentState(mController.Animator.RunStateHash))
            if (mController.InputHandler.MoveInput.x > .01f || mController.InputHandler.MoveInput.x < -.01f)
            {
                // mController.StateMachine.JumpState.type = PlayerJumpState.EType.Run;

                mController.StateMachine.SwitchState(mController.StateMachine.RunJumpState);
                mController.InputHandler.ResetJump();
            }
            //else if (mController.InputHandler.MoveInput.x < .3f && mController.InputHandler.MoveInput.x > -.3f)
            else
            {
                // mController.StateMachine.JumpState.type = PlayerJumpState.EType.Idle;

                mController.StateMachine.SwitchState(mController.StateMachine.JumpState);
                mController.InputHandler.ResetJump();
            }

        }
    }
}
