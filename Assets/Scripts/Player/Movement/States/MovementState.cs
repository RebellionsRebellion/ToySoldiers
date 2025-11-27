using System;
using UnityEngine;

public abstract class MovementState : State, IMovementState
{
    [HideInInspector] protected new PlayerMovement stateMachine;

    public MovementState(StateMachine stateMachine) : base(stateMachine)
    {
        this.stateMachine = (PlayerMovement)stateMachine;
    }


    public virtual bool UseGravity => true;
    public virtual bool UseRigidbody => false;
    public virtual bool UseRootMotion => false;
    public virtual bool UseMouseRotatePlayer => true;

    public override void OnEnter()
    {
        stateMachine.ToggleRigidbody(UseRigidbody);
        
        stateMachine.PlayerAnimator.applyRootMotion = UseRootMotion;
    }




}

// Used for properties common to all movement states
public interface IMovementState
{
    public bool UseGravity { get; }
    public bool UseRigidbody { get; }
    public bool UseRootMotion { get; }
    public bool UseMouseRotatePlayer { get; }
}