using System;
using System.Collections;
using PrimeTween;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[Serializable]
public class ClimbingSettings : StateSettings
{
    [Header("Climbing")]
    [SerializeField] private LayerMask climbableLayer;
    public LayerMask ClimbableLayer => climbableLayer;
    [SerializeField] private float climbRange = 1f;
    public float ClimbRange => climbRange;
    [SerializeField] private Vector2 climbingAngleLimits = new Vector2(-40f, 40f);
    public Vector2 ClimbingAngleLimits => climbingAngleLimits;
    [SerializeField] private float climbVerticalSpeed = 1f;
    public float ClimbVerticalSpeed => climbVerticalSpeed;
    [SerializeField] private float climbHorizontalSpeed = 1f;
    public float ClimbHorizontalSpeed => climbHorizontalSpeed;
    [SerializeField] private InputAxis.RecenteringSettings climbCameraRecentering;
    public InputAxis.RecenteringSettings ClimbCameraRecentering => climbCameraRecentering;
    [Tooltip("Initially to stop player from reclimbing immediately after jumping")]
    [SerializeField] private float climbingDelayAfterJump = 0.5f;
    public float ClimbingDelayAfterJump => climbingDelayAfterJump;
    [SerializeField] private float climbingRetryDelay = 0.5f;
    public float ClimbingRetryDelay => climbingRetryDelay;
    [Tooltip("Time taken for player to lock onto the wall when they start climbing")]
    [SerializeField] private float climbingStartLockIntoPlace = 0.25f;
    public float ClimbingStartLockIntoPlace => climbingStartLockIntoPlace;
    [SerializeField] private float climbDistanceFromWall = 0.33f;
    public float ClimbDistanceFromWall => climbDistanceFromWall;
    [Tooltip("Speed multiplier while player is holding sprint button")]
    [SerializeField] private float climbSprintSpeedMultiplier = 1.5f;
    public float ClimbSprintSpeedMultiplier => climbSprintSpeedMultiplier;
    [SerializeField] private float climbVaultDistance = 0.5f;
    public float ClimbVaultDistance => climbVaultDistance;
    [SerializeField] private float climbVaultDuration = 1f;
    public float ClimbVaultDuration => climbVaultDuration;
    [Tooltip("Brief delay after hanging before being able to vault")]
    [SerializeField] private float climbVaultDelayAfterHang = 0.4f;
    public float ClimbVaultDelayAfterHang => climbVaultDelayAfterHang;
    [Tooltip("Control vertical speed of vaulting animation")]
    [SerializeField] private AnimationCurve climbVaultEasingVertical;
    public AnimationCurve ClimbVaultEasingVertical => climbVaultEasingVertical;
    [Tooltip("Control horizontal speed of vaulting animation")]
    [SerializeField] private AnimationCurve climbVaultEasingHorizontal;
    public AnimationCurve ClimbVaultEasingHorizontal => climbVaultEasingHorizontal;
    
    [Header("Climbing Stamina")]
    [SerializeField] private CanvasGroup climbStaminaUI;
    public CanvasGroup ClimbStaminaUI => climbStaminaUI;
    [SerializeField] private Image climbStaminaBar;
    public Image ClimbStaminaBar => climbStaminaBar;
    [Tooltip("Maximum stamina before falling")]
    [SerializeField] private float maxClimbStamina = 5f;
    public float MaxClimbStamina => maxClimbStamina;
    [Tooltip("Stamina drain rate per second while climbing")]
    [SerializeField] private float climbStaminaDrainRate = 1f;
    public float ClimbStaminaDrainRate => climbStaminaDrainRate;
    [Tooltip("Stamina regen rate per second while hanging")]
    [SerializeField] private float climbStaminaRegenRate = 2f;
    public float ClimbStaminaRegenRate => climbStaminaRegenRate;
    [SerializeField] private float climbStaminaFadeDuration = 0.5f;
    public float ClimbStaminaFadeDuration => climbStaminaFadeDuration;
    [SerializeField] private Gradient climbStaminaGradient;
    public Gradient ClimbStaminaGradient => climbStaminaGradient;
    [SerializeField] private float climbStartMinStamina = 0.2f;
    public float ClimbStartMinStamina => climbStartMinStamina;
}

public class ClimbingState : MovementState
{
    private static readonly int IsClimbing = Animator.StringToHash("IsClimbing");
    private static readonly int ClimbSpeed = Animator.StringToHash("ClimbSpeed");
    private static readonly int IsHanging = Animator.StringToHash("IsHanging");

    private ClimbingSettings Settings => stateMachine.ClimbingSettings;
    
