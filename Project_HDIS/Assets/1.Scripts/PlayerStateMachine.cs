using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerStateMachine : MonoBehaviour
{
    public enum EState { Move, IdleJump, RunJump, Ladder }

    public EState CurrentState => mCurrentState;
    public PlayerStateBase CurrentStateBase => mCurrentStateBase;
    [Obsolete] public PlayerMoveState MoveState => _moveState;
    [Obsolete] public PlayerJumpState JumpState => _jumpState;
    [Obsolete] public PlayerRunJumpState RunJumpState => _runJumpState;
    [Obsolete] public PlayerChangeDirectionState ChangeDirectionState => _changeDirectionState;

    [SerializeField] private List<PlayerStateBase> _states = new List<PlayerStateBase>();
    [SerializeField] private PlayerMoveState _moveState;
    [SerializeField] private PlayerJumpState _jumpState;
    [SerializeField] private PlayerRunJumpState _runJumpState;
    [SerializeField] private PlayerChangeDirectionState _changeDirectionState;

    private PlayerController mController;
    private EState mCurrentState;
    private PlayerStateBase mCurrentStateBase;
    private List<PlayerStateBase> mStates = new List<PlayerStateBase>(10);
    private Dictionary<EState, PlayerStateBase> mStateDic = new Dictionary<EState, PlayerStateBase>(10);

    public PlayerStateBase GetStateBase(EState state)
    {
        return mStateDic[state];
    }

    public void ResisterState(PlayerStateBase state)
    {
        state.Initialize(mController);
        mStateDic.Add(state.key, state);
    }

    [Obsolete]
    public void SwitchState(PlayerStateBase state)
    {
        mCurrentStateBase?.ExitState();
        mCurrentStateBase = state;
        mCurrentStateBase.EnterState();
    }
    
    public PlayerStateBase SwitchState(EState state)
    {

        mCurrentStateBase?.ExitState();
        mCurrentStateBase = mStateDic[state];
        mCurrentState = state;
        mCurrentStateBase.EnterState();

        mController.Animator.SetState((int)state);

        return mCurrentStateBase;
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

        foreach(var state in _states)
        {
            ResisterState(state);
            //mStateDic.Add(state.key, state);
            //state.Initialize(mController);
        }
    }
}
