using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterInputController : NetworkBehaviour
{
    private PlayerInputActions playerInput;

    private Vector2 currentMovementInput;
    private Vector3 currentMovement;
    private Vector2 currentLookInput;
    private bool isMovementPressed;
    private bool isRunPressed;
    private bool isJumpPressed = false;

    public Vector2 CurrentMovementInput { get => currentMovementInput; set => currentMovementInput = value; }
    public Vector3 CurrentMovement { get => currentMovement; set => currentMovement = value; }
    public bool IsMovementPressed { get => isMovementPressed; set => isMovementPressed = value; }
    public bool IsRunPressed { get => isRunPressed; set => isRunPressed = value; }
    public bool IsJumpPressed { get => isJumpPressed; set => isJumpPressed = value; }
    public Vector2 CurrentLookInput { get => currentLookInput; set => currentLookInput = value; }

    private void Awake()
    {
        playerInput = new PlayerInputActions();
    }

    private void OnEnable()
    {
        playerInput.Enable();
        ListenInputEvent();
    }

    private void OnDisable()
    {
        playerInput.Disable();
    }

    private void ListenInputEvent()
    {
        // Movement
        playerInput.Player.Movement.started += OnMovementInput;
        playerInput.Player.Movement.performed += OnMovementInput;
        playerInput.Player.Movement.canceled += OnMovementInput;

        // Run
        playerInput.Player.Run.started += OnRunInput;
        playerInput.Player.Run.canceled += OnRunInput;

        // Jump
        playerInput.Player.Jump.started += OnJumpInput;
        playerInput.Player.Jump.canceled += OnJumpInput;

        // Look
        playerInput.Player.Look.started += OnLookInput;
        playerInput.Player.Look.performed += OnLookInput;
        playerInput.Player.Look.canceled += OnLookInput;
    }

    void OnMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    void OnLookInput(InputAction.CallbackContext context)
    {
        currentLookInput = context.ReadValue<Vector2>();
    }

    void OnRunInput(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }

    void OnJumpInput(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
    }

    private void OnApplicationFocus(bool focus)
    {
        SetCursorState(focus);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
