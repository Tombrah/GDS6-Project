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

    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer = -1;
    private int previousPlayerCount = 0;

    [SerializeField] private Transform lobbyList;
    [SerializeField] private GameObject lobbyPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform container;
    [SerializeField] private TMP_InputField lobbyNameInput;
    [SerializeField] private TMP_Text lobbyNameText;
    [SerializeField] private TMP_InputField playerNameInput;

    private bool isLocalPlayerReady = false;
    [SerializeField] private GameObject readyButton;
    private float countdownTimer = 4f;
    private Coroutine co;

    private bool isJoining;
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

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            playerName = "PlayerName" + Mathf.RoundToInt(Random.Range(0, 100));
            playerNameInput.text = playerName;
            Debug.Log(playerName);
        }
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

    public async void CreateLobby()
    {
        try
        {
            string lobbyName = "My Lobby";
            if (lobbyNameInput.text != "")
            {
                lobbyName = lobbyNameInput.text;
            }
            int maxPlayers = 2;
            if (playerNameInput.text != "")
            {
                playerName = playerNameInput.text;
            }
            playerNameInput.gameObject.SetActive(false);

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

            GameObject playerInstance = Instantiate(playerPrefab, container);
            PlayerPrefab playerPrefabScript = playerInstance.GetComponent<PlayerPrefab>();
            playerPrefabScript.SetName(playerName);

            lobbyNameText.text = lobbyName;

            CreateRelay();
            //PlayerData.Instance.UpdatePlayerNameServerRpc(playerName);

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
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                    new QueryFilter(QueryFilter.FieldOptions.S1, "0", QueryFilter.OpOptions.NE)
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
            if (playerNameInput.text != "")
            {
                playerName = playerNameInput.text;
            }
            playerNameInput.gameObject.SetActive(false);

            JoinLobbyByIdOptions joinLobbyOptions = new JoinLobbyByIdOptions
            {
                Player = GetPlayer()
            };

            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(targetLobby.Id, joinLobbyOptions);

            joinedLobby = lobby;

            JoinRelay(joinedLobby.Data["RelayCode"].Value);
            //PlayerData.Instance.UpdatePlayerNameServerRpc(playerName);

            readyButton.SetActive(true);

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
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
                { "ReadyState", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "Unready") }
            }
        };
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
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
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
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }
}
