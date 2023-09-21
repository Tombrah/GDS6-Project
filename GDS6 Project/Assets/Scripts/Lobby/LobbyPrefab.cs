using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

public class LobbyPrefab : MonoBehaviour
{
    [SerializeField] private TMP_Text lobbyName;
    [SerializeField] private TMP_Text lobbyPlayers;

    private Lobby lobby;

    private void Awake()
    {
        GetComponentInChildren<Button>().onClick.AddListener(JoinLobby);
    }

    public void Initialise(Lobby lobby)
    {
        this.lobby = lobby;

        lobbyName.text = lobby.Name;
        lobbyPlayers.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
    }

    public void JoinLobby()
    {
        LobbyManager.Instance.JoinLobby(lobby);
    }
}
