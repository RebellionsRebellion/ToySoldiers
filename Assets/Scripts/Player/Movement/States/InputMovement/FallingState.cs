using System;
using UnityEngine;

[Serializable]
public class FallingSettings : StateSettings
{
    [Header("Falling")]
    [Tooltip("Speed multiplier when falling")]
    [SerializeField] private float fallingHorizontalSpeedMultiplier = 0.33f;
    public float FallingHorizontalSpeedMultiplier => fallingHorizontalSpeedMultiplier;
    [Tooltip("Minimum vertical velocity needed before considering the player to be falling")]
    [SerializeField] private float fallingVelocityThreshold = 0.5f;
    public float FallingVelocityThreshold => fallingVelocityThreshold;
}

public class FallingState : InputMoveState
{
    private static readonly int IsFalling = Animator.StringToHash("IsFalling");
    
    public new FallingSettings Settings => stateMachine.FallingSettings;
    public override float GetSpeedMultiplier => Settings.FallingHorizontalSpeedMultiplier;
    public override bool CanJump => false;

    public FallingState(StateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Initialize()
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();

        stateMachine.PlayerAnimator.CrossFade("Falling", 0.25f);
        stateMachine.PlayerAnimator.SetBool(IsFalling, true);
    }

    public override void OnExit()
    {
        stateMachine.PlayerAnimator.SetBool(IsFalling, false);
    }

    public override void Tick()
    {
        base.Tick();
    }
    public override void FixedTick()
    {
    }

    public override void CheckTransitions()
    {
        base.CheckTransitions();
        // Wait until character controller detects ground
        if(stateMachine.IsGrounded)
            SwitchState(stateMachine.WalkingState);

        if (stateMachine.InputController.IsJumping)
            SwitchState(stateMachine.ParachuteState);
    }


    public override bool CanEnter()
    {
        return true;
    }
}
