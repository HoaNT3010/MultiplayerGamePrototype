using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    public class LobbyEventArgs : EventArgs
    {
        public Lobby lobby;
    }

    public class LobbyListEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    public event EventHandler OnGameStarted;
    public event EventHandler OnAuthenticated;
    public event EventHandler<LobbyEventArgs> OnJoinedLobby;
    public event EventHandler<LobbyEventArgs> OnKickedFromLobby;
    public event EventHandler<LobbyEventArgs> OnJoinedLobbyUpdate;
    public event EventHandler<LobbyListEventArgs> OnLobbyListChanged;

    [Header("Authenticate UI")]
    [SerializeField] private GameObject authenticatePanel;
    [SerializeField] private GameObject nameChangePanel;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TMP_InputField playerNameInput;

    private string playerName;
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float heartbeatTimerMax = 15.0f;
    private float pollingTimer;
    private float pollingTimerMax = 1.1f;
    private bool isAuthenticated = false;


    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        SubscribeToEvents();
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;

        playerName = "PlayerName";
        playerNameText.text = playerName;
        if(!isAuthenticated)
        {
            authenticatePanel.SetActive(true);
        }
        else
        {
            authenticatePanel.SetActive(false);
        }
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void LobbyUI_StartGameClick(object sender, EventArgs e)
    {
        if (IsLobbyHost())
        {
            if(IsPlayerHostMode())
            {
                StartGameHostMode();
            }
            else
            {
                StartGameHostMode();
            }
        }
    }

    private async void StartGameHostMode()
    {
        try
        {
            Debug.Log("Start game with player host mode");
            string relayCode = await RelayManager.Instance.CreateRelay();
            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                    {
                        { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                    }
            });
            joinedLobby = null;
            hostLobby = null;
            OnGameStarted?.Invoke(this, EventArgs.Empty);
            // When lobby host started the game, lobby is deleted?
            SwitchToPlaygroundSceneHost();
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private void SwitchToPlaygroundSceneHost()
    {
        SceneManager.LoadSceneAsync(1, LoadSceneMode.Single).completed += LobbyManager_HostCompleted;
    }

    private void LobbyManager_HostCompleted(AsyncOperation obj)
    {
        NetworkManager.Singleton.StartHost();
    }

    private async void LobbyUI_KickPlayerFromLobby(object sender, LobbyUI.KickPlayerEventArgs e)
    {
        if (IsLobbyHost())
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, e.playerId);
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogWarning("Failed to kick player from lobby");
                Debug.Log(ex);
            }
        }
    }

    private async void LobbyUI_PlayerLeaveLobby(object sender, LobbyEventArgs e)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(e.lobby.Id, GetAuthenticatedPlayerId());
            joinedLobby = null;
            if (e.lobby.HostId == GetAuthenticatedPlayerId())
            {
                hostLobby = null;
            }
            RefreshLobbies();
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogWarning("Failed to remove player from lobby");
            Debug.Log(ex);
        }

    }

    private void LobbyListUI_OnClickLobbyItem(object sender, LobbyListUI.ClickLobbyEventArgs e)
    {
        JoinLobbyId(e.lobbyId);
    }

    private void Update()
    {
        if (hostLobby != null)
        {
            HandleLobbyHeartbeat();
        }
        if(joinedLobby != null)
        {
            HandleLobbyPolling();
        }
        if(Input.GetKeyDown(KeyCode.M))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public string GetPlayerName()
    {
        return playerName;
    }

    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                heartbeatTimer = heartbeatTimerMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void HandleLobbyPolling()
    {
        try
        {
            if (joinedLobby != null)
            {
                pollingTimer -= Time.deltaTime;
                if (pollingTimer < 0f)
                {
                    pollingTimer = pollingTimerMax;
                    joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                    OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });

                    // Mitigate host lobby
                    if (hostLobby != null && joinedLobby.HostId == GetAuthenticatedPlayerId())
                    {
                        hostLobby = joinedLobby;
                    }

                    if (!CheckPlayerInLobby())
                    {
                        // Player is kicked out of lobby!
                        Debug.Log("Player is kicked out of lobby!");
                        OnKickedFromLobby?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
                        joinedLobby = null;
                    }

                    // Start game!
                    if (joinedLobby.Data["RelayCode"].Value != "0")
                    {
                        if (!IsLobbyHost())
                        {
                            RelayManager.Instance.JoinRelay(joinedLobby.Data["RelayCode"].Value);
                        }
                    }
                }
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning("Failed to poll for lobby refresh");
            Debug.Log(e);
        }
    }

    private void RelayManager_ClientRelayDataSet(object sender, EventArgs e)
    {
        SceneManager.LoadSceneAsync(1, LoadSceneMode.Single).completed += LobbyManager_ClientCompleted;
        joinedLobby = null;
    }

    private void LobbyManager_ClientCompleted(AsyncOperation obj)
    {
        NetworkManager.Singleton.StartClient();
    }

    private bool CheckPlayerInLobby()
    {
        if (joinedLobby != null && joinedLobby.Players != null)
        {
            foreach (var player in joinedLobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private async void AuthenticatePlayer()
    {
        InitializationOptions options = new InitializationOptions();
        options.SetProfile(playerName);
        await UnityServices.InitializeAsync(options);
        AuthenticationService.Instance.SignedIn += () =>
        {
            isAuthenticated = true;
            RefreshLobbies();
            authenticatePanel.SetActive(false);
            Debug.Log("Authenticated with player name: " + AuthenticationService.Instance.Profile);
            OnAuthenticated?.Invoke(this, new EventArgs());
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public void AuthenticateClick()
    {
        AuthenticatePlayer();
    }

    private async void JoinLobbyId(string lobbyId)
    {
        try
        {
            JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
            {
                Player = GetPlayer(),

            };
            Lobby newLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId, options);
            joinedLobby = newLobby;
            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = newLobby });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning("Failed to join lobby");
            Debug.Log(e);
        }
    }

    public async void RefreshLobbies()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = 20,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created),
                }
            };
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(options);
            OnLobbyListChanged?.Invoke(this, new LobbyListEventArgs { lobbyList = queryResponse.Results });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning("Failed to refresh lobbies");
            Debug.Log(e);
        }
    }

    public void OpenNameChangePanel()
    {
        playerNameInput.text = playerName;
        nameChangePanel.SetActive(true);
    }

    public void CancelNameChange()
    {
        nameChangePanel.SetActive(false);
    }

    public void NameChangeFinish()
    {
        // Not Validated
        playerName = playerNameInput.text;
        playerNameText.text = playerName;
        nameChangePanel.SetActive(false);
    }

    private void LobbyListUI_OnCreateLobby(object sender, LobbyListUI.CreateLobbyEventArgs e)
    {
        CreatingLobby(e);
    }

    public async void CreatingLobby(LobbyListUI.CreateLobbyEventArgs newLobbyArgs)
    {
        CreateLobbyOptions options = new CreateLobbyOptions
        {
            IsPrivate = newLobbyArgs.isPrivate,
            Player = GetPlayer(),
            Data = new Dictionary<string, DataObject>
            {
                {"HostMode", new DataObject(DataObject.VisibilityOptions.Public, newLobbyArgs.hostMode, DataObject.IndexOptions.S1)},
                {"RelayCode", new DataObject(DataObject.VisibilityOptions.Member, "0") }
            }
        };
        try
        {

            Lobby newLobby = await LobbyService.Instance.CreateLobbyAsync(newLobbyArgs.lobbyName, newLobbyArgs.maxPlayers, options);
            hostLobby = newLobby;
            joinedLobby = newLobby;
            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = newLobby });
            Debug.Log("New lobby created: " + newLobby.Name + " - " + newLobby.MaxPlayers + " - " + newLobby.Id + " - " + newLobby.LobbyCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning("Failed to create new lobby");
            Debug.Log(e);
        }
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            }
        };
    }

    public string GetAuthenticatedPlayerId()
    {
        return AuthenticationService.Instance.PlayerId;
    }

    public Lobby GetJoinedLobby()
    {
        return joinedLobby;
    }

    public bool IsLobbyHost()
    {
        return joinedLobby.HostId == GetAuthenticatedPlayerId();
    }

    private bool IsPlayerHostMode()
    {
        return joinedLobby.Data["HostMode"].Value == "Host";
    }

    public bool IsPlayerAuthenticated() 
    {
        return isAuthenticated;
    }

    private void SubscribeToEvents()
    {
        LobbyListUI lobbyListUI = FindObjectOfType<LobbyListUI>(true);
        if(lobbyListUI == null)
        {
            Debug.Log("LobbyListUI object is null. Cannot subscribe to it's events");
        }
        else
        {
            lobbyListUI.OnCreateLobby += LobbyListUI_OnCreateLobby;
            lobbyListUI.OnClickLobbyItem += LobbyListUI_OnClickLobbyItem;
            Debug.Log("LobbyListUI's events subscribed");
        }

        LobbyUI lobbyUI = FindObjectOfType<LobbyUI>(true);
        if(lobbyUI == null)
        {
            Debug.Log("LobbyUI object is null. Cannot subscribe to it's events");
        }
        else
        {
            lobbyUI.PlayerLeaveLobby += LobbyUI_PlayerLeaveLobby;
            lobbyUI.KickPlayerFromLobby += LobbyUI_KickPlayerFromLobby;
            lobbyUI.StartGameClick += LobbyUI_StartGameClick;
            Debug.Log("LobbyUI's events subscribed");
        }

        RelayManager.Instance.ClientRelayDataSet += RelayManager_ClientRelayDataSet;
    }

    private void UnsubscribeFromEvents()
    {
        LobbyListUI lobbyListUI = FindObjectOfType<LobbyListUI>();
        if (lobbyListUI != null)
        {
            lobbyListUI.OnCreateLobby -= LobbyListUI_OnCreateLobby;
            lobbyListUI.OnClickLobbyItem -= LobbyListUI_OnClickLobbyItem;
        }

        LobbyUI lobbyUI = FindObjectOfType<LobbyUI>();
        if (lobbyUI != null)
        {
            lobbyUI.PlayerLeaveLobby -= LobbyUI_PlayerLeaveLobby;
            lobbyUI.KickPlayerFromLobby -= LobbyUI_KickPlayerFromLobby;
            lobbyUI.StartGameClick -= LobbyUI_StartGameClick;
        }

        RelayManager.Instance.ClientRelayDataSet -= RelayManager_ClientRelayDataSet;
    }
}
