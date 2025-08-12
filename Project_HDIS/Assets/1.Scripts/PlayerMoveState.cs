using PropMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerMovement;

public class PlayerMoveState : PlayerStateBase
{
    private Vector3 mPreviousForward;       // ȸ���� �����ϸ� ��� �������� ������ üũ�ؾߵǱ� ������ ���� ������ ����
    private bool mbDirectionChanged = false;
    private bool mbRotating = false;        // ���� ���Ǵ� ���� ������ ȸ���� üũ�ϴ� �����̱� ������ ����

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
        // Ű�Է��� ������ ������ �ٲ�� ���� ������ ���� �ڵ�
        if (mController.InputHandler.MoveInput.x > .001f || mController.InputHandler.MoveInput.x < -.001f)
        {
            EDirection targetDirection = mController.InputHandler.MoveInput.x > 0f ? EDirection.Right : EDirection.Left;

            if (targetDirection != mController.Movement.Direction)
            {
                mbDirectionChanged = true;
                // AnimatorController�� ���� �����ϰ� ������ �ʴ� Parameter
                mController.Animator.Turning(true);
            }
            else
            {
                // ������ �ٲ�� �����ϴ� �����ӿ� transition condition ������ �ַ���
                // if-else�� ���� �� ������ ������ ������ ���� ���ϴ�
                // �׸��� ���� ���ʿ� ������ �ʴ� Parameter
                mController.Animator.SetHorizontal(mController.InputHandler.MoveInput.x);
            }

            // ������ �ٲ� �� �� �� �������ָ� �Ǳ� ������ �� ���ε� if�� �ȿ� �־��ָ� �� �� �ϴ�
            mController.Movement.SetDirection(targetDirection);
        }

        // Turn CW/CCW
        Vector3 currentForward = transform.forward;
        float deltaRotatedAngle = Vector3.SignedAngle(mPreviousForward, currentForward, Vector3.up);
        
        // ȸ�� �����ϸ� ��� ���� ȸ������ üũ �� Turn �ִϸ��̼� ��ȯ
        if (mbDirectionChanged)
        {
            // �ݽð���� ȸ�� Ʈ����
            if (deltaRotatedAngle < -5f)
            {
                mController.Animator.TurnL(true);
                mController.Animator.TurnR(false);
                // mController.Animator.Turning(false);
                mbDirectionChanged = false;

                // ȸ�� ������ ������ ȸ���̶�� �ǵ��� ���� �Ǵµ�
                // ������ ���Ǵ� ���� ������ Ȥ�� ���Ǹ� �Ű��� ����� �� ����
                mbRotating = true;
            }
            // �ð���� ȸ�� Ʈ����
            else if (deltaRotatedAngle > 5f)
            {
                mController.Animator.TurnL(false);
                mController.Animator.TurnR(true);
                // mController.Animator.Turning(false);
                mbDirectionChanged = false;
                mbRotating = true;
            }
        }
        // ȸ�� ���� üũ ���� �ƴϸ� 
        else
        {
            if (deltaRotatedAngle > -1f && deltaRotatedAngle < 1f)
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
            // ���� �Է��� ���� �� �̵� �Է��� ������ ������ RunJump
            if (mController.InputHandler.MoveInput.x > .01f || mController.InputHandler.MoveInput.x < -.01f)
            {
                mController.StateMachine.SwitchState(PlayerStateMachine.EState.RunJump);
                mController.InputHandler.ResetJump();
            }
            else
            {
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

                    // Top���� �� Ű �Է����� �� ��ٸ� Ÿ�� �� �����ϱ� ����
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
