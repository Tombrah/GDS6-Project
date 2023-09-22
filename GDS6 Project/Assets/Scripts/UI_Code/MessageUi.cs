using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using Unity.Services.Authentication;

public class MessageUi : MonoBehaviour
{
    public static MessageUi Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject initialUi;
    [SerializeField] private GameObject lobbyUi;

    public bool restart = false;
    private bool sendBack;

    private void Awake()
    {
        Instance = this;

        closeButton.onClick.AddListener(() => 
        {
            if (restart) Loader.Load(Loader.Scene.MainMenu);
            if (sendBack) initialUi.GetComponent<InitialUi>().Show();
            Hide();
        });
    }

    private void Start()
    {
        LobbyManager.Instance.OnCreateLobbyStarted += LobbyManager_OnCreateLobbyStarted;
        LobbyManager.Instance.OnCreateLobbyFailed += LobbyManager_OnCreateLobbyFailed;
        LobbyManager.Instance.OnCreateLobbyFinished += LobbyManager_OnCreateLobbyFinished;
        LobbyManager.Instance.OnJoinLobbyStarted += LobbyManager_OnJoinLobbyStarted;
        LobbyManager.Instance.OnJoinLobbyFinished += LobbyManager_OnJoinLobbyFinished;
        LobbyManager.Instance.OnJoinLobbyFailed += LobbyManager_OnJoinLobbyFailed;

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

        Hide();
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (clientId == NetworkManager.ServerClientId && !LobbyManager.Instance.isHost)
        {
            ShowMessage("Host disconnected", true);
            sendBack = true;
            lobbyUi.SetActive(false);
        }
    }

    private void LobbyManager_OnJoinLobbyFinished(object sender, System.EventArgs e)
    {
        lobbyUi.SetActive(true);
        Hide();
    }

    private void LobbyManager_OnCreateLobbyFinished(object sender, System.EventArgs e)
    {
        lobbyUi.SetActive(true);
        Hide();
    }

    private void LobbyManager_OnJoinLobbyFailed(object sender, System.EventArgs e)
    {
        ShowMessage("Failed to join Lobby!", true);
        sendBack = true;
    }

    private void LobbyManager_OnJoinLobbyStarted(object sender, System.EventArgs e)
    {
        ShowMessage("Joining Lobby...");
    }

    private void LobbyManager_OnCreateLobbyFailed(object sender, System.EventArgs e)
    {
        ShowMessage("Failed to create Lobby!", true);
        sendBack = true;
    }

    private void LobbyManager_OnCreateLobbyStarted(object sender, System.EventArgs e)
    {
        ShowMessage("Creating Lobby...");
    }

    public void ShowMessage(string message, bool showButton = false)
    {
        Show();
        sendBack = false;
        closeButton.gameObject.SetActive(showButton);
        messageText.text = message;
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
        }
    }
}
