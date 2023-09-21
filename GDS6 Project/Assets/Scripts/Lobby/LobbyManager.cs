using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    public event EventHandler OnCreateLobbyStarted;
    public event EventHandler OnCreateLobbyFinished;
    public event EventHandler OnCreateLobbyFailed;
    public event EventHandler OnJoinLobbyStarted;
    public event EventHandler OnJoinLobbyFinished;
    public event EventHandler OnJoinLobbyFailed;
    public event EventHandler<OnlobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnlobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer = -1;
    private int previousPlayerCount = 0;

    private bool isLocalPlayerReady = false;
    private float countdownTimer = 4f;
    private Coroutine co;

    private bool isHost;
    private bool allPlayersReady = false;
    private bool canRun = true;

    public string playerName;

    private void Awake()
    {
        Instance = this;
    }

    private async void Start()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        playerName = "PlayerName: " + UnityEngine.Random.Range(0, 100);
    }

    private void Update()
    {
        HandleHeartbeat();
        HandleLobbyUpdate();

        if (allPlayersReady && canRun)
        {
            canRun = false;
            co = StartCoroutine(StartCountdown());
        }
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

            allPlayersReady = true;
            foreach (Player player in joinedLobby.Players)
            {
                if (GetPlayerReadyState(player) != "Ready")
                {
                    if (co != null) StopCoroutine(co);
                    countdownTimer = 4;
                    allPlayersReady = false;
                    canRun = true;
                    break;
                }
            }
        }
    }

    public async void CreateLobby(string lobbyName)
    {
        OnCreateLobbyStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            if (lobbyName == "") lobbyName = "My Lobby!";

            int maxPlayers = 2;

            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { "RelayCode", new DataObject(DataObject.VisibilityOptions.Public, "0", DataObject.IndexOptions.S1) }
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, lobbyOptions);

            hostLobby = lobby;
            joinedLobby = hostLobby;

            isHost = true;

            CreateRelay();

            Debug.Log("Created Lobby! " + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            OnCreateLobbyFailed?.Invoke(this, EventArgs.Empty);
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
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                    new QueryFilter(QueryFilter.FieldOptions.S1, "0", QueryFilter.OpOptions.NE)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };
            
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbyOptions);

            OnLobbyListChanged?.Invoke(this, new OnlobbyListChangedEventArgs
            {
                lobbyList = queryResponse.Results
            });

            Debug.Log("Lobbies Found: " + queryResponse.Results.Count);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobby(Lobby targetLobby)
    {
        OnJoinLobbyStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            JoinLobbyByIdOptions joinLobbyOptions = new JoinLobbyByIdOptions
            {
                Player = GetPlayer()
            };

            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(targetLobby.Id, joinLobbyOptions);

            joinedLobby = lobby;

            JoinRelay(joinedLobby.Data["RelayCode"].Value);

            Debug.Log("Joined Lobby!");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            OnJoinLobbyFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

            joinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void Deletelobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);

                joinedLobby = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
                { "ReadyState", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "Unready") }
            }
        };
    }

    public List<Player> GetPlayersInLobby()
    {
        return joinedLobby.Players;
    }

    private string GetPlayerName(Player player)
    {
        return player.Data["PlayerName"].Value;
    }

    private string GetPlayerReadyState(Player player)
    {
        return player.Data["ReadyState"].Value;
    }

    public async void UpdatePlayerReady()
    {
        try
        {
            isLocalPlayerReady = !isLocalPlayerReady;
            string readyState;
            if (isLocalPlayerReady)
            {
                readyState = "Ready";
            }
            else
            {
                readyState = "Unready";
            }
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "ReadyState", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, readyState) }
                }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private IEnumerator StartCountdown()
    {
        while (countdownTimer > 0)
        {
            countdownTimer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        StartGame();
    }

    public float GetCountdownTimer()
    {
        return countdownTimer;
    }

    public void StartGame()
    {
        if (isHost)
        {
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                Debug.Log(clientId);
            }
            Loader.LoadNetwork(Loader.Scene.GameScene);
        }
    }

    public async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "RelayCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode, DataObject.IndexOptions.S1) }
                }
            });

            OnCreateLobbyFinished?.Invoke(this, EventArgs.Empty);
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            OnCreateLobbyFailed?.Invoke(this, EventArgs.Empty);
            Deletelobby();
        }
    }

    public async void JoinRelay(string joinCode)
    {
        Debug.Log("Join Code is: " + joinCode);
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();

            OnJoinLobbyFinished?.Invoke(this, EventArgs.Empty);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
            OnJoinLobbyFailed?.Invoke(this, EventArgs.Empty);
            LeaveLobby();
        }
    }
}
