using System;
using UnityEngine;


[Serializable]
public class ParachutingSettings : StateSettings
{
    [Header("Parachuting")]
    [SerializeField] private float parachutingMaxSpeed = 15f;
    public float ParachuteMaxSpeed => parachutingMaxSpeed;
    [SerializeField] private float parachutingAcceleration = 30f;
    public float ParachuteAcceleration => parachutingAcceleration;
    [SerializeField] private float parachutingTurnSpeed = 5f;
    public float ParachuteTurnSpeed => parachutingTurnSpeed;
    [SerializeField] private float parachutingGravity = -3f;
    public float ParachuteGravity => parachutingGravity;
    [SerializeField] private float parachutingStartBoost;
    public float ParachuteStartBoost => parachutingStartBoost;
}

public class ParachuteState : MovementState
{

    public ParachuteState(StateMachine stateMachine) : base(stateMachine)
    {
    }
    
    public ParachutingSettings Settings => stateMachine.ParachutingSettings;
    public override bool UseRigidbody => true;

    public override bool CanEnter()
    {
        return true;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        
        
        // Gives a small forward boost when deploying parachute
        stateMachine.GetRigidbody.AddForce(stateMachine.transform.forward * Settings.ParachuteStartBoost, ForceMode.VelocityChange);

    }

    public override void CheckTransitions()
    {
                // Cancel state
        if (stateMachine.InputController.IsCrouching)
            SwitchState(stateMachine.FallingState);

        // Landed
        if (stateMachine.IsGrounded)
            SwitchState(stateMachine.WalkingState);
    }

    public override void Initialize()
    {
    }

    public override void OnExit()
    {

    }

    public override void Tick()
    {
    }

    public override void FixedTick()
    {
        // Move forward 
        Vector3 target = stateMachine.transform.forward * Settings.ParachuteMaxSpeed;
        Vector3 current = stateMachine.GetRigidbody.linearVelocity;
        Vector3 next = Vector3.MoveTowards(current, target, Settings.ParachuteAcceleration * Time.fixedDeltaTime);
        stateMachine.GetRigidbody.linearVelocity = new Vector3(next.x, current.y, next.z);
        
        // Apply gentle gravity
        Vector3 gravityVelocity = Vector3.up * (Settings.ParachuteGravity * Time.fixedDeltaTime);
        stateMachine.GetRigidbody.AddForce(gravityVelocity, ForceMode.Force);
        
        // Rotate based on input
        Vector2 input = stateMachine.InputController.FrameMove;
        float turnAmount = input.x * Settings.ParachuteTurnSpeed * Time.fixedDeltaTime;
        stateMachine.GetRigidbody.AddTorque(Vector3.up * turnAmount, ForceMode.Force);
        
    }

}
