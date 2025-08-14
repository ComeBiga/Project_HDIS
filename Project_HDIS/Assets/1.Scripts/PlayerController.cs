using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler), typeof(PlayerMovement))]
public class PlayerController : MonoBehaviour
{
    public PlayerMovement Movement => mMovement;
    public PlayerInputHandler InputHandler => mInputHandler;
    public PlayerStateMachine StateMachine => mStateMachine;
    public PlayerAnimator Animator => _animator;

    [SerializeField] private PlayerAnimator _animator;

    private PlayerInputHandler mInputHandler;
    private PlayerMovement mMovement;
    private PlayerStateMachine mStateMachine;

    // Start is called before the first frame update
    void Start()
    {
        mInputHandler = GetComponent<PlayerInputHandler>();
        mMovement = GetComponent<PlayerMovement>();
        mMovement.Initialize();

        mStateMachine = GetComponent<PlayerStateMachine>();
        mStateMachine.SwitchState(PlayerStateMachine.EState.Move);
    }

    // Update is called once per frame
    void Update()
    {
        mStateMachine.CurrentStateBase.Tick();

        mMovement.Tick();
    }
}
