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

    private Lobby lobby;

    private void Awake()
    {
        GetComponentInChildren<Button>().onClick.AddListener(JoinLobby);
    }

    public void Initialise(Lobby lobby)
    {
        this.lobby = lobby;

        lobbyName.text = lobby.Name;
    }

    public void JoinLobby()
    {
        LobbyManager.Instance.JoinLobby(lobby);
    }
}
