using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKControl : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private bool _ikActive;
    [SerializeField] private Transform _trRightHandObj;
    [SerializeField] private Transform _trLookAtObj;

    [SerializeField]
    [Range(0f, 1f)]
    private float _rightHandWeight;

    // Start is called before the first frame update
    void Start()
    {
   
    }

    void OnAnimatorIK(int layerIndex)
    {
        // Debug.Log(layerIndex);

        if(_animator)
        {
            if(_ikActive)
            {
                if(_trLookAtObj != null)
                {
                    _animator.SetLookAtPosition(_trLookAtObj.position);
                    _animator.SetLookAtWeight(1);
                }

                if(_trRightHandObj != null)
                {
                    _animator.SetIKPosition(AvatarIKGoal.RightHand, _trRightHandObj.position);
                    _animator.SetIKRotation(AvatarIKGoal.RightHand, _trRightHandObj.rotation);
                    _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, _rightHandWeight);
                    _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, _rightHandWeight);
                }
            }
            else
            {
                _animator.SetLookAtWeight(0);
                _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            }
        }
    }
}