    public ClimbingState(StateMachine stateMachine) : base(stateMachine)
    {
    }

    public override bool UseGravity => false;
    public override bool UseRootMotion => true;

    private Tween hangToVaultDelay;
    private Tween staminaFadeTween;
    private bool didVault;
    private float climbTimer;
    private bool isHanging;
    private float currentStamina;

    public override void Initialize()
    {
        base.Initialize();
        
        Settings.ClimbStaminaUI.alpha = 0.0f;
        
        currentStamina = Settings.MaxClimbStamina;

    }

    public override void OnEnter()
    {
        base.OnEnter();
        
        // If no start data, climb condition hasnt properly been checked
        if (!climbStartData.IsValid)
        {
            Debug.LogWarning("ClimbState entered without valid climb start data");
            SwitchState(stateMachine.FallingState);
            return;
        }

        this.stateMachine.ToggleCameraXOrbit(true);
        
        stateMachine.PlayerAnimator.SetBool(IsClimbing, true);
        
        currentStamina = Settings.MaxClimbStamina;
        
        ToggleStaminaBar(true);

        climbTimer = 0.0f;
        didVault = false;
    }

    public override void OnExit()
    {
        this.stateMachine.ToggleCameraXOrbit(false);
        if(!didVault)
            stateMachine.SetVelocity(Vector3.zero);
        
        // Climb speed needs to be set to 1 to finish climb animation properly
        stateMachine.PlayerAnimator.SetFloat(ClimbSpeed, 1);
        
        ToggleStaminaBar(false);
        
        StopClimbing();
    }

    private void StopClimbing()
    {
        stateMachine.PlayerAnimator.SetBool(IsClimbing, false);
        stateMachine.PlayerAnimator.applyRootMotion = false;
    }
    

    public override void Tick()
    {
        if(didVault)
            return;
        
        ClimbDirections climbState = GetClimbState();
        // If they can climb or hang
        if (CanClimb(climbState))
        {
            // Get input
            Vector3 input = stateMachine.InputController.FrameMove;
            
            float upInput = input.y;
            // Only move up if they can
            if (upInput > 0f && !climbState.HasFlag(ClimbDirections.Up))
                upInput = 0f;
            
            float sideInput = input.x;
            // Only move sideways if theres space
            if (sideInput < 0f && !climbState.HasFlag(ClimbDirections.Left))
                sideInput = 0f;
            if (sideInput > 0f && !climbState.HasFlag(ClimbDirections.Right))
                sideInput = 0f;
            
            // If moving sideways but not up, disable root motion
            if (Mathf.Abs(sideInput) > 0f && Mathf.Abs(upInput) <= 0f)
            {
                stateMachine.PlayerAnimator.applyRootMotion = false;
            }
            else
            {
                stateMachine.PlayerAnimator.applyRootMotion = true;
            }
            
            float currentClimbSpeed = Mathf.Abs(upInput) > Mathf.Abs(sideInput) ? upInput : sideInput;
            // Apply sprint multiplier
            if (stateMachine.InputController.IsSprinting)
                currentClimbSpeed *= Settings.ClimbSprintSpeedMultiplier;
            
            // Drain stamina if moving in either direction
            if (Mathf.Abs(upInput) > 0f || Mathf.Abs(sideInput) > 0f)
            {
                currentStamina -= Settings.ClimbStaminaDrainRate * Mathf.Abs(currentClimbSpeed) * Time.deltaTime;
                currentStamina = Mathf.Max(0f, currentStamina);
            }
            

            
            stateMachine.PlayerAnimator.SetFloat(ClimbSpeed, currentClimbSpeed);
            
            Vector3 upVelocity = Vector3.up * (Settings.ClimbVerticalSpeed * upInput);
            Vector3 rightVelocity = stateMachine.PlayerTransform.right * (Settings.ClimbHorizontalSpeed * sideInput);
            Vector3 finalVelocity = upVelocity + rightVelocity;
            // Apply sprint multiplier
            if (stateMachine.InputController.IsSprinting)
                finalVelocity *= Settings.ClimbSprintSpeedMultiplier;
            
            stateMachine.SetVelocity(finalVelocity);
        }
        
        // If they cant climb up
        if (CanHang(climbState))
        {
            // If not currently hanging, start hanging
            if(!isHanging)
                StartHanging();
            
            // Regain stamina while hanging
            currentStamina += Settings.ClimbStaminaRegenRate * Time.deltaTime;
            
            // Can jump to vault over ledge
            if (stateMachine.InputController.JumpDown && !hangToVaultDelay.isAlive)
            {
                VaultOverLedge();
            }
        }
        else if(isHanging)
        {
            isHanging = false;
            stateMachine.PlayerAnimator.SetBool(IsHanging, false);
        }
        
        // Lock player to wall at start
        if (climbTimer < Settings.ClimbingStartLockIntoPlace)
        {
            float lockT = climbTimer / Settings.ClimbingStartLockIntoPlace;
            Vector3 direction = (climbStartData.PlayerPosition - climbStartData.RaycastHit.point).normalized;
            // Lerp in direction
            Vector3 targetPosition = climbStartData.RaycastHit.point + direction * Settings.ClimbDistanceFromWall; // Half meter from wall
            stateMachine.SetPosition(Vector3.Lerp(climbStartData.PlayerPosition, targetPosition, lockT));
            // Face wall
            Quaternion targetRotation = Quaternion.LookRotation(-climbStartData.RaycastHit.normal);
            stateMachine.SetRotation(Quaternion.Slerp(stateMachine.PlayerTransform.rotation, targetRotation, lockT));
        }
        
        UpdateStaminaUI();
        
        climbTimer += Time.deltaTime;

    }

