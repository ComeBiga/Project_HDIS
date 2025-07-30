using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerStateMachine : MonoBehaviour
{
    public PlayerStateBase CurrentState => mCurrentState;
    public PlayerMoveState MoveState => _moveState;
    public PlayerJumpState JumpState => _jumpState;
    public PlayerRunJumpState RunJumpState => _runJumpState;
    public PlayerChangeDirectionState ChangeDirectionState => _changeDirectionState;

    [SerializeField] private PlayerMoveState _moveState;
    [SerializeField] private PlayerJumpState _jumpState;
    [SerializeField] private PlayerRunJumpState _runJumpState;
    [SerializeField] private PlayerChangeDirectionState _changeDirectionState;

    private PlayerController mController;
    private PlayerStateBase mCurrentState;
    private List<PlayerStateBase> mStates = new List<PlayerStateBase>(10);

    public void SwitchState(PlayerStateBase state)
    {
        mCurrentState?.ExitState();
        mCurrentState = state;
        mCurrentState.EnterState();
    }

    private void Start()
    {
        mController = GetComponent<PlayerController>();

        mStates.Add(_moveState);
        mStates.Add(_jumpState);
        mStates.Add(_runJumpState);
        mStates.Add(_changeDirectionState);

        foreach(var state in mStates)
        {
            state.Initialize(mController);
        }
    }
}
