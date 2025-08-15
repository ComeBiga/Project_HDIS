using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClimbObjectState : PlayerStateBase
{
    [Header("Climb Up")]
    // [SerializeField] private float _climbSpeed = 2f;
    [SerializeField] private float _lerpRotationDuration = .2f;     // 이동 방향에서 애니메이션 방향으로 각도 보정해주는 시간
    [SerializeField] private float _lerpYPosDuration = .2f;         // 애니메이션 높이로 보정해주는 시간
    [SerializeField] private float _climbUpZSpeed = 3f;

    [Header("Climb Down")]
    [SerializeField] private float _lerpZPosDuration = .2f;         // 애니메이션 위치로 보정해주는 시간
    [SerializeField] private float _climbDownRotateSpeed = 2f;
    [SerializeField] private float _climbDownYSpeed = 1f;
    [SerializeField] private float _climbDownZSpeed = 1f;

    private Animator mAnimator;
    private PushPullObject mPushPullObject;
    private bool mbClimbing = false;                // 오르기/내리기 중인지
    private bool mbClimbUp;                         // 오르기 인지 내리기 인지

    public override void Initialize(PlayerController controller)
    {
        base.Initialize(controller);

        mAnimator = controller.Animator.Animator;
    }

    public override void EnterState()
    {
        // Gravity랑 Collider 비활성화하려고 호출한 함수
        // PlayerMovement 코드 정리하면서 같이 정리해줘야 할 듯
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
        // Gravity랑 Collider 비활성화하려고 호출한 함수
        // PlayerMovement 코드 정리하면서 같이 정리해줘야 할 듯
        mController.Movement.StopClimbLadder();

        // 내리기일 때 어느 방향을 보고 있었는 지 체크하는 parameter
        mController.Animator.SetHorizontal(0f);
        // 오르기 인지 내리기 인지 AnimatorController에서 체크는 아래 parameter로 한다.
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

        // 내리기 일 때 어느 방향을 보고 있었는 지는 이 함수에서 해주지만
        // 오르기 인지 내리기 인지는 PlayerMoveState에서 해주고 있어서 똑같이 이 함수로 옮기면 좋을 듯
        if (mController.Movement.Direction == PlayerMovement.EDirection.Right)
        {
            mController.Animator.SetHorizontal(1f);
        }
        else
        {
            mController.Animator.SetHorizontal(-1f);
        }
    }

    // 오브젝트 오르내리기 상태를 애니메이션이 끝날 때 벗어나도록 호출해주고 있다.
    public void EndClimbObject()
    {
        mbClimbing = false;
        mController.StateMachine.SwitchState(PlayerStateMachine.EState.Move);
    }

    private IEnumerator eClimbUp()
    {
        // 애니메이션 시작할 때 y를 보정해주는 위치
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
                // 지금 _lerpYPosDuration과 _lerpRotationDuration 값이 같기 때문에 같은 if문 안에 작성해줌
                // 값이 달라질거면 if문을 나눠줘야됨, 같을거면 변수를 통합해줘야됨
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
            deltaPosition.y *= 2f;                                  // y에 대한 speed도 변수 선언해주기
            // 오브젝트와 반대방향으로 배수처리되지 않게 z가 0보다 작을 때만 배수 처리
            deltaPosition.z *= (transform.position.z < 0f) ? _climbUpZSpeed : 0f;
            transform.position += deltaPosition;

            yield return null;
        }
    }

    private IEnumerator eClimbDown()
    {
        // 애니메이션 시작할 때 z를 보정해주는 위치
        Bounds ppoBounds = mPushPullObject.BoxCollider.bounds;
        Vector3 targetPos = transform.position;
        targetPos.z = ppoBounds.min.z;

        float timer = 0f;

        // ClimbUp이랑 다르게 보정 코드가 다르게 작성돼있는데 확인해볼 필요가 있을 듯
        // ClimbUp이랑 다르게 애니메이션 태그 확인 코드를 안해줘서 이렇게 작성된 걸로 추측함
        while (timer < _lerpZPosDuration)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, timer / _lerpZPosDuration);

            timer += Time.deltaTime;
            yield return null;
        }

        float rotatedAngles = 0f;

        while (mbClimbing)
        {
            // 애니메이션 태크 확인 코드가 없어서 지금 사용되지 않음
            AnimatorStateInfo animatorStateInfo = mAnimator.GetCurrentAnimatorStateInfo(0);

            // deltaPosition
            Vector3 deltaPosition = mAnimator.deltaPosition;
            deltaPosition.x = 0f;
            // 반대방향으로 배수 처리되지 않게 하기 위해 조건을 줌
            deltaPosition.y *= (deltaPosition.y < 0f) ? _climbDownYSpeed : 1f;
            deltaPosition.z *= (deltaPosition.z < 0f) ? _climbDownZSpeed : 1f;
            transform.position += deltaPosition;

            // deltaRotation
            Vector3 deltaEulerAngles = mAnimator.deltaRotation.eulerAngles;
            // deltaRotation 값이 반대 회전이면 누적 각도가 의도와 다르게 쌓이는 걸 방지하기 위해 조건을 줌
            // ex) delta 각도가 358도라면 358 - 360 = -2로 계산
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
