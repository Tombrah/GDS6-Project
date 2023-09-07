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

    private LobbyManager lobbyManager;

    private Lobby lobby;

    public void Initialise(LobbyManager lobbyManager, Lobby lobby)
    {
        this.lobbyManager = lobbyManager;
        this.lobby = lobby;

        lobbyName.text = lobby.Name;
        lobbyPlayers.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
    }

    public void JoinLobby()
    {
        lobbyManager.JoinLobby(lobby);
    }
}
