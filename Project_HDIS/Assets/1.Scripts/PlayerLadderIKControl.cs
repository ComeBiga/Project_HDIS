using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerLadderIKControl : MonoBehaviour
{
    [SerializeField] 
    private bool _ikActive;

    [SerializeField] 
    private Transform _trRightHandTarget;
    [SerializeField] 
    [Range(0f, 1f)]
    private float _rightHandTargetWeight;
    [SerializeField] 
    private Transform _trLeftHandTarget;
    [SerializeField] 
    [Range(0f, 1f)]
    private float _leftHandTargetWeight;
    [SerializeField] 
    private Transform _trRightFootTarget;
    [SerializeField] 
    [Range(0f, 1f)]
    private float _rightFootTargetWeight;
    [SerializeField] 
    private Transform _trLeftFootTarget;
    [SerializeField] 
    [Range(0f, 1f)]
    private float _leftFootTargetWeight;
    [SerializeField] 
    private Transform _trHintTarget;
    [SerializeField] 
    [Range(0f, 1f)]
    private float _hintTargetWeight;

    private Animator mAnimator;
    private PlayerStateMachine mStateMachine;

    // Start is called before the first frame update
    void Start()
    {
        mAnimator = GetComponent<Animator>();
        mStateMachine = GetComponentInParent<PlayerStateMachine>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (_ikActive)
        {
            if (mStateMachine.CurrentState == PlayerStateMachine.EState.Ladder)
            {
                mAnimator.SetIKPosition(AvatarIKGoal.RightHand, _trRightHandTarget.position);
                mAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, _rightHandTargetWeight);

                mAnimator.SetIKPosition(AvatarIKGoal.LeftHand, _trLeftHandTarget.position);
                mAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, _leftHandTargetWeight);

                mAnimator.SetIKPosition(AvatarIKGoal.RightFoot, _trRightFootTarget.position);
                mAnimator.SetIKPositionWeight(AvatarIKGoal.RightFoot, _rightFootTargetWeight);

                mAnimator.SetIKPosition(AvatarIKGoal.LeftFoot, _trLeftFootTarget.position);
                mAnimator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, _leftFootTargetWeight);

                mAnimator.SetIKHintPosition(AvatarIKHint.RightElbow, _trHintTarget.position);
                mAnimator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, _hintTargetWeight);
            }
        }
    }
}
