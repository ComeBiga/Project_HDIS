using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClimbObjectState : PlayerStateBase
{
    [Header("Climb Up")]
    // [SerializeField] private float _climbSpeed = 2f;
    [SerializeField] private float _lerpRotationDuration = .2f;     // �̵� ���⿡�� �ִϸ��̼� �������� ���� �������ִ� �ð�
    [SerializeField] private float _lerpYPosDuration = .2f;         // �ִϸ��̼� ���̷� �������ִ� �ð�
    [SerializeField] private float _climbUpZSpeed = 3f;

    [Header("Climb Down")]
    [SerializeField] private float _lerpZPosDuration = .2f;         // �ִϸ��̼� ��ġ�� �������ִ� �ð�
    [SerializeField] private float _climbDownRotateSpeed = 2f;
    [SerializeField] private float _climbDownYSpeed = 1f;
    [SerializeField] private float _climbDownZSpeed = 1f;

    private Animator mAnimator;
    private PushPullObject mPushPullObject;
    private bool mbClimbing = false;                // ������/������ ������
    private bool mbClimbUp;                         // ������ ���� ������ ����

    public override void Initialize(PlayerController controller)
    {
        base.Initialize(controller);

        mAnimator = controller.Animator.Animator;
    }

    public override void EnterState()
    {
        // Gravity�� Collider ��Ȱ��ȭ�Ϸ��� ȣ���� �Լ�
        // PlayerMovement �ڵ� �����ϸ鼭 ���� ��������� �� ��
        mController.Movement.StartClimbLadder();
        mbClimbing = true;

        if (mbClimbUp)
        {
            StartCoroutine(eClimbUp());
        }
        else
        {
            StartCoroutine(eClimbDown());
        }
    }

    public override void ExitState()
    {
        // Gravity�� Collider ��Ȱ��ȭ�Ϸ��� ȣ���� �Լ�
        // PlayerMovement �ڵ� �����ϸ鼭 ���� ��������� �� ��
        mController.Movement.StopClimbLadder();

        // �������� �� ��� ������ ���� �־��� �� üũ�ϴ� parameter
        mController.Animator.SetHorizontal(0f);
        // ������ ���� ������ ���� AnimatorController���� üũ�� �Ʒ� parameter�� �Ѵ�.
        mController.Animator.SetVertical(0f);
    }

    public override void Tick()
    {
        //AnimatorStateInfo animatorStateInfo = mAnimator.GetCurrentAnimatorStateInfo(0);

        //if (!animatorStateInfo.IsTag("ClimbObject"))
        //{
        //    // mController.StateMachine.SwitchState(PlayerStateMachine.EState.Move);
        //    return;
        //}

        //Vector3 deltaPosition = mAnimator.deltaPosition;
        //deltaPosition.y *= 2f;
        //deltaPosition.z *= (transform.position.z < 0f) ? _lerpYPosZSpeed : 0f;
        //transform.position += deltaPosition;
    }

    public void SetClimbObject(PushPullObject pushPullObject, bool climbUp)
    {
        mPushPullObject = pushPullObject;
        mbClimbUp = climbUp;

        // ������ �� �� ��� ������ ���� �־��� ���� �� �Լ����� ��������
        // ������ ���� ������ ������ PlayerMoveState���� ���ְ� �־ �Ȱ��� �� �Լ��� �ű�� ���� ��
        if (mController.Movement.Direction == PlayerMovement.EDirection.Right)
        {
            mController.Animator.SetHorizontal(1f);
        }
        else
        {
            mController.Animator.SetHorizontal(-1f);
        }
    }

    // ������Ʈ ���������� ���¸� �ִϸ��̼��� ���� �� ������� ȣ�����ְ� �ִ�.
    public void EndClimbObject()
    {
        mbClimbing = false;
        mController.StateMachine.SwitchState(PlayerStateMachine.EState.Move);
    }

    private IEnumerator eClimbUp()
    {
        // �ִϸ��̼� ������ �� y�� �������ִ� ��ġ
        Bounds ppoBounds = mPushPullObject.BoxCollider.bounds;
        Vector3 targetPos = transform.position;
        targetPos.y = ppoBounds.max.y;

        Quaternion targetRotation = Quaternion.Euler(Vector3.zero);

        float timer = 0f;

        while (mbClimbing)
        {
            if (timer < _lerpYPosDuration)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, timer / _lerpYPosDuration);
                // ���� _lerpYPosDuration�� _lerpRotationDuration ���� ���� ������ ���� if�� �ȿ� �ۼ�����
                // ���� �޶����Ÿ� if���� ������ߵ�, �����Ÿ� ������ ��������ߵ�
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, timer / _lerpRotationDuration);

                timer += Time.deltaTime;
            }

            AnimatorStateInfo animatorStateInfo = mAnimator.GetCurrentAnimatorStateInfo(0);

            if (!animatorStateInfo.IsTag("ClimbUp"))
            {
                yield return null;
                continue;
            }

            Vector3 deltaPosition = mAnimator.deltaPosition;
            deltaPosition.y *= 2f;                                  // y�� ���� speed�� ���� �������ֱ�
            // ������Ʈ�� �ݴ�������� ���ó������ �ʰ� z�� 0���� ���� ���� ��� ó��
            deltaPosition.z *= (transform.position.z < 0f) ? _climbUpZSpeed : 0f;
            transform.position += deltaPosition;

            yield return null;
        }
    }

    private IEnumerator eClimbDown()
    {
        // �ִϸ��̼� ������ �� z�� �������ִ� ��ġ
        Bounds ppoBounds = mPushPullObject.BoxCollider.bounds;
        Vector3 targetPos = transform.position;
        targetPos.z = ppoBounds.min.z;

        float timer = 0f;

        // ClimbUp�̶� �ٸ��� ���� �ڵ尡 �ٸ��� �ۼ����ִµ� Ȯ���غ� �ʿ䰡 ���� ��
        // ClimbUp�̶� �ٸ��� �ִϸ��̼� �±� Ȯ�� �ڵ带 �����༭ �̷��� �ۼ��� �ɷ� ������
        while (timer < _lerpZPosDuration)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, timer / _lerpZPosDuration);

            timer += Time.deltaTime;
            yield return null;
        }

        float rotatedAngles = 0f;

        while (mbClimbing)
        {
            // �ִϸ��̼� ��ũ Ȯ�� �ڵ尡 ��� ���� ������ ����
            AnimatorStateInfo animatorStateInfo = mAnimator.GetCurrentAnimatorStateInfo(0);

            // deltaPosition
            Vector3 deltaPosition = mAnimator.deltaPosition;
            deltaPosition.x = 0f;
            // �ݴ�������� ��� ó������ �ʰ� �ϱ� ���� ������ ��
            deltaPosition.y *= (deltaPosition.y < 0f) ? _climbDownYSpeed : 1f;
            deltaPosition.z *= (deltaPosition.z < 0f) ? _climbDownZSpeed : 1f;
            transform.position += deltaPosition;

            // deltaRotation
            Vector3 deltaEulerAngles = mAnimator.deltaRotation.eulerAngles;
            // deltaRotation ���� �ݴ� ȸ���̸� ���� ������ �ǵ��� �ٸ��� ���̴� �� �����ϱ� ���� ������ ��
            // ex) delta ������ 358����� 358 - 360 = -2�� ���
            deltaEulerAngles.y = (deltaEulerAngles.y < Number.DEG_180) ? deltaEulerAngles.y : deltaEulerAngles.y - Number.DEG_360;
            deltaEulerAngles.y *= _climbDownRotateSpeed;
            rotatedAngles += deltaEulerAngles.y;

            if (Mathf.Abs(rotatedAngles) < Number.DEG_90)
            {
                transform.rotation *= Quaternion.Euler(deltaEulerAngles);
            }
            else
            {
                transform.rotation = Quaternion.Euler(Vector3.zero);
            }

            yield return null;
        }
    }
}