    public override void FixedTick()
    {
    }

    private void UpdateStaminaUI()
    {
        float t = currentStamina / Settings.MaxClimbStamina;
        Settings.ClimbStaminaBar.fillAmount = t;
        
        Color staminaColor = Settings.ClimbStaminaGradient.Evaluate(1-t);
        Settings.ClimbStaminaBar.color = staminaColor;
    }
    
    private float GetStaminaPercentage()
    {
        return currentStamina / Settings.MaxClimbStamina;
    }

    private void ToggleStaminaBar(bool value)
    {
        float alpha = value ? 1f : 0f;
        
        if(staminaFadeTween.isAlive)
            staminaFadeTween.Stop();
        
        staminaFadeTween = Tween.Alpha(Settings.ClimbStaminaUI, alpha, Settings.ClimbStaminaFadeDuration);
    }
    
    private void StartHanging()
    {
        isHanging = true;
        
        stateMachine.PlayerAnimator.SetBool(IsHanging, true);
        
        hangToVaultDelay = Tween.Delay(Settings.ClimbVaultDelayAfterHang);
    }

    private void VaultOverLedge()
    {
        SetDelay(Settings.ClimbingDelayAfterJump);
        
        ToggleStaminaBar(false);
                
        //stateMachine.AddVelocity(Vector3.up * stateMachine.JumpForce);
        
        // Calculate ending position
        Vector3 verticalOffset = Vector3.up * (stateMachine.PlayerHeight + 0.5f);
        Vector3 forwardOffset = stateMachine.PlayerTransform.forward * Settings.ClimbVaultDistance;
        Vector3 targetPosition = stateMachine.PlayerTransform.position + verticalOffset + forwardOffset;
        // Raycast down to find ground
        Vector3 vaultPosition = targetPosition;
        Ray downRay = new Ray(targetPosition, Vector3.down);
        if (Physics.Raycast(downRay, out var hitInfo, stateMachine.PlayerHeight + 1.5f, Settings.ClimbableLayer))
        {
            vaultPosition = hitInfo.point + Vector3.up * 0.1f; // Slightly above ground
        }
        
        stateMachine.PlayerAnimator.CrossFade("ClimbingFinish", 0.1f);
        //StopClimbing();

        stateMachine.StartCoroutine(VaultingOverLedge(vaultPosition));
                
        didVault = true;
    }

    private IEnumerator VaultingOverLedge(Vector3 targetPosition)
    {

        float vaultTimer;
        float vaultDuration = Settings.ClimbVaultDuration;
        Vector3 startPosition = stateMachine.PlayerTransform.position;
        for (vaultTimer = 0f; vaultTimer < vaultDuration; vaultTimer += Time.deltaTime)
        {
            float t = vaultTimer / vaultDuration;
            
            // Use vertical and horizontal easing curves
            float verticalT = Settings.ClimbVaultEasingVertical.Evaluate(t);
            float horizontalT = Settings.ClimbVaultEasingHorizontal.Evaluate(t);
            // Lerp separately
            Vector3 verticalPosition = Vector3.Lerp(startPosition, new Vector3(startPosition.x, targetPosition.y, startPosition.z), verticalT);
            Vector3 horizontalPosition = Vector3.Lerp(startPosition, new Vector3(targetPosition.x, startPosition.y, targetPosition.z), horizontalT);
            Vector3 newPosition = new Vector3(horizontalPosition.x, verticalPosition.y, horizontalPosition.z);
            stateMachine.SetPosition(newPosition);
            yield return null;
        }
        
        // Switch to walking state
        SwitchState(stateMachine.WalkingState);
    }

