using UnityEngine;

public class ParachuteState : MovementState
{
    public ParachuteState(StateMachine stateMachine) : base(stateMachine)
    {
    }

    public override bool CanEnter()
    {
        return true;
    }

    public override void OnEnter()
    {
        base.OnEnter();

        Debug.Log("parachuting");
    }

    public override void CheckTransitions()
    {
    }

    public override void Initialize()
    {
    }

    public override void OnExit()
    {
        // Cancel state
        if (stateMachine.InputController.IsCrouching)
            SwitchState(stateMachine.FallingState);

        // Landed
        if (stateMachine.IsGrounded)
            SwitchState(stateMachine.WalkingState);


    }

    public override void Tick()
    {
    }

    public override void FixedTick()
    {
        stateMachine.GetRigidbody.linearVelocity = Vector3.down * Time.fixedDeltaTime * 0.1f;
    }

}
