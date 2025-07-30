using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunJumpState : PlayerStateBase
{
    private Vector3 mMoveInput;

    public override void EnterState()
    {
        mController.Movement.Jump();

        mController.Animator.SetRunJump();
    }

    public override void ExitState()
    {

    }

    public override void Tick()
    {
        mMoveInput = mController.InputHandler.MoveInput;

        if (mController.Movement.Direction == PlayerMovement.EDirection.Right)
        {
            if (mMoveInput.x < 0f)
                mMoveInput.x = 0f;
        }
        else
        {
            if (mMoveInput.x > 0f)
                mMoveInput.x = 0f;
        }

        mController.Movement.Move(mMoveInput);
        mController.Animator.SetHorizontal(mController.InputHandler.MoveInput.x);

        if (!mController.Movement.Jumping)
        {
            mController.StateMachine.SwitchState(mController.StateMachine.MoveState);
        }
    }
}

