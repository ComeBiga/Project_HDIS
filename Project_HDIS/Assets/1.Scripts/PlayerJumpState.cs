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
        mController.Animator.SetJump();
    }

    public override void ExitState()
    {

    }

    public override void Tick()
    {
        mController.Animator.SetHorizontal(mController.InputHandler.MoveInput.x);
    }
}
