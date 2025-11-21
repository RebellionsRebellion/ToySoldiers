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
    [Tooltip("Minimum distance from ground to initiate parachute")]
    [SerializeField] private float minimumHeightToDeploy = 3f;
    public float MinimumHeightToDeploy => minimumHeightToDeploy;
    [Tooltip("Maximum downward velocity to check if theyre near the ground, if they're falling faster then this no need to check if they're near the ground")]
    [SerializeField] private float groundCheckVelocityThreshold = -2f;
    public float GroundCheckVelocityThreshold => groundCheckVelocityThreshold;
    [Tooltip("Minimum distance from wall to initiate parachute")]
    [SerializeField] private float minimumDeployDistanceFromWall = 3;
    public float MinimumDeployDistanceFromWall => minimumDeployDistanceFromWall;
}

public class ParachuteState : MovementState
{
    private static readonly int IsParachuting = Animator.StringToHash("IsParachuting");
    
    public ParachuteState(StateMachine stateMachine) : base(stateMachine)
    {
    }
    
    public ParachutingSettings Settings => stateMachine.ParachutingSettings;
    public override bool UseRigidbody => true;


    protected override void SetEnterConditions()
    {
        base.SetEnterConditions();
        
        AddCanEnterCondition(CanParachute);
    }

    public override void OnEnter()
    {
        base.OnEnter();
        
        
        // Gives a small forward boost when deploying parachute
        stateMachine.GetRigidbody.AddForce(stateMachine.transform.forward * Settings.ParachuteStartBoost, ForceMode.VelocityChange);
        
        stateMachine.PlayerAnimator.SetBool(IsParachuting, true);
        stateMachine.PlayerAnimator.CrossFade("Parachuting", 0.1f);

    }

    public override void CheckTransitions()
    { 
        // Cancel state
        if (stateMachine.InputController.IsCrouching)
            SwitchState(stateMachine.FallingState);

        // Landed
        if (stateMachine.IsGrounded)
            SwitchState(stateMachine.WalkingState);
        
        // If they hit a wall
        if (IsFacingWall() && stateMachine.ClimbingState.CanClimb())
        {
            SwitchState(stateMachine.ClimbingState);
        }
    }

    public override void OnExit()
    {
        stateMachine.PlayerAnimator.SetBool(IsParachuting, false);

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
    
    private bool IsFacingWall(float distance = 1f)
    {
        Vector3 origin = stateMachine.transform.position + Vector3.up * stateMachine.PlayerHeight/2;
        return Physics.Raycast(origin, stateMachine.transform.forward, out var hit, distance, stateMachine.EnvironmentLayer);
    }

    private bool CanParachute()
    {
        // Check distance to ground if velocity is too low
        if(stateMachine.GetGroundDistance() < Settings.MinimumHeightToDeploy && stateMachine.CurrentVelocity.y > Settings.GroundCheckVelocityThreshold)
            return false;
        
        
        // Check if wall is in the wall
        if (IsFacingWall(Settings.MinimumDeployDistanceFromWall))
            return false;

        return true;
    }

}
