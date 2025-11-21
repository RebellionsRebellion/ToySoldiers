using System;
using PrimeTween;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

// Used https://github.com/nskoczylas/FSM-Movement for reference and inspiration

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : StateMachine
{
    private CharacterController cc;
    private Rigidbody rb;
    public Rigidbody GetRigidbody => rb;
    private CapsuleCollider col;
    
    [Header("Components")]
    public Transform PlayerTransform => transform;
    [SerializeField] private PlayerInputController inputController;
    public PlayerInputController InputController => inputController;
    [SerializeField] private Camera playerCamera;
    public Camera PlayerCamera => playerCamera;
    [SerializeField] private Animator playerAnimator;
    public Animator PlayerAnimator => playerAnimator;
    [SerializeField] private CinemachineInputAxisController cinemachineInputAxisController;
    [SerializeField] private CinemachineOrbitalFollow cinemachineOrbitalFollow;

    [Header("Attributes")] 
    [Tooltip("Height of the player character, used in things like climbing checks")]
    [SerializeField] private float playerHeight = 2f;
    public float PlayerHeight => playerHeight;
    [SerializeField] private float playerRadius = 0.5f;
    public float PlayerRadius => playerRadius;
    [Tooltip("Distance from ground to be considered grounded")]
    [SerializeField] private float minGroundDistance = 0.15f;
    public float MinGroundDistance => minGroundDistance;

    [SerializeField] private LayerMask environmentLayer;
    public LayerMask EnvironmentLayer => environmentLayer;
    [SerializeField] private MovementSettings movementSettings = new MovementSettings();
    public MovementSettings MovementSettings => movementSettings;
    
    [SerializeField] private WalkingSettings walkingSettings = new WalkingSettings();
    public WalkingSettings WalkingSettings => walkingSettings;
    [SerializeField] private SprintingSettings sprintingSettings = new SprintingSettings();
    public SprintingSettings SprintingSettings => sprintingSettings;
    [SerializeField] private CrouchingSettings crouchingSettings = new CrouchingSettings();
    public CrouchingSettings CrouchingSettings => crouchingSettings;
    [SerializeField] private SlidingSettings slidingSettings = new SlidingSettings();
    public SlidingSettings SlidingSettings => slidingSettings;

    [SerializeField] private ClimbingSettings climbingSettings = new ClimbingSettings();
    public ClimbingSettings ClimbingSettings => climbingSettings;
    [SerializeField] private JumpingSettings jumpingSettings = new JumpingSettings();
    public JumpingSettings JumpingSettings => jumpingSettings;
    [SerializeField] private FallingSettings fallingSettings = new FallingSettings();
    public FallingSettings FallingSettings => fallingSettings;
    [SerializeField] private ParachutingSettings parachutingSettings = new ParachutingSettings();
    public ParachutingSettings ParachutingSettings => parachutingSettings;
    
    private WalkingState walkingState; 
    public WalkingState WalkingState => walkingState;
    private SprintingState sprintingState;
    public SprintingState SprintingState => sprintingState;
    private CrouchingState crouchingState;
    public CrouchingState CrouchingState => crouchingState;
    private ClimbingState climbingState;
    public ClimbingState ClimbingState => climbingState;
    private SlidingState slidingState;
    public SlidingState SlidingState => slidingState;
    private JumpingState jumpingState;
    public JumpingState JumpingState => jumpingState;
    private FallingState fallingState;
    public FallingState FallingState => fallingState;
    private ParachuteState parachuteState;
    public ParachuteState ParachuteState => parachuteState;
    

    
    public Vector3 CurrentVelocity => currentVelocity;
    private Vector3 currentVelocity;
    
    
    [Header("Looking")]
    [Tooltip("Multiplier to adjust look sensitivity")]
    [SerializeField] private float lookSensitivity = 0.1f;




    private bool canRotatePlayer = true;
    public bool CanRotatePlayer => canRotatePlayer;
    
    private float defaultColliderHeight;
    private InputAxis.RecenteringSettings originalCameraRecentering;
    
    // Public Properties
    public bool IsGrounded => CheckOnGround();

    private void Awake()
    {
        PrimeTweenConfig.warnEndValueEqualsCurrent = false;
        
        cc = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        // Copy character controller collider data to capsule collider
        col.radius = cc.radius;
        col.height = cc.height;
        col.center = cc.center;
        
        Cursor.lockState = CursorLockMode.Locked;
        
        SetupStates();
        
        defaultColliderHeight = cc.height;
    }
    
    private void SetupStates()
    {
        walkingState = new WalkingState(this);
        walkingState.Initialize();
        sprintingState = new SprintingState(this);
        sprintingState.Initialize();
        crouchingState = new CrouchingState(this);
        crouchingState.Initialize();
        climbingState = new ClimbingState(this);
        climbingState.Initialize();
        slidingState = new SlidingState(this);
        slidingState.Initialize();
        jumpingState = new JumpingState(this);
        jumpingState.Initialize();
        fallingState = new FallingState(this);
        fallingState.Initialize();
        parachuteState = new ParachuteState(this);
        parachuteState.Initialize();
        
        
        SwitchState(walkingState, null);
    }

    protected override void Update()
    {
        base.Update();
        
        if (!(currentState is IMovementState { UseRigidbody: true }))
        {
            // Rotating player when no rigidbody
            if(canRotatePlayer)
                FrameLook();
            
            // Only apply velocity if currently not using rigidbody
            ApplyVelocity();
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        
        if (currentState is IMovementState { UseRigidbody: true })
        {
            // Rotating player when using rigidbody
            if(canRotatePlayer)
                FrameLook();
        }
    }


    private void ApplyVelocity()
    {
        // Apply gravity
        if (currentState is IMovementState { UseGravity: true })
        {
            currentVelocity.y += Physics.gravity.y * Time.deltaTime;
            
            // Set velocity to small negative value when grounded to prevent floating
            if (IsGrounded && currentVelocity.y < 0f)
            {
                currentVelocity.y = -1f; // Small negative value to keep grounded
            }
        }
        
        // Apply velocity for frame
        Vector3 finalVelocity = currentVelocity * Time.deltaTime;
        cc.Move(finalVelocity); 
    }

    
    private void OnAnimatorMove()
    {
        if (playerAnimator.applyRootMotion)
        {
            Vector3 animDeltaPosition = playerAnimator.deltaPosition;
            cc.Move(animDeltaPosition);
        }
    }

    public void SetVelocity(Vector3 newVelocity)
    {
        currentVelocity = newVelocity;
    }
    public void AddVelocity(Vector3 addVelocity)
    {
        currentVelocity += addVelocity;
    }
    public void SetPosition(Vector3 newPosition)
    {
        cc.enabled = false;
        transform.position = newPosition;
        cc.enabled = true;
    }
    public void SetRotation(Quaternion newRotation)
    {
        transform.rotation = newRotation;
    }
    public void SetRotation(Vector3 eulerAngles)
    {
        transform.rotation = Quaternion.Euler(eulerAngles);
    }
    
    public void ChangeHeight(float newHeight)
    {
        cc.height = newHeight;
        Vector3 center = cc.center;
        center.y = newHeight / 2f;
        cc.center = center;
    }
    public void ChangeHeightDefault()
    {
        ChangeHeight(defaultColliderHeight);
    }

    private RigidbodyInterpolation originalRigidbodyInterpolation;
    public void ToggleRigidbody(bool value)
    {
        cc.enabled = !value;
        rb.isKinematic = !value;
        if(rb.interpolation != RigidbodyInterpolation.None)
            originalRigidbodyInterpolation = rb.interpolation;
        rb.interpolation = value ? originalRigidbodyInterpolation : RigidbodyInterpolation.None;
        col.enabled = value;
    }

    private void FrameLook()
    {
        Vector3 input = inputController.FrameLook;
        
        Vector3 finalInput = input * (lookSensitivity * Time.deltaTime);
        
        // Rotate player Y axis
        transform.Rotate(Vector3.up, finalInput.x);
        
    }
    public void ToggleCameraXOrbit(bool enable)
    {
        // Toggle Cinemachine X axis
        foreach(var controller in cinemachineInputAxisController.Controllers)
        {
            if(controller.Name == "Look Orbit X")
            {
                controller.Enabled = enable;
                // Toggle player from rotating with camera
                canRotatePlayer = !enable;
                if (!enable)
                {
                    cinemachineOrbitalFollow.HorizontalAxis.TriggerRecentering();
                    cinemachineOrbitalFollow.HorizontalAxis.Recentering = originalCameraRecentering;
                }
                else
                {
                    originalCameraRecentering = cinemachineOrbitalFollow.HorizontalAxis.Recentering;
                    cinemachineOrbitalFollow.HorizontalAxis.Recentering = climbingSettings.ClimbCameraRecentering;
                }
                break;
            }
        }
    }
    
    public float GetGroundDistance()
    {
        float sphereRadius = cc.radius * 0.9f;
        Vector3 sphereOrigin = transform.position + Vector3.up * (sphereRadius);
        float maxDistance = 100f;
        
        if(Physics.SphereCast(sphereOrigin, sphereRadius, Vector3.down, out var hitInfo, maxDistance, environmentLayer))
        {
            return hitInfo.distance;
        }
        return maxDistance;
        
    }

    // Spherecast to check if on ground
    private bool CheckOnGround()
    {
        if(GetGroundDistance() < minGroundDistance)
            return true;

        return false;
    }
    
}
