using PropMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerMovement;

public class PlayerMoveState : PlayerStateBase
{
    private Vector3 mPreviousForward;
    private float mRotationDirection;
    private bool mbChangeDirection = false;
    private bool mbRotating = false;

    public override void EnterState()
    {
        mPreviousForward = mController.Movement.Direction == PlayerMovement.EDirection.Left ?
                           Vector3.left : Vector3.right;
    }

    public override void ExitState() 
    {

    }

    public override void Tick()
    {
        // Move
        mController.Movement.Move(mController.InputHandler.MoveInput);
        mController.Animator.SetInputXMagnitude(Mathf.Abs(mController.InputHandler.MoveInput.x));

        // Set Direction
        if (mController.InputHandler.MoveInput.x > .001f || mController.InputHandler.MoveInput.x < -.001f)
        {
            EDirection targetDirection = mController.InputHandler.MoveInput.x > 0f ? EDirection.Right : EDirection.Left;

            if (targetDirection != mController.Movement.Direction)
            {
                mbChangeDirection = true;
                mController.Animator.Turning(true);
            }
            else
            {
                mController.Animator.SetHorizontal(mController.InputHandler.MoveInput.x);
            }

            mController.Movement.SetDirection(targetDirection);
        }

        // Turn Animation
        Vector3 currentForward = transform.forward;

        mRotationDirection = Vector3.SignedAngle(mPreviousForward, currentForward, Vector3.up);

        if (mbChangeDirection)
        {
            if (mRotationDirection < -5f)
            {
                mController.Animator.TurnL(true);
                mController.Animator.TurnR(false);
                mController.Animator.Turning(false);
                mbChangeDirection = false;
                mbRotating = true;
            }
            else if (mRotationDirection > 5f)
            {
                mController.Animator.TurnL(false);
                mController.Animator.TurnR(true);
                mController.Animator.Turning(false);
                mbChangeDirection = false;
                mbRotating = true;
            }
            //else
            //{
            //    mController.Animator.TurnL(false);
            //    mController.Animator.TurnR(false);
            //}
        }
        else
        {
            if (mRotationDirection > -1f && mRotationDirection < 1f)
            {
                mController.Animator.TurnL(false);
                mController.Animator.TurnR(false);
                mbRotating = false;
            }
        }

        mPreviousForward = currentForward;

        // Rotate
        mController.Movement.UpdateRotation();

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

                mController.StateMachine.SwitchState(PlayerStateMachine.EState.RunJump);
                mController.InputHandler.ResetJump();
            }
            //else if (mController.InputHandler.MoveInput.x < .3f && mController.InputHandler.MoveInput.x > -.3f)
            else
            {
                // mController.StateMachine.JumpState.type = PlayerJumpState.EType.Idle;

                mController.StateMachine.SwitchState(PlayerStateMachine.EState.IdleJump);
                mController.InputHandler.ResetJump();
            }

        }

        // Ladder
        if (mController.CheckLadderObject(out Collider[] ladderColliders))
        {
            foreach (Collider ladderCollider in ladderColliders)
            {
                // Bottom
                if (mController.InputHandler.MoveInput.y > .1f)
                {
                    if (ladderCollider.tag == "LadderTop")
                        continue;

                    PlayerLadderState ladderStateBase = mController.StateMachine.GetStateBase(PlayerStateMachine.EState.Ladder) as PlayerLadderState;
                    LadderHandler ladderHandler = ladderCollider.GetComponent<LadderHandler>();

                    if (ladderStateBase.IsOverRange(ladderHandler))
                        continue;

                    ladderStateBase.SetLadder(ladderHandler, startFromBottom : true);

                    mController.StateMachine.SwitchState(PlayerStateMachine.EState.Ladder);
                }
                // Top
                else if (mController.InputHandler.MoveInput.y < -.1f)
                {
                    if (ladderCollider.tag != "LadderTop")
                        continue;

                    PlayerLadderState ladderStateBase = mController.StateMachine.GetStateBase(PlayerStateMachine.EState.Ladder) as PlayerLadderState;
                    LadderHandler ladderHandler = ladderCollider.GetComponentInParent<LadderHandler>();
                    ladderStateBase.SetLadder(ladderHandler, startFromBottom: false);

                    mController.StateMachine.SwitchState(PlayerStateMachine.EState.Ladder);
                }
            }

        }
    }
}
