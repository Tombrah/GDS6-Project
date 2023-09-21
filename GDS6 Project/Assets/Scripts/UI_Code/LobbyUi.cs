using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUi : MonoBehaviour
{
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Transform container;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject initialUi;

    private int previousPlayerCount = 0;
    private bool isReady = false;

    private void Awake()
    {
        leaveButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.LeaveLobby();
            initialUi.SetActive(true);
            Hide();
        });
        readyButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.UpdatePlayerReady();
            isReady = !isReady;
            if (isReady)
            {
                readyButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Unready";
            }
            else
            {
                readyButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Ready";
            }
        });
    }

    private void Start()
    {
        LobbyManager.Instance.OnJoinLobbyFinished += LobbyManager_OnJoinLobbyFinished;
        LobbyManager.Instance.OnCreateLobbyFinished += LobbyManager_OnCreateLobbyFinished;

        Hide();
    }

    private void LobbyManager_OnCreateLobbyFinished(object sender, System.EventArgs e)
    {
        Show();
    }

    private void LobbyManager_OnJoinLobbyFinished(object sender, System.EventArgs e)
    {
        Show();
    }

    private void Update()
    {
        if (previousPlayerCount != LobbyManager.Instance.GetPlayersInLobby().Count)
        {
            UpdatePlayerList();
            previousPlayerCount = LobbyManager.Instance.GetPlayersInLobby().Count;
        }
    }


    private void UpdatePlayerList()
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        foreach (Player player in LobbyManager.Instance.GetPlayersInLobby())
        {
            GameObject prefab = Instantiate(playerPrefab, container);
            prefab.GetComponent<PlayerPrefab>().SetName(player.Data["PlayerName"].Value);
        }
    }
    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
