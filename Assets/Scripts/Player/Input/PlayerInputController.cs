using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    
    public Vector2 FrameMove { get; private set; }
    public Vector2 FrameLook { get; private set; }
    public bool JumpDown { get; private set; }
    public bool JumpHeld { get; private set; }
    private bool previousJumpHeld;

    public bool IsSprinting { get; private set; }
    public bool IsCrouching { get; private set; }
    
    private void LateUpdate()
    {
        previousJumpHeld = JumpHeld;

        JumpDown = false;
    }
    
    private void OnMove(InputValue inputValue)
    {
        FrameMove = inputValue.Get<Vector2>();
    }
    
    private void OnJump(InputValue inputValue)
    {
        bool isPressed = inputValue.isPressed;

        JumpDown = isPressed && !previousJumpHeld;
        JumpHeld = isPressed;
    }
    
    private void OnSprint(InputValue inputValue)
    {
        IsSprinting = inputValue.isPressed;
    }
    
    private void OnCrouch(InputValue inputValue)
    {
        IsCrouching = inputValue.isPressed;
    }
    
    private void OnLook(InputValue inputValue)
    {
        FrameLook = inputValue.Get<Vector2>();
    }
    
}
