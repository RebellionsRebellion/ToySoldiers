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

    public override void OnEnter()
    {
        stateMachine.ToggleRigidbody(UseRigidbody);
    }




}

// Used for properties common to all movement states
public interface IMovementState
{
    public bool UseGravity { get; }
    public bool UseRigidbody { get; }
}