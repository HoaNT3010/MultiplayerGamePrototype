using UnityEngine;

public class PlayerIdleState : PlayerState
{

    public PlayerIdleState(PlayerContext context, PlayerStateMachine.PlayerState key) : base(context, key)
    {
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
        Context.characterController.Move(Context.inputManager.CurrentMovement * Time.deltaTime);
    }

    public override void CheckSwitchState()
    {
        // if jump is pressed and grounded, switch to jump state
        if (Context.inputManager.IsJumpPressed && Context.characterController.isGrounded)
        {
            NextState = PlayerStateMachine.PlayerState.Jump;
        }
        // if movement and run is pressed, switch to run state
        else if (Context.inputManager.IsRunPressed && Context.inputManager.IsMovementPressed)
        {
            NextState = PlayerStateMachine.PlayerState.Run;
        }
        // if only movement is pressed, switch to walk state
        else if (Context.inputManager.IsMovementPressed)
        {
            NextState = PlayerStateMachine.PlayerState.Walk;
        }
        else if(!Context.inputManager.IsJumpPressed && !Context.characterController.isGrounded) 
        {
            NextState = PlayerStateMachine.PlayerState.Fall;
        }
    }

    public override void HandleAnimation()
    {
        SetAnimatorDirectionParameters(Context.animationIdleThreshold);
    }
}
