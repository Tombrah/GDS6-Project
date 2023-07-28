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
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private int previousPlayerCount = 0;

    [SerializeField] private Transform lobbyList;
    [SerializeField] private GameObject lobbyPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform container;

    private bool isJoining;
    private bool isHost;

    private string playerName;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        playerName = "Tombrah" + Mathf.RoundToInt(Random.Range(0, 100));
        Debug.Log(playerName);
    }

    private void Update()
    {
        HandleHeartbeat();
        HandleLobbyUpdate();
    }

    private async void HandleHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0)
            {
                heartbeatTimer = 15;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void HandleLobbyUpdate()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0)
            {
                lobbyUpdateTimer = 1.1f;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
            }

            if (previousPlayerCount < joinedLobby.Players.Count)
            {
                foreach (Transform child in container)
                {
                    if (child.CompareTag("PlayerUI"))
                    {
                        continue;
                    }
                    Destroy(child.gameObject);
                }

                foreach (Player player in joinedLobby.Players)
                {
                    GameObject playerInstance = Instantiate(playerPrefab, container);
                    PlayerPrefab playerPrefabScript = playerInstance.GetComponent<PlayerPrefab>();
                    playerPrefabScript.SetName(GetPlayerName(player));
                }

                previousPlayerCount = joinedLobby.Players.Count;
            }

            if (joinedLobby.Data["RelayCode"].Value != "0")
            {
                if (!isHost)
                {
                    GetComponent<TestRelay>().JoinRelay(joinedLobby.Data["RelayCode"].Value);

                    joinedLobby = null;
                }

                gameObject.SetActive(false);
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
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, "0") }
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, lobbyOptions);

            hostLobby = lobby;
            joinedLobby = hostLobby;

            isHost = true;

            GameObject playerInstance = Instantiate(playerPrefab, container);
            PlayerPrefab playerPrefabScript = playerInstance.GetComponent<PlayerPrefab>();
            playerPrefabScript.SetName(playerName);

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
            foreach (Transform child in container)
            {
                if (child.CompareTag("LobbyUI"))
                {
                    continue;
                }
                Destroy(child.gameObject);
            }

            foreach(Lobby lobby in queryResponse.Results)
            {
                GameObject lobbyInstance = Instantiate(lobbyPrefab, container);
                LobbyPrefab lobbyPrefabScript = lobbyInstance.GetComponent<LobbyPrefab>();

                lobbyPrefabScript.Initialise(this, lobby);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobby(Lobby targetLobby)
    {
        if (isJoining) { return; }

        isJoining = true;
        
        try
        {
            JoinLobbyByIdOptions joinLobbyOptions = new JoinLobbyByIdOptions
            {
                Player = GetPlayer()
            };

            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(targetLobby.Id, joinLobbyOptions);

            joinedLobby = lobby;

            Debug.Log("Joined Lobby!");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            isJoining = false;
        }

        isJoining = false;
    }

    public async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            }
        };
    }


    private string GetPlayerName(Player player)
    {
        return player.Data["PlayerName"].Value;
    }

    public async void StartGame()
    {
        if (isHost)
        {
            try
            {
                string relayCode = await GetComponent<TestRelay>().CreateRelay();

                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                    }
                });

                joinedLobby = lobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
}
