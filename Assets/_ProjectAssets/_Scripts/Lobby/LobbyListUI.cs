using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using static LobbyManager;

public static class ButtonExtension
{
    public static void AddEventListener<T>(this Button button, T param, Action<T> OnClick)
    {
        button.onClick.AddListener(() =>
        {
            OnClick(param);
        });
    }
}

public class LobbyListUI : MonoBehaviour
{
    public class CreateLobbyEventArgs : EventArgs
    {
        public string lobbyName;
        public int maxPlayers;
        public bool isPrivate;
        public string hostMode;
    }

    public class ClickLobbyEventArgs : EventArgs
    {
        public string lobbyId;
    }

    public event EventHandler<CreateLobbyEventArgs> OnCreateLobby;
    public event EventHandler<ClickLobbyEventArgs> OnClickLobbyItem;

    [Header("Lobby List UI")]
    [SerializeField] private GameObject lobbyContainer;
    [SerializeField] private GameObject lobbyItemTemplate;
    [SerializeField] private GameObject emptyNotification;
    [Header("Create Lobby UI")]
    [SerializeField] private GameObject createLobbyPanel;
    [SerializeField] private TMP_InputField createLobbyNameField;
    [SerializeField] private TMP_InputField createLobbyMaxPlayersField;
    [SerializeField] private TMP_Dropdown createLobbyVisibilityDropdown;
    [SerializeField] private TMP_Dropdown createLobbyHostModeDropdown;

    private List<Lobby> currentLobbyItems = new List<Lobby>();
    private string createLobbyName;
    private int createLobbyMaxPlayers;
    private string createLobbyVisibility;
    private string createLobbyHostMode;

    private void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        SubscribeToEvent();
        if (LobbyManager.Instance.IsPlayerAuthenticated())
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }

    }

    private void OnDestroy()
    {
        UnsubscribeFromEvent();
    }

    private void SubscribeToEvent()
    {
        LobbyManager.Instance.OnAuthenticated += LobbyManager_OnAuthenticated;
        LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
        LobbyManager.Instance.OnJoinedLobby += LobbyManager_OnJoinedLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnKickedFromLobby;
    }

    private void UnsubscribeFromEvent()
    {
        LobbyManager.Instance.OnAuthenticated -= LobbyManager_OnAuthenticated;
        LobbyManager.Instance.OnLobbyListChanged -= LobbyManager_OnLobbyListChanged;
        LobbyManager.Instance.OnJoinedLobby -= LobbyManager_OnJoinedLobby;
        LobbyManager.Instance.OnKickedFromLobby -= LobbyManager_OnKickedFromLobby;
    }

    private void LobbyManager_OnKickedFromLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        gameObject.SetActive(true);
    }

    private void LobbyUI_PlayerLeaveLobby(object sender, LobbyEventArgs e)
    {
        gameObject.SetActive(true);
    }

    private void LobbyManager_OnJoinedLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        gameObject.SetActive(false);
    }

    private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager.LobbyListEventArgs e)
    {
        UpdateLobbyContainer(e.lobbyList);
    }

    private void LobbyManager_OnAuthenticated(object sender, EventArgs e)
    {
        createLobbyPanel.SetActive(false);
        gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void UpdateLobbyContainer(List<Lobby> lobbies)
    {
        // Destroy existing lobby item
        if (lobbyContainer.transform.childCount > 0)
        {
            currentLobbyItems.Clear();
            while (lobbyContainer.transform.childCount > 0)
            {
                DestroyImmediate(lobbyContainer.transform.GetChild(0).gameObject);
            }
        }
        if (lobbies.Count < 1)
        {
            emptyNotification.SetActive(true);
        }
        else
        {
            emptyNotification.SetActive(false);
            GameObject newLobbyItem;
            for (int i = 0; i < lobbies.Count; i++)
            {
                newLobbyItem = Instantiate(lobbyItemTemplate, lobbyContainer.transform);
                newLobbyItem.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = lobbies[i].Name;
                newLobbyItem.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = lobbies[i].Players.Count + "/" + lobbies[i].MaxPlayers;
                newLobbyItem.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = lobbies[i].LobbyCode;
                DataObject lobbyHostMode = lobbies[i].Data["HostMode"];
                newLobbyItem.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = lobbyHostMode.Value;

                // Extension method for on-click button event
                newLobbyItem.GetComponent<Button>().AddEventListener(i, OnLobbyItemClick);
                currentLobbyItems.Add(lobbies[i]);
            }
        }
    }

    private void OnLobbyItemClick(int itemIndex)
    {
        Debug.Log("Lobby index " + itemIndex + " clicked! Lobby id: " + currentLobbyItems[itemIndex].Id);
        OnClickLobbyItem?.Invoke(this, new ClickLobbyEventArgs { lobbyId = currentLobbyItems[itemIndex].Id });
    }

    public void OpenCreateLobbyDialog()
    {
        SetInitialCreateLobbyValues();
        createLobbyPanel.SetActive(true);
    }

    private void SetInitialCreateLobbyValues()
    {
        createLobbyName = LobbyManager.Instance.GetPlayerName() + "'s lobby";
        createLobbyNameField.text = createLobbyName;
        createLobbyMaxPlayers = 4;
        createLobbyMaxPlayersField.text = createLobbyMaxPlayers.ToString();
        createLobbyVisibility = "Public";
        createLobbyVisibilityDropdown.SetValueWithoutNotify(0);
        createLobbyHostMode = "Host";
        createLobbyHostModeDropdown.SetValueWithoutNotify(0);
    }

    public void CreateLobbyNameChanged()
    {
        createLobbyName = createLobbyNameField.text;
    }

    public void CreateLobbyMaxPlayersChanged()
    {
        createLobbyMaxPlayers = int.Parse(createLobbyMaxPlayersField.text);
    }

    public void CreateLobbyVisibilityChange()
    {
        createLobbyVisibility = createLobbyVisibilityDropdown.options[createLobbyVisibilityDropdown.value].text;
    }

    public void CreateLobbyHostModeChange()
    {
        createLobbyHostMode = createLobbyHostModeDropdown.options[createLobbyHostModeDropdown.value].text;
    }

    public void CancelCreateLobby()
    {
        createLobbyPanel.SetActive(false);
    }

    public void ClickCreateLobby()
    {
        createLobbyPanel.SetActive(false);
        OnCreateLobby?.Invoke(this, new CreateLobbyEventArgs
        {
            lobbyName = createLobbyName,
            maxPlayers = createLobbyMaxPlayers,
            isPrivate = createLobbyVisibility == "Private",
            hostMode = createLobbyHostMode,
        });
    }
}
