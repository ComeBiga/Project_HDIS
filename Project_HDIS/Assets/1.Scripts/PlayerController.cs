using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler), typeof(PlayerMovement))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] PlayerAnimator _animator;

    [Header("PushPull")]
    [SerializeField] private Transform _trPushPullOrigin;
    [SerializeField] private float _pushPullRange;
    [SerializeField] private LayerMask _pushPullLayer;

    private PlayerInputHandler mInputHandler;
    private PlayerMovement mMovement;

    private bool mbPushPull = false;
    private PushPullObject mPushPullObject;

    // Start is called before the first frame update
    void Start()
    {
        mInputHandler = GetComponent<PlayerInputHandler>();
        mMovement = GetComponent<PlayerMovement>();
        mMovement.Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        if (mInputHandler.IsInteracting && checkPushPullObject(out mPushPullObject))
        {
            mMovement.PushPull(mInputHandler.MoveInput, mPushPullObject.PushPullSpeed);

            mPushPullObject.PushPull(mMovement.Velocity);

            _animator.SetPushPull(true);
        }
        else
        {
            mMovement.Move(mInputHandler.MoveInput);

            if (mInputHandler.JumpPressed)
            {
                mMovement.Jump();
                mInputHandler.ResetJump();
            }

            if(mMovement.Jumping && checkClimbObject(out Collider collider))
            {
                Bounds bounds = collider.bounds;

                mMovement.Climb(bounds);
            }

            if(mInputHandler.DownPressed)
            {
                mMovement.ClimbDown();
                mInputHandler.ResetDown();
            }

            _animator.SetPushPull(false);
        }

        mMovement.Tick();
    }

    private bool checkClimbObject(out Collider collider)
    {
        Collider[] pushPullColliders = Physics.OverlapSphere(_trPushPullOrigin.position, _pushPullRange, _pushPullLayer);

        if (pushPullColliders.Length > 0)
        {
            collider = pushPullColliders[0];
            // pushPullObject = pushPullColliders[0].GetComponent<PushPullObject>();
            return true;
        }

        collider = null;
        return false;
    }

    private bool checkPushPullObject(out PushPullObject pushPullObject)
    {
        Collider[] pushPullColliders = Physics.OverlapSphere(_trPushPullOrigin.position, _pushPullRange, _pushPullLayer);

        if (pushPullColliders.Length > 0)
        {
            pushPullObject = pushPullColliders[0].GetComponent<PushPullObject>();
            // mPushPullObject.GetComponent<Rigidbody>().velocity = mRigidbody.velocity;

            mbPushPull = true;
            // _animator.SetBool("PushPull", true);

            return true;
        }

        pushPullObject = null;
        return false;
    }
}
