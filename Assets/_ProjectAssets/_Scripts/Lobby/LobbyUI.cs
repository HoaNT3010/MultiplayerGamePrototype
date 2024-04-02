using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public event EventHandler<LobbyManager.LobbyEventArgs> PlayerLeaveLobby;
    public class KickPlayerEventArgs : EventArgs
    {
        public string playerId;
    }
    public event EventHandler<KickPlayerEventArgs> KickPlayerFromLobby;
    public event EventHandler StartGameClick;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI lobbyName;
    [SerializeField] private TextMeshProUGUI lobbyPlayerCounter;
    [SerializeField] private GameObject playerContainer;
    [SerializeField] private GameObject playerItemTemplate;
    [SerializeField] private GameObject startGameButton;

    private List<Player> currentPlayers = new List<Player>();

    private void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        SubscribeToEvent();
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvent();
    }

    private void SubscribeToEvent()
    {
        LobbyManager.Instance.OnJoinedLobby += LobbyManager_OnJoinedLobby;
        LobbyManager.Instance.OnJoinedLobbyUpdate += LobbyManager_OnJoinedLobbyUpdate;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnKickedFromLobby;
    }

    private void UnsubscribeFromEvent()
    {
        LobbyManager.Instance.OnJoinedLobby -= LobbyManager_OnJoinedLobby;
        LobbyManager.Instance.OnJoinedLobbyUpdate -= LobbyManager_OnJoinedLobbyUpdate;
        LobbyManager.Instance.OnKickedFromLobby -= LobbyManager_OnKickedFromLobby;
    }

    private void LobbyManager_OnKickedFromLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        gameObject.SetActive(false);
    }

    private void LobbyManager_OnJoinedLobbyUpdate(object sender, LobbyManager.LobbyEventArgs e)
    {
        lobbyPlayerCounter.text = e.lobby.Players.Count + "/" + e.lobby.MaxPlayers;
        UpdateLobbyPlayers(e.lobby);
    }

    private void LobbyManager_OnJoinedLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        lobbyName.text = e.lobby.Name;
        lobbyPlayerCounter.text = e.lobby.Players.Count + "/" + e.lobby.MaxPlayers;
        UpdateLobbyPlayers(e.lobby);

        gameObject.SetActive(true);
    }

    private void UpdateLobbyPlayers(Lobby lobby)
    {
        try
        {
            if (lobby != null)
            {
                if (LobbyManager.Instance.IsLobbyHost())
                {
                    startGameButton.SetActive(true);
                }
                else
                {
                    startGameButton.SetActive(false);
                }

                // Destroy existing player item
                if (playerContainer.transform.childCount > 0)
                {
                    currentPlayers.Clear();
                    while (playerContainer.transform.childCount > 0)
                    {
                        DestroyImmediate(playerContainer.transform.GetChild(0).gameObject);
                    }
                }
                GameObject newPlayersItem;

                for (int i = 0; i < lobby.Players.Count; i++)
                {
                    newPlayersItem = Instantiate(playerItemTemplate, playerContainer.transform);
                    newPlayersItem.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = lobby.Players[i].Data["PlayerName"]?.Value;
                    if (lobby.HostId == lobby.Players[i].Id)
                    {
                        newPlayersItem.transform.GetChild(1).gameObject.SetActive(true);
                    }
                    else if (lobby.HostId == LobbyManager.Instance.GetAuthenticatedPlayerId())
                    {
                        // Click button
                        newPlayersItem.transform.GetChild(2).GetComponent<Button>().AddEventListener(lobby.Players[i].Id, KickPlayer);
                        newPlayersItem.transform.GetChild(2).gameObject.SetActive(true);
                    }

                    currentPlayers.Add(lobby.Players[i]);
                }
            }

        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }
    }

    public void LeaveLobbyClick()
    {
        PlayerLeaveLobby?.Invoke(this, new LobbyManager.LobbyEventArgs { lobby = LobbyManager.Instance.GetJoinedLobby() });
        gameObject.SetActive(false);
    }

    private void KickPlayer(string playerId)
    {
        KickPlayerFromLobby?.Invoke(this, new KickPlayerEventArgs { playerId = playerId });
    }

    public void StartGameButtonClick()
    {
        StartGameClick?.Invoke(this, new EventArgs());
        gameObject.SetActive(false);
    }
}
