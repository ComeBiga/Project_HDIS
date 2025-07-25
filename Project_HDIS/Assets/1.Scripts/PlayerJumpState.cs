using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerStateBase
{
    public override void EnterState()
    {
        mController.Movement.Jump();
    }

    public override void ExitState()
    {

    }

    public override void Tick()
    {
        if(!mController.Movement.Jumping)
        {
            mController.StateMachine.SwitchState(mController.StateMachine.MoveState);
        }
    }
}
