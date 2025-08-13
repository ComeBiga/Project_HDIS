using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClimbObjectState : PlayerStateBase
{
    [SerializeField] private float _climbSpeed = 2f;
    [SerializeField] private float _lerpYPosDuration = .2f;
    [SerializeField] private float _lerpYPosZSpeed = 3f;

    private Animator mAnimator;
    private PushPullObject mPushPullObject;

    public override void Initialize(PlayerController controller)
    {
        base.Initialize(controller);

        mAnimator = controller.Animator.Animator;
    }

    public override void EnterState()
    {
        mController.Movement.StartClimbLadder();
        transform.rotation = Quaternion.Euler(Vector3.zero);

        StartCoroutine(eLerpPositionY());
    }

    public override void ExitState()
    {
        mController.Movement.StopClimbLadder();
    }

    public override void Tick()
    {
        AnimatorStateInfo animatorStateInfo = mAnimator.GetCurrentAnimatorStateInfo(0);

        if(!animatorStateInfo.IsTag("ClimbObject"))
        {
            mController.StateMachine.SwitchState(PlayerStateMachine.EState.Move);
            return;
        }

        Vector3 deltaPosition = mAnimator.deltaPosition;
        deltaPosition.y *= 2f;
        deltaPosition.z *= (transform.position.z < 0f) ? _lerpYPosZSpeed : 0f;
        transform.position += deltaPosition;
    }

    public void SetClimbObject(PushPullObject pushPullObject)
    {
        mPushPullObject = pushPullObject;
    }

    private IEnumerator eLerpPositionY()
    {
        Bounds ppoBounds = mPushPullObject.BoxCollider.bounds;
        Vector3 targetPos = transform.position;
        targetPos.y = ppoBounds.max.y;
        // transform.position = characterPos;

        float timer = 0f;

        while (timer < _lerpYPosDuration)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, timer / _lerpYPosDuration);

            timer += Time.deltaTime;
            yield return null;
        }
    }
}
