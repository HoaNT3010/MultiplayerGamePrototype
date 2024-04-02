using UnityEngine;

public class PlayerStateMachine : StateManager<PlayerStateMachine.PlayerState>
{
    public enum PlayerState
    {
        Idle,
        Walk,
        Run,
        Jump,
        Fall,
    }

    [Header("Player Data")]
    [SerializeField] private PlayerContext playerContext;

    private CharacterController characterController;
    private Animator animator;
    private CharacterInputController inputManager;

    //void Awake()
    //{
    //    GetComponents();
    //    InitializeStates();
    //}

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        GetComponents();
        InitializeStates();
    }

    private void GetComponents()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        inputManager = GetComponent<CharacterInputController>();

        // Assign components to player context
        playerContext = ScriptableObject.CreateInstance<PlayerContext>();
        playerContext.characterController = characterController;
        playerContext.animator = animator;
        playerContext.inputManager = inputManager;
    }

    private void InitializeStates()
    {
        States.Add(PlayerState.Idle, new PlayerIdleState(playerContext, PlayerState.Idle));
        States.Add(PlayerState.Walk, new PlayerWalkState(playerContext, PlayerState.Walk));
        States.Add(PlayerState.Run, new PlayerRunState(playerContext, PlayerState.Run));
        States.Add(PlayerState.Jump, new PlayerJumpState(playerContext, PlayerState.Jump));
        States.Add(PlayerState.Fall, new PlayerFallState(playerContext, PlayerState.Fall));

        CurrentState = States[PlayerState.Idle];
    }
}
