using System;
using UnityEngine;
using UnityEngine.Serialization;


[Serializable]
public class ParachutingSettings : StateSettings
{
    [Header("Parachuting")]
    [SerializeField] private float parachutingMaxSpeed = 15f;
    public float ParachuteMaxSpeed => parachutingMaxSpeed;
    [SerializeField] private float parachutingAcceleration = 30f;
    public float ParachuteAcceleration => parachutingAcceleration;
    [SerializeField] private float parachutingTurnMaxSpeed = 5f;
    public float ParachuteTurnMaxSpeed => parachutingTurnMaxSpeed;
    [SerializeField] private float parachutingTurnAcceleration = 10f;
    public float ParachuteTurnAcceleration => parachutingTurnAcceleration;
    [SerializeField] private float parachutingDiveMaxAngle = 45f;
    public float ParachuteDiveMaxAngle => parachutingDiveMaxAngle;
    [SerializeField] private float parachutingDiveSpeed = 3;
    public float ParachuteDiveAcceleration => parachutingDiveSpeed;
    [SerializeField] private Easing.EaseType parachutingDiveEasing = Easing.EaseType.InOutSine;
    public Easing.EaseType ParachuteDiveEasing => parachutingDiveEasing;
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
    public override bool UseMouseRotatePlayer => false;
    public override bool ControlRotation => true;

    private float currentDiveTime = 0.0f;


    protected override void SetEnterConditions()
    {
        base.SetEnterConditions();
        
        AddCanEnterCondition(CanParachute);
    }

    public override void OnEnter()
    {
        base.OnEnter();
        
        stateMachine.PlayerCamera.ChangeCamera(PlayerCamera.CameraType.Parachute);
        
        // Gives a small forward boost when deploying parachute
        stateMachine.GetRigidbody.AddForce(stateMachine.transform.forward * Settings.ParachuteStartBoost, ForceMode.VelocityChange);
        
        stateMachine.PlayerAnimator.SetBool(IsParachuting, true);
        stateMachine.PlayerAnimator.CrossFade("Parachuting", 0.1f);
        
        currentDiveTime = 0.0f;

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
        
        stateMachine.PlayerCamera.ChangeCamera(PlayerCamera.CameraType.Main);

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
        
        var rb = stateMachine.GetRigidbody;
        
        // Turn horizontally
        float targetTurn  = input.x * Settings.ParachuteTurnMaxSpeed;
        float currentTurn = rb.angularVelocity.y;
        float newY = Mathf.MoveTowards(
            currentTurn,
            targetTurn,
            Settings.ParachuteTurnAcceleration * Time.fixedDeltaTime
        );
        Vector3 ang1 = rb.angularVelocity;
        ang1.y = newY;
        rb.angularVelocity = ang1;   
        
        // Tilt forward/back based on vertical input
        if (Mathf.Abs(input.y) > 0)
            currentDiveTime += Time.fixedDeltaTime;
        else
            currentDiveTime -= Time.fixedDeltaTime;
        currentDiveTime = Mathf.Clamp(currentDiveTime, 0.0f, 1.0f);
        
        var diveEase = Easing.FindEaseType(Settings.ParachuteDiveEasing);
        float diveFactor = diveEase(currentDiveTime);
        float targetPitch = input.y * Settings.ParachuteDiveMaxAngle * diveFactor;
        Vector3 currentEuler = stateMachine.transform.eulerAngles;
        // Handle wrap around
        if (currentEuler.x > 180f)
            currentEuler.x -= 360f;
        float newX = Mathf.MoveTowards(
            currentEuler.x,
            targetPitch,
            Settings.ParachuteDiveAcceleration * Time.fixedDeltaTime
        );
        stateMachine.transform.eulerAngles = new Vector3(newX, currentEuler.y, currentEuler.z);



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
