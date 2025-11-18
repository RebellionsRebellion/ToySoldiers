using System;
using UnityEngine;

public abstract class State
{
    [HideInInspector] protected StateMachine stateMachine;

    protected State(StateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public abstract void Initialize();
    public abstract void OnEnter();
    public abstract void OnExit();
    public abstract void Tick();
    public abstract void FixedTick();
    public abstract void CheckTransitions();
    public abstract bool CanEnter();
    
    public bool HasChangedState = false;
    protected bool SwitchState(State newState)
    {
        return stateMachine.SwitchState(newState, this);
    }

}

public abstract class StateSettings { }