using UnityEngine;

public class PlayerJumpState : PlayerState
{
    public PlayerJumpState(PlayerContext context, PlayerStateMachine.PlayerState key) : base(context, key)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        SetupJumpVariables();
    }

    public override void UpdateState()
    {
        HandleAnimation();
        HandleGravity();
        HandleJump();
    }

    public override void UpdateStatePhysic()
    {
        base.UpdateStatePhysic();
        // If jump while running
        if (Context.inputManager.IsMovementPressed && Context.inputManager.IsRunPressed && Context.isJumping)
        {
            Vector3 newMovement = Context.inputManager.CurrentMovement;
            newMovement.x = Context.inputManager.CurrentMovementInput.x * Context.runSpeed;
            newMovement.z = Context.inputManager.CurrentMovementInput.y * Context.runSpeed;
            Context.inputManager.CurrentMovement = newMovement;
        }
        // If jump while walking
        else if (Context.inputManager.IsMovementPressed && !Context.inputManager.IsRunPressed && Context.isJumping)
        {
            Vector3 newMovement = Context.inputManager.CurrentMovement;
            newMovement.x = Context.inputManager.CurrentMovementInput.x * Context.walkSpeed;
            newMovement.z = Context.inputManager.CurrentMovementInput.y * Context.walkSpeed;
            Context.inputManager.CurrentMovement = newMovement;
        }
        Vector3 moveDirection = Context.animator.gameObject.transform.TransformDirection(Context.inputManager.CurrentMovement);
        Context.characterController.Move(moveDirection * Time.deltaTime);
    }

    public override void CheckSwitchState()
    {
        //If character landed and still running, switch to run state
        if (Context.inputManager.IsMovementPressed && Context.inputManager.IsRunPressed)
        {
            NextState = PlayerStateMachine.PlayerState.Run;
        }
        //If character landed and still walking, switch to run state
        else if (Context.inputManager.IsMovementPressed && !Context.inputManager.IsRunPressed)
        {
            NextState = PlayerStateMachine.PlayerState.Walk;
        }
        //If character landed and no movement, switch to idle state
        else
        {
            NextState = PlayerStateMachine.PlayerState.Idle;
        }
    }

    public override void HandleGravity()
    {
        // Increase the gravity force when falling and allow player to shorten the jump
        bool isFalling = Context.inputManager.CurrentMovement.y <= 0.0f || !Context.inputManager.IsJumpPressed;
        if (isFalling)
        {
            float previousYVelocity = Context.inputManager.CurrentMovement.y;
            float newYVelocity = Context.inputManager.CurrentMovement.y + (Context.airborneGravity * Time.deltaTime * Context.fallingMultiplier);
            // Optional, can set the maximum amount of falling gravity using Mathf.Max
            float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f;

            Vector3 newMovement = Context.inputManager.CurrentMovement;
            newMovement.y = nextYVelocity;
            Context.inputManager.CurrentMovement = newMovement;
        }
        else
        {
            float previousYVelocity = Context.inputManager.CurrentMovement.y;
            float newYVelocity = Context.inputManager.CurrentMovement.y + (Context.airborneGravity * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f;

            Vector3 newMovement = Context.inputManager.CurrentMovement;
            newMovement.y = nextYVelocity;
            Context.inputManager.CurrentMovement = newMovement;
        }
    }

    private void HandleJump()
    {
        if (!Context.isJumping && Context.characterController.isGrounded && Context.inputManager.IsJumpPressed)
        {
            Context.isJumping = true;
            Vector3 newMovement = Context.inputManager.CurrentMovement;
            newMovement.y = Context.initialJumpVelocity * 0.5f;
            Context.inputManager.CurrentMovement = newMovement;
        }
        else if (!Context.inputManager.IsJumpPressed && Context.isJumping && Context.characterController.isGrounded)
        {
            Context.isJumping = false;
            // When landed, switch to other states
            Context.animator.SetBool(Context.animatorJumpHash, false);
            CheckSwitchState();
        }
    }

    private void SetupJumpVariables()
    {
        float timeToApex = Context.maxJumpTime / 2;
        Context.airborneGravity = (-2 * Context.maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        Context.initialJumpVelocity = (2 * Context.maxJumpHeight) / timeToApex;
    }

    public override void HandleAnimation()
    {
        Context.animator.SetBool(Context.animatorJumpHash, true);
    }
}
