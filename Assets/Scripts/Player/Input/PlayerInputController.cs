using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    
    public Vector2 FrameMove { get; private set; }
    public Vector2 FrameLook { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsClimbing { get; private set; }
    public bool IsCrouching { get; private set; }
    public bool IsAiming { get; private set; }
    public bool IsShooting { get; private set; }
    public Action OnShootAction;
    public bool IsReloading { get; private set; }
    public Action OnReloadAction;

    private void Awake()
    {
        LoadInputActions();
    }
    
    private void LoadInputActions()
    {
        // Enable all action maps
        foreach (var map in inputActions.actionMaps)
        {
            map.Enable();
        }
    }

    private void OnMove(InputValue inputValue)
    {
        FrameMove = inputValue.Get<Vector2>();
    }
    
    private void OnJump(InputValue inputValue)
    {
        IsJumping = inputValue.isPressed;
    }
    
    private void OnSprint(InputValue inputValue)
    {
        IsSprinting = inputValue.isPressed;
    }
    
    private void OnClimb(InputValue inputValue)
    {
        IsClimbing = inputValue.isPressed;
    }
    
    private void OnCrouch(InputValue inputValue)
    {
        IsCrouching = inputValue.isPressed;
    }
    
    private void OnLook(InputValue inputValue)
    {
        FrameLook = inputValue.Get<Vector2>();
    }
    
    private void OnAim(InputValue inputValue)
    {
        IsAiming = inputValue.isPressed;
    }
    
    private void OnShoot(InputValue inputValue)
    {
        IsShooting = inputValue.isPressed;
        
        if(IsShooting && OnShootAction != null)
            OnShootAction.Invoke();
    }
    
    private void OnReload(InputValue inputValue)
    {
        IsReloading = inputValue.isPressed;
        
        if(IsReloading && OnReloadAction != null)
            OnReloadAction.Invoke();
    }
}
