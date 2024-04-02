using UnityEngine;

public class PlayerRunState : PlayerState
{
    public PlayerRunState(PlayerContext context, PlayerStateMachine.PlayerState key) : base(context, key)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        Vector3 newMovement = new Vector3(Context.inputManager.CurrentMovementInput.x, Context.groundedGravity, Context.inputManager.CurrentMovementInput.y);
        Context.inputManager.CurrentMovement = newMovement;
    }

    public override void HandleGravity()
    {
        Vector3 newMovement = Context.inputManager.CurrentMovement;
        newMovement.y = Context.groundedGravity;
        Context.inputManager.CurrentMovement = newMovement;
    }

    public override void UpdateStatePhysic()
    {
        base.UpdateStatePhysic();
        Vector3 moveDirection = Context.animator.gameObject.transform.TransformDirection(Context.inputManager.CurrentMovement);
        Context.characterController.Move(moveDirection * Context.runSpeed * Time.deltaTime);
    }

    public override void CheckSwitchState()
    {
        // if jump is pressed and grounded, switch to jump state
        if (Context.inputManager.IsJumpPressed && Context.characterController.isGrounded)
        {
            NextState = PlayerStateMachine.PlayerState.Jump;
        }
        // if movement is pressed but run is released, switch to walk state
        else if (!Context.inputManager.IsRunPressed && Context.inputManager.IsMovementPressed)
        {
            NextState = PlayerStateMachine.PlayerState.Walk;
        }
        // if movement is released, switch to idle state
        else if (!Context.inputManager.IsMovementPressed)
        {
            NextState = PlayerStateMachine.PlayerState.Idle;
        }
        else if (!Context.inputManager.IsJumpPressed && !Context.characterController.isGrounded)
        {
            NextState = PlayerStateMachine.PlayerState.Fall;
        }
    }

    public override void HandleAnimation()
    {
        SetAnimatorDirectionParameters(Context.animationRunThreshold);
    }
}
