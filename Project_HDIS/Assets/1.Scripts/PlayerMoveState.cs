using PropMaker;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using static PlayerMovement;

public class PlayerMoveState : PlayerStateBase
{
    [Header("Ladder")]
    [SerializeField] private float _ladderRadius = .5f;

    [Header("PushPull")]
    [SerializeField] private Transform _trPushPullOrigin;
    [SerializeField] private float _pushPullRadius;
    [SerializeField] private LayerMask _pushPullLayer;
    [SerializeField] private float _pushPullZDistance;

    private Vector3 mPreviousForward;       // ȸ���� �����ϸ� ��� �������� ������ üũ�ؾߵǱ� ������ ���� ������ ����
    private bool mbDirectionChanged = false;
    private bool mbRotating = false;        // ���� ���Ǵ� ���� ������ ȸ���� üũ�ϴ� �����̱� ������ ����
    private PushPullObject mPushPullObject = null;

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

            // Ű �Է� ����� ���� ������ �ٸ��� ���� ��ȯ
            if (targetDirection != mController.Movement.Direction)
            {
                mbDirectionChanged = true;
                mController.Movement.SetDirection(targetDirection);
            }
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
        if (checkLadderObject(out Collider[] ladderColliders))
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

        // PushPull
        if(checkPushPullObject(out PushPullObject[] pushPullObjects))
        {
            PushPullObject pushPullObject = pushPullObjects[0];
            mPushPullObject = pushPullObject;
            Bounds pushPullObjectBounds = pushPullObject.BoxCollider.bounds;
            Vector3 characterPos = transform.position;

            float distanceToPPO = 0f;

            if(characterPos.x < pushPullObjectBounds.min.x)
            {
                distanceToPPO = pushPullObjectBounds.min.x - characterPos.x;

                characterPos.z = -((_pushPullRadius - distanceToPPO) / _pushPullRadius) *_pushPullZDistance;
                transform.position = characterPos;
            }
            else if(characterPos.x > pushPullObjectBounds.max.x)
            {
                distanceToPPO = characterPos.x - pushPullObjectBounds.max.x;

                characterPos.z = -((_pushPullRadius - distanceToPPO) / _pushPullRadius) * _pushPullZDistance;
                transform.position = characterPos;
            }
            else
            {
                distanceToPPO = characterPos.z - pushPullObjectBounds.min.z;

                // PushPull
                if (distanceToPPO < _pushPullZDistance && mController.InputHandler.IsInteracting)
                {
                    PlayerPushPullState pushPullState = mController.StateMachine.GetStateBase(PlayerStateMachine.EState.PushPull) as PlayerPushPullState;
                    pushPullState.SetPushPullObject(pushPullObjects[0]);
                    mController.StateMachine.SwitchState(PlayerStateMachine.EState.PushPull);
                }
            }

            // Climb Object
            if (mController.InputHandler.MoveInput.y > .1f)
            {
                mController.Animator.SetVertical(1f);
                PlayerClimbObjectState pushPullState = mController.StateMachine.GetStateBase(PlayerStateMachine.EState.ClimbObject) as PlayerClimbObjectState;
                pushPullState.SetClimbObject(pushPullObjects[0], climbUp : true);
                mController.StateMachine.SwitchState(PlayerStateMachine.EState.ClimbObject);
            }
            else if(mController.InputHandler.MoveInput.y < -.1f)
            {
                mController.Animator.SetVertical(-1f);
                PlayerClimbObjectState pushPullState = mController.StateMachine.GetStateBase(PlayerStateMachine.EState.ClimbObject) as PlayerClimbObjectState;
                pushPullState.SetClimbObject(pushPullObjects[0], climbUp: false);
                mController.StateMachine.SwitchState(PlayerStateMachine.EState.ClimbObject);
            }
        }
    }

    private bool checkLadderObject(out Collider[] collider)
    {
        Collider[] ladderColliders = Physics.OverlapSphere(transform.position, _ladderRadius, LayerMask.GetMask("Ladder"));

        if (ladderColliders.Length > 0)
        {
            collider = ladderColliders;
            return true;
        }

        collider = null;
        return false;
    }

    private bool checkPushPullObject(out PushPullObject[] pushPullObjects)
    {
        Collider[] pushPullColliders = Physics.OverlapSphere(_trPushPullOrigin.position, _pushPullRadius, _pushPullLayer);

        if (pushPullColliders.Length > 0)
        {
            pushPullObjects = new PushPullObject[pushPullColliders.Length];

            for(int i = 0; i < pushPullColliders.Length; ++i)
            {
                pushPullObjects[i] = pushPullColliders[i].GetComponent<PushPullObject>();
            }

            return true;
        }

        pushPullObjects = null;
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        // Gizmos.DrawWireSphere(_trPushPullOrigin.position, _pushPullRadius);
    }
}
