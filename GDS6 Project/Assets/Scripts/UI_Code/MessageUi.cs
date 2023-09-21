using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessageUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;
    [SerializeField] private LobbyUi lobbyUi;

    private void Awake()
    {
        closeButton.onClick.AddListener(Hide);
    }

    private void Start()
    {
        LobbyManager.Instance.OnCreateLobbyStarted += LobbyManager_OnCreateLobbyStarted;
        LobbyManager.Instance.OnCreateLobbyFailed += LobbyManager_OnCreateLobbyFailed;
        LobbyManager.Instance.OnCreateLobbyFinished += LobbyManager_OnCreateLobbyFinished;
        LobbyManager.Instance.OnJoinLobbyStarted += LobbyManager_OnJoinLobbyStarted;
        LobbyManager.Instance.OnJoinLobbyFinished += LobbyManager_OnJoinLobbyFinished;
        LobbyManager.Instance.OnJoinLobbyFailed += LobbyManager_OnJoinLobbyFailed;

        Hide();
    }

    private void LobbyManager_OnJoinLobbyFinished(object sender, System.EventArgs e)
    {
        lobbyUi.Show();
        Hide();
    }

    private void LobbyManager_OnCreateLobbyFinished(object sender, System.EventArgs e)
    {
        lobbyUi.Show();
        Hide();
    }

    private void LobbyManager_OnJoinLobbyFailed(object sender, System.EventArgs e)
    {
        ShowMessage("Failed to join Lobby!");
        closeButton.gameObject.SetActive(true);
    }

    private void LobbyManager_OnJoinLobbyStarted(object sender, System.EventArgs e)
    {
        ShowMessage("Joining Lobby...");
    }

    private void LobbyManager_OnCreateLobbyFailed(object sender, System.EventArgs e)
    {
        ShowMessage("Failed to create Lobby!");
        closeButton.gameObject.SetActive(true);
    }

    private void LobbyManager_OnCreateLobbyStarted(object sender, System.EventArgs e)
    {
        ShowMessage("Creating Lobby...");
    }

    private void ShowMessage(string message)
    {
        Show();
        closeButton.gameObject.SetActive(false);
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
}
