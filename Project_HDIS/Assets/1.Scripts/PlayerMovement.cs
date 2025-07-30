using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public Vector3 Velocity => mRigidbody.velocity;
    public bool Jumping => mbJumping;
    public Vector3 Position => transform.position;
    public EDirection Direction => mDirection;

    public enum EDirection { Left, Right };

    [Header("Move")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotateSpeed = 10f;

    [Header("Jump")]
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private Transform _trGroundCheck;
    [SerializeField] private float _groundCheckRadius = .1f;
    [SerializeField] private float _groundCheckDisableDuration = .2f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Animator")]
    [SerializeField] private PlayerAnimator _animator;

    private Rigidbody mRigidbody;
    private EDirection mDirection = EDirection.Right;
    private bool mbJumping = false;
    private bool mbIsGrounded = false;
    private float mGroundCheckDisableTimer = 0f;

    public void Initialize()
    {
        mRigidbody = GetComponent<Rigidbody>();

        mRigidbody.MoveRotation(DirectionToRotation(mDirection));
    }

    public void Move(Vector2 moveInput)
    {
        Vector3 velocity = mRigidbody.velocity;
        velocity.x = moveInput.x * _moveSpeed;
        mRigidbody.velocity = velocity;
    }

    public void ChangeDirection()
    {
        mDirection = mDirection == EDirection.Right ? EDirection.Left : EDirection.Right;
    }

    public void SetDirection(EDirection direction)
    {
        mDirection = direction;
    }

    public void PushPull(Vector2 moveInput, float speed)
    {
        Vector3 velocity = mRigidbody.velocity;
        velocity.x = moveInput.x * speed;
        mRigidbody.velocity = velocity;

        // rotateTowards(Vector3.forward, 0f);
        mRigidbody.MoveRotation(Quaternion.Euler(0f, 0f, 0f));

        _animator.SetHorizontal(moveInput.x);
    }

    public void Jump()
    {
        if(mbIsGrounded)
        {
            mbJumping = true;
            mRigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            mGroundCheckDisableTimer = _groundCheckDisableDuration;

            // _animator.SetJump();
        }
    }

    public void StopJump()
    {
        mbJumping = false;
        mbIsGrounded = true;

        mRigidbody.isKinematic = true;
        mRigidbody.velocity = Vector3.zero;
        mRigidbody.isKinematic = false;
    }
    public void Climb(Bounds bounds)
    {
        //SetPosition(Position.x, bounds.max.y, bounds.min.z + (bounds.size.z / 2f));
        SetPosition(Position.x, bounds.max.y + .02f, bounds.min.z);
        mRigidbody.MoveRotation(Quaternion.Euler(0f, 0f, 0f));
        StopJump();

        _animator.SetClimb();
    }

    public void ClimbDown()
    {
        mRigidbody.MoveRotation(Quaternion.Euler(0f, 180f, 0f));
        _animator.SetClimbDown();
    }

    public void SetPosition(float x, float y, float z)
    {
        transform.position = new Vector3(x, y, z);
    }

    public void Tick()
    {
        // Rotate
        updateRotation();

        // Check Ground
        checkGround();
    }

    private void updateRotation()
    {
        Quaternion targetRotation = DirectionToRotation(mDirection);

        mRigidbody.MoveRotation(Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * _rotateSpeed));
    }

    private void checkGround()
    {
        if (mGroundCheckDisableTimer > 0f)
        {
            mGroundCheckDisableTimer -= Time.deltaTime;
            mbIsGrounded = false;

            _animator.SetIsGrounded(mbIsGrounded);
        }
        else
        {
            mbIsGrounded = Physics.CheckSphere(_trGroundCheck.position, _groundCheckRadius, _groundLayer);

            if (mbIsGrounded)
            {
                mbJumping = false;
            }

            _animator.SetIsGrounded(mbIsGrounded);
        }
    }

    private Quaternion DirectionToRotation(EDirection direction)
    {
        Quaternion targetRotation = Quaternion.identity;

        if (direction == EDirection.Right)
        {
            targetRotation = Quaternion.Euler(0f, 90f, 0f);
        }
        else if (direction == EDirection.Left)
        {
            targetRotation = Quaternion.Euler(0f, -90f, 0f);
        }
        
        return targetRotation;
    }
}
