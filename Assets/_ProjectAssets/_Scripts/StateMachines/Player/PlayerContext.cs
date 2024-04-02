using UnityEngine;

[CreateAssetMenu(fileName = "Player Context", menuName = "Player/New Player Context")]
public class PlayerContext : ScriptableObject
{
    public float idleSpeed = 1.0f;
    public float walkSpeed = 2.0f;
    public float runSpeed = 4.0f;

    public bool isJumping = false;
    public float initialJumpVelocity;
    public float maxJumpHeight = 4.0f;
    public float maxJumpTime = 0.85f;
    public float groundedGravity = -0.05f;
    public float airborneGravity = -9.8f;
    public float fallingMultiplier = 2.0f;

    public float mouseSensitivity = 3.0f;
    public float rotationFactorPerFrame = 0.2f;

    public float animationIdleThreshold = 0f;
    public float animationWalkThreshold = 0.3f;
    public float animationRunThreshold = 1.0f;
    public float animateSmoothingSpeed = 2.0f;
    public readonly int animatorHorizontalHash = Animator.StringToHash("horizontal");
    public readonly int animatorVerticalHash = Animator.StringToHash("vertical");
    public readonly int animatorJumpHash = Animator.StringToHash("jump");
    public readonly int animatorFallHash = Animator.StringToHash("fall");

    public CharacterController characterController;
    public Animator animator;
    public CharacterInputController inputManager;

}
