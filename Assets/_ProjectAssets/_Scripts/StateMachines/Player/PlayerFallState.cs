using UnityEngine;

public class PlayerFallState : PlayerState
{
    public PlayerFallState(PlayerContext context, PlayerStateMachine.PlayerState key) : base(context, key)
    {
    }

    public override void CheckSwitchState()
    {
        //If character landed and still running, switch to run state
        if (Context.inputManager.IsMovementPressed && Context.inputManager.IsRunPressed && Context.characterController.isGrounded)
        {
            NextState = PlayerStateMachine.PlayerState.Run;
        }
        //If character landed and still walking, switch to run state
        else if (Context.inputManager.IsMovementPressed && !Context.inputManager.IsRunPressed && Context.characterController.isGrounded)
        {
            NextState = PlayerStateMachine.PlayerState.Walk;
        }
        //If character landed and no movement, switch to idle state
        else if(Context.characterController.isGrounded && !Context.inputManager.IsMovementPressed && !Context.inputManager.IsRunPressed)
        {
            NextState = PlayerStateMachine.PlayerState.Idle;
        }
    }

    public override void HandleGravity()
    {
        float previousYVelocity = Context.inputManager.CurrentMovement.y;
        float newYVelocity = Context.inputManager.CurrentMovement.y + (Context.airborneGravity * Time.deltaTime * Context.fallingMultiplier);
        // Optional, can set the maximum amount of falling gravity using Mathf.Max
        float nextYVelocity = Mathf.Max((previousYVelocity + newYVelocity) * 0.5f, -20.0f);

        Vector3 newMovement = Context.inputManager.CurrentMovement;
        newMovement.y = nextYVelocity;
        Context.inputManager.CurrentMovement = newMovement;
    }

    public override void UpdateStatePhysic()
    {
        base.UpdateStatePhysic();
        // If fall while running
        if (Context.inputManager.IsMovementPressed && Context.inputManager.IsRunPressed && Context.isJumping)
        {
            Vector3 newMovement = Context.inputManager.CurrentMovement;
            newMovement.x = Context.inputManager.CurrentMovementInput.x * Context.runSpeed;
            newMovement.z = Context.inputManager.CurrentMovementInput.y * Context.runSpeed;
            Context.inputManager.CurrentMovement = newMovement;
        }
        // If fall while walking
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

    public override void HandleAnimation()
    {
        if(Context.inputManager.CurrentMovement.y < -12.0f)
        {
            Context.animator.SetBool(Context.animatorFallHash, true);
        }
    }

    public override void ExitState()
    {
        base.ExitState();
        Context.animator.SetBool(Context.animatorFallHash, false);
    }

    public override void EnterState()
    {
        base.EnterState();
        Vector3 newMovement = Context.inputManager.CurrentMovement;
        newMovement.y = -9.8f;
        Context.inputManager.CurrentMovement = newMovement;
    }
}
