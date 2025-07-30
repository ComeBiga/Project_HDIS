using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerChangeDirectionState : PlayerStateBase
{
    private int rotationCount = 0;
    private PlayerMovement.EDirection targetDirection;

    public override void EnterState()
    {
        //Debug.Log("=====ENTER=====");
        //Debug.Log("body " + mController.Animator.Animator.bodyRotation.eulerAngles);
        //Debug.Log("root " + mController.Animator.Animator.rootRotation.eulerAngles);
        //Debug.Log("target " + mController.Animator.Animator.targetRotation.eulerAngles);

        mController.Animator.ChangeDirection();
        mController.Animator.enableRootMotion = true;

        targetDirection = mController.Movement.Direction == PlayerMovement.EDirection.Left ?
                          PlayerMovement.EDirection.Right : PlayerMovement.EDirection.Left;

        // mController.Animator.Flip(true);
        // mController.Movement.RotateTowards(targetDirection, lerp: false);
    }

    public override void ExitState()
    {
        mController.Animator.enableRootMotion = false;
        //PlayerMovement.EDirection direction = mController.Movement.Direction == PlayerMovement.EDirection.Left ?
        //                                        PlayerMovement.EDirection.Right : PlayerMovement.EDirection.Left;
        // mController.Movement.RotateTowards(targetDirection, lerp: false);
        // mController.Movement.ChangeDirection(targetDirection);
        // mController.Animator.ResetRootMotionRotation();
        // mController.Animator.Animator.applyRootMotion = false;
        // Debug.Log(mController.Animator.Animator.GetCurrentAnimatorStateInfo(0).IsTag("ChangeDirection"));
        // mController.Animator.Flip(false);

        //Debug.Log("=====EXIT=====");
        //Debug.Log("body " + mController.Animator.Animator.bodyRotation.eulerAngles);
        //Debug.Log("root " + mController.Animator.Animator.rootRotation.eulerAngles);
        //Debug.Log("target " + mController.Animator.Animator.targetRotation.eulerAngles);
        //Debug.Log("delta " + mController.Animator.Animator.deltaRotation.eulerAngles
        //    + " rotation " + transform.rotation.eulerAngles);
    }

    public override void Tick()
    {
        // mController.Movement.RotateTowards(targetDirection);
        // Debug.Log(mController.Animator.Animator.GetCurrentAnimatorStateInfo(0).IsTag("ChangeDirection"));
        // Debug.Log(mController.Animator.Animator.deltaRotation.eulerAngles);
    }
}
