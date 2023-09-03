using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class LobbyPrefab : MonoBehaviour
{
    [SerializeField] private TMP_Text lobbyName;
    [SerializeField] private TMP_Text lobbyPlayers;

    private LobbyManager testLobby;

    private Lobby lobby;

    public void Initialise(LobbyManager testLobby, Lobby lobby)
    {
        this.testLobby = testLobby;
        this.lobby = lobby;

        lobbyName.text = lobby.Name;
        lobbyPlayers.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
    }

    public void JoinLobby()
    {
        testLobby.JoinLobby(lobby);
    }
}
