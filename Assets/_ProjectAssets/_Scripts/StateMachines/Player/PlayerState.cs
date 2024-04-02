using UnityEngine;

public class PlayerState : BaseState<PlayerStateMachine.PlayerState>
{
    protected PlayerContext Context;
    protected PlayerStateMachine.PlayerState NextState;
    public PlayerState(PlayerContext context, PlayerStateMachine.PlayerState key) : base(key)
    {
        Context = context;
    }

    public override void EnterState()
    {
        NextState = StateKey;
        //Debug.Log("Enter new state: " + StateKey.ToString());
    }

    public override void ExitState()
    {

    }

    public override PlayerStateMachine.PlayerState GetNextState()
    {
        return NextState;
    }

    public override void OnTriggerEnter(Collider other)
    {

    }

    public override void OnTriggerExit(Collider other)
    {

    }

    public override void OnTriggerStay(Collider other)
    {

    }

    public override void UpdateState()
    {
        CheckSwitchState();
        HandleAnimation();
        HandleGravity();
    }

    public override void CheckSwitchState()
    {
    }

    public override void UpdateStatePhysic()
    {
        // Disable character rotation since it messed with the animations
        HandleRotation();
    }

    public virtual void HandleGravity() { }

    public virtual void HandleAnimation() { }

    public virtual void HandleRotation()
    {
        if (Context.inputManager.CurrentLookInput != Vector2.zero)
        {
            Context.animator.gameObject.transform.Rotate(Vector3.up, Context.inputManager.CurrentLookInput.x * Context.mouseSensitivity * Time.deltaTime);
        }
    }

    public virtual void SetAnimatorDirectionParameters(float threshold)
    {
        // Setting values
        float currentHorizontal = Context.animator.GetFloat(Context.animatorHorizontalHash);
        float currentVertical = Context.animator.GetFloat(Context.animatorVerticalHash);
        float targetHorizontal = Context.inputManager.CurrentMovementInput.x * threshold;
        float targetVertical = Context.inputManager.CurrentMovementInput.y * threshold;

        // Push current values to target values if close enough, to prevent glitching
        if (Mathf.Abs(currentHorizontal - targetHorizontal) < 0.1f)
        {
            currentHorizontal = targetHorizontal;
        }
        if (Mathf.Abs(currentVertical - targetVertical) < 0.1f)
        {
            currentVertical = targetVertical;
        }

        // Horizontal lerping
        if (currentHorizontal < targetHorizontal)
        {
            currentHorizontal += Context.animateSmoothingSpeed * Time.deltaTime;
        }
        else if (currentHorizontal > targetHorizontal)
        {
            currentHorizontal -= Context.animateSmoothingSpeed * Time.deltaTime;
        }
        // Vertical lerping
        if (currentVertical < targetVertical)
        {
            currentVertical += Context.animateSmoothingSpeed * Time.deltaTime;
        }
        else if (currentVertical > targetVertical)
        {
            currentVertical -= Context.animateSmoothingSpeed * Time.deltaTime;
        }

        // Assign new values to animator's parameters
        Context.animator.SetFloat(Context.animatorHorizontalHash, currentHorizontal);
        Context.animator.SetFloat(Context.animatorVerticalHash, currentVertical);
    }
}
