using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public Vector3 Velocity => mRigidbody.velocity;
    public bool Jumping => mbJumping;
    public bool IsGrounded => mbIsGrounded;
    public Vector3 Position => transform.position;
    public EDirection Direction => mDirection;
    public EDirection OppositeDirection => (mDirection == EDirection.Left) ? EDirection.Right : EDirection.Left;

    public enum EDirection { Left, Right };

    public bool ignoreUpdateRotation = false;   // 현재 사용되지 않음

    [Header("Move")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotateSpeed = 10f;

    [Header("Jump")]
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private Transform _trGroundCheck;
    [SerializeField] private float _groundCheckRadius = .1f;
    [SerializeField] private float _groundCheckDisableDuration = .2f;   // 점프 직후 점프 중복 방지를 위해 바닥 체크하지 않는 시간
    [SerializeField] private LayerMask _groundLayer;

    [Header("Animator")]
    [SerializeField] private PlayerAnimator _animator;

    private Rigidbody mRigidbody;
    private CapsuleCollider mCapsuleCollider;
    private EDirection mDirection = EDirection.Right;
    private bool mbJumping = false;
    private bool mbIsGrounded = false;
    private float mGroundCheckDisableTimer = 0f;

    public void Initialize()
    {
        mRigidbody = GetComponent<Rigidbody>();
        mCapsuleCollider = GetComponent<CapsuleCollider>();

        mRigidbody.MoveRotation(DirectionToRotation(mDirection));
    }

    public void Move(Vector2 moveInput)
    {
        Vector3 velocity = mRigidbody.velocity;
        velocity.x = moveInput.x * _moveSpeed;
        mRigidbody.velocity = velocity;
    }
    
    public void Move(Vector2 moveInput, float speed)
    {
        Vector3 velocity = mRigidbody.velocity;
        velocity.x = moveInput.x * speed;
        mRigidbody.velocity = velocity;
    }

    // deltaPosition값을 받아 이동 처리를 하는 함수
    // 사용하려던 부분을 velocity로 처리하여 지금은 사용하지 않음
    public void MoveDelta(Vector3 deltaPosition)
    {
        mRigidbody.MovePosition(transform.position + deltaPosition);
    }

    public void SetVelocity(Vector3 velocity)
    {
        mRigidbody.velocity = velocity;
    }
    
    // RotateTo 함수는 UpdateRotation과 이름이 다른 함수지만 매개변수를 받아 Update하는 함수임
    // 매개변수를 통해 목표 방향을 전달받는 방식이 다름
    public void RotateTo(Quaternion from, EDirection direction, float t)
    {
        Quaternion targetRotation = DirectionToRotation(direction);

        mRigidbody.MoveRotation(Quaternion.Lerp(from, targetRotation, t));
    }
    
    public void RotateTo(EDirection fromDirection, EDirection toDirection, float t)
    {
        Quaternion fromRotation = DirectionToRotation(fromDirection);
        Quaternion toRotation = DirectionToRotation(toDirection);

        mRigidbody.MoveRotation(Quaternion.Lerp(fromRotation, toRotation, t));
    }

    public void RotateTo(EDirection direction, float t)
    {
        Quaternion targetRotation = DirectionToRotation(direction);

        mRigidbody.MoveRotation(Quaternion.Lerp(transform.rotation, targetRotation, t));
    }

    public void RotateTo(EDirection direction)
    {
        RotateTo(direction, Time.deltaTime * _rotateSpeed);
    }

    // 현재 방향과 반대 방향으로 변수를 설정하는 함수
    // 캐릭터의 회전을 Update 하지는 않음
    public void ChangeDirection()
    {
        mDirection = mDirection == EDirection.Right ? EDirection.Left : EDirection.Right;
    }

    // 방향을 설정하는 함수
    // 캐릭터의 회전을 Update 하지는 않음
    public void SetDirection(EDirection direction)
    {
        mDirection = direction;
    }

    // 캐릭터의 회전을 Update하는 함수
    public void UpdateRotation()
    {
        Quaternion targetRotation = DirectionToRotation(mDirection);

        mRigidbody.MoveRotation(Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * _rotateSpeed));
    }

    // 해당 방향을 나타내는 Quaternion 값을 반환
    public Quaternion DirectionToRotation(EDirection direction)
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

    // Quaternion 값으로부터 방향을 반환
    // 1 : right(90), -1 : left(-90), 0 : 그 외
    public static int RotationToDirection(Quaternion rotation)
    {
        if (Mathf.Approximately(rotation.eulerAngles.y, 90f))
            return 1;
        else if (Mathf.Approximately(rotation.eulerAngles.y, -90f))
            return -1;
        else
            return 0;
    }

    // PushPull 할 때 캐릭터의 방향과 이동을 처리
    // 지금은 방향만 한 번 세팅해주고 원래 용도대로 사용되지 않음
    // 함수를 제거하고 직접 방향을 세팅해주면 될 듯
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

    // 사다리를 탈 때 사용되는 함수
    // 현재는 애니메이션 RootMotion으로 사다리 이동을 처리하기 때문에
    // 사용되지 않는 함수
    public void ClimbLadder(Vector3 moveInput, float climbSpeed)
    {
        Vector3 velocity = mRigidbody.velocity;
        velocity.y = moveInput.y * climbSpeed;
        mRigidbody.velocity = velocity;
    }

    // 사다리를 탈 때 중력과 Collider를 비활성화 해주는 함수
    // 사다리 외에 오브젝트 탈 때도 호출되기 때문에 함수명을 변경하던지
    // Gravity, Collider 각각 나눠서 함수를 선언해야할 듯
    public void StartClimbLadder()
    {
        mRigidbody.useGravity = false;
        //mRigidbody.isKinematic = true;
        mRigidbody.velocity = Vector3.zero;
        //mRigidbody.isKinematic = false;
        mCapsuleCollider.enabled = false;
    }

    public void StopClimbLadder()
    {
        mRigidbody.useGravity = true;
        mRigidbody.isKinematic = false;
        mCapsuleCollider.enabled = true;
    }

    // 기존에 오브젝트를 탈 때 사용된 함수
    // 지금은 RootMotion으로 처리하기 때문에 사용되지 않음
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

    // transform.position을 세팅해주는 함수인데 각 위치에서 직접 세팅해주면 될 듯
    public void SetPosition(float x, float y, float z)
    {
        transform.position = new Vector3(x, y, z);
    }

    public void Tick()
    {
        // Rotate
        // updateRotation();

        // Check Ground
        checkGround();
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
            // 바닥을 Sphere로 체크하고 있는데 다른 기능들을 생각해서 Laycast로 바꿔야 될 것 같음
            mbIsGrounded = Physics.CheckSphere(_trGroundCheck.position, _groundCheckRadius, _groundLayer);

            if (mbIsGrounded)
            {
                mbJumping = false;
            }

            _animator.SetIsGrounded(mbIsGrounded);
        }
    }
}
