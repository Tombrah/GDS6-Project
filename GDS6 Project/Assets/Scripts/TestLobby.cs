using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using TMPro;

public class TestLobby : MonoBehaviour
{
    [SerializeField] private TMP_Text displayCode;
    private Lobby hostLobby;
    private float timer;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {
        
    }

    private async void HandleHeartbeat()
    {
        if (hostLobby != null)
        {
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                timer = 15;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    public async void CreateLobby()
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 2;
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = true,
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers);

            hostLobby = lobby;

            displayCode.text = lobby.LobbyCode;
            Debug.Log("Created Lobby! " + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbyOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };
            
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbyOptions);

            Debug.Log("Lobbies Found: " + queryResponse.Results.Count);
            foreach(Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobby()
    {
        try
        {
            string codeInput = GetComponentInChildren<TMP_InputField>().text;
            await Lobbies.Instance.JoinLobbyByCodeAsync(codeInput);

            Debug.Log("Joined Lobby!");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}