    public override void CheckTransitions()
    {
        var climbState = GetClimbState();
        // If they jump while not hanging or can no longer climb
        if (
            (stateMachine.InputController.JumpDown && !CanHang(climbState)) || 
             (CantClimb(climbState) && !didVault)
            )
        {
            // Trigger retry delay
            SetDelay(Settings.ClimbingRetryDelay);
            
            SwitchState(stateMachine.FallingState);
            return;
        }

        // Fall when out of stamina
        if (currentStamina <= 0f)
        {
            SetDelay(Settings.ClimbingRetryDelay);
            
            SwitchState(stateMachine.FallingState);
            return;
        }
    }
    
    
    private ClimbStartData climbStartData;
    
    public ClimbDirections GetClimbState()
    {
        Transform playerTransform = stateMachine.PlayerTransform;
        Vector3 bottomOrigin = playerTransform.position + Vector3.up * 0.1f;
        Vector3 topOrigin = playerTransform.position + Vector3.up * (stateMachine.PlayerHeight - 0.1f);
        Vector3 midOrigin = (bottomOrigin + topOrigin) / 2f;
        Vector3 direction = playerTransform.forward;
        Vector3 sideDirection = playerTransform.right * stateMachine.PlayerRadius;
        // Create rays for each direction
        Ray bottomRay = new Ray(bottomOrigin, direction);
        Ray topRay = new Ray(topOrigin, direction);
        Ray leftRay = new Ray(midOrigin - sideDirection, direction);
        Ray rightRay = new Ray(midOrigin + sideDirection, direction);
        
        ClimbDirections climbDirections = ClimbDirections.None;
        
        if (Physics.Raycast(bottomRay, out var hitInfo, Settings.ClimbRange, Settings.ClimbableLayer))
        {
            Vector3 normal = hitInfo.normal;
            // Calculate angle between normal and ray direction
            float angle = Vector3.SignedAngle(-normal, direction, Vector3.up);
            if (angle < Settings.ClimbingAngleLimits.x || angle > Settings.ClimbingAngleLimits.y)
                return climbDirections;
            
            // // Make sure input is pressed
            // if (!stateMachine.InputController.IsClimbing)
            //     return climbDirections;
            
            // Assume can climb in all directions initially
            climbDirections = ClimbDirections.Up | ClimbDirections.Down | ClimbDirections.Left | ClimbDirections.Right;
            
            // If some ray don't hit, remove from result
            if (!Physics.Raycast(topRay, Settings.ClimbRange, Settings.ClimbableLayer))
            {
                climbDirections &= ~ClimbDirections.Up;
            }
            if (!Physics.Raycast(leftRay, Settings.ClimbRange, Settings.ClimbableLayer))
            {
                climbDirections &= ~ClimbDirections.Left;
            }
            if (!Physics.Raycast(rightRay, Settings.ClimbRange, Settings.ClimbableLayer))
            {
                climbDirections &= ~ClimbDirections.Right;
            }
            
            // Set the start data if they aren't currently climbing
            if (stateMachine.CurrentState != this)
                climbStartData = new ClimbStartData(hitInfo, playerTransform.position);

        }

        return climbDirections;
    }
    
    private bool CantClimb(ClimbDirections climbDirections) => climbDirections == ClimbDirections.None;
    public bool CantClimb() => CantClimb(GetClimbState());
    private bool CanClimb(ClimbDirections climbDirections) => climbDirections.HasFlag(ClimbDirections.Up) || 
                                                             climbDirections.HasFlag(ClimbDirections.Down) || 
                                                             climbDirections.HasFlag(ClimbDirections.Left) || 
                                                             climbDirections.HasFlag(ClimbDirections.Right);
    public bool CanClimb() => CanClimb(GetClimbState());
    private bool CanHang(ClimbDirections climbDirections) => CanClimb(climbDirections) && !climbDirections.HasFlag(ClimbDirections.Up);
    public bool CanHang() => CanHang(GetClimbState());
    [Flags]
    public enum ClimbDirections
    {
        None = 0,
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8
    }
    
    private struct ClimbStartData
    {
        public bool IsValid;
        public RaycastHit RaycastHit;
        public Vector3 PlayerPosition;
        
        public ClimbStartData(RaycastHit rayHit, Vector3 playerPosition)
        {
            this.IsValid = true;
            this.RaycastHit = rayHit;
            this.PlayerPosition = playerPosition;
        }
    }
}
