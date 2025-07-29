using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerStateBase
{
    public enum EType { Idle, Run }

    public EType type = EType.Idle;

    private Vector3 mMoveInput;

    public override void EnterState()
    {
        if (type == EType.Run)
        {
            mController.Movement.Jump();
        }

        // mController.Movement.Jump();
        mController.Animator.SetJump();
        mController.Animator.enableRootMotion = true;
    }

    public override void ExitState()
    {
        mController.Animator.enableRootMotion = false;
    }

    public override void Tick()
    {
        if(type == EType.Run)
        {
            mMoveInput = mController.InputHandler.MoveInput;

            if(mController.Movement.Direction == PlayerMovement.EDirection.Right)
            {
                if (mMoveInput.x < 0f)
                    mMoveInput.x = 0f;
            }
            else
            {
                if(mMoveInput.x > 0f)
                    mMoveInput.x = 0f;
            }

            mController.Movement.Move(mMoveInput);

            if (!mController.Movement.Jumping)
            {
                mController.StateMachine.SwitchState(mController.StateMachine.MoveState);
            }
        }
        //if(!mController.Movement.Jumping)
        //{
        //    mController.StateMachine.SwitchState(mController.StateMachine.MoveState);
        //}
    }
}
