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
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_Text lobbyNameText;

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
                    DestroyImmediate(child.gameObject);
                }

                foreach (Player player in joinedLobby.Players)
                {
                    GameObject playerInstance = Instantiate(playerPrefab, container);
                    PlayerPrefab playerPrefabScript = playerInstance.GetComponent<PlayerPrefab>();
                    playerPrefabScript.SetName(GetPlayerName(player));
                }

                previousPlayerCount = joinedLobby.Players.Count;
            }
        }
    }

    public async void CreateLobby()
    {
        try
        {
            string lobbyName = nameInput.text;
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

            lobbyNameText.text = lobbyName;

            CreateRelay();

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
                DestroyImmediate(child.gameObject);
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

            JoinRelay(joinedLobby.Data["RelayCode"].Value);

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

    public void StartGame()
    {
        if (isHost)
        {
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
                        { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
                    }
            });
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}
