using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;

    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        RoundResetting,
        GameEnded
    }

    [SerializeField] private Transform[] playerPrefabs;
    public List<Transform> playerSpawnPoints;
    public List<Transform> respawnPoints;

    [HideInInspector] public NetworkList<int> playerScores;
    private Dictionary<ulong, bool> playerReadyDictionary;
    private Dictionary<ulong, bool> playerJoinDictionary;

    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);
    private NetworkVariable<int> round = new NetworkVariable<int>(0);
    private NetworkVariable<float> countdownTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);
    private NetworkVariable<float> roundResetTimer = new NetworkVariable<float>(0f);
    [Tooltip("In-game timer in seconds")]
    [SerializeField] private int roundMax = 4;
    [SerializeField] private float countdownTimerMax = 3f;
    [SerializeField] private float gamePlayingTimerMax = 90f;
    [SerializeField] private float roundResetTimerMax = 10f;

    private int roleId;
    private bool callOnce;

    private void Awake()
    {
        Instance = this;

        playerScores = new NetworkList<int>();

        playerReadyDictionary = new Dictionary<ulong, bool>();
        playerJoinDictionary = new Dictionary<ulong, bool>();
    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;
        if (IsServer)
        {
            round.Value = 0;
        }
        PlayerJoinedServerRpc();
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        switch (state.Value)
        {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                countdownTimer.Value -= Time.deltaTime;
                if (countdownTimer.Value < 0f)
                {
                    state.Value = State.GamePlaying;
                    gamePlayingTimer.Value = gamePlayingTimerMax;
                    round.Value++;
                }
                break;
            case State.GamePlaying:
                gamePlayingTimer.Value -= Time.deltaTime;
                if (gamePlayingTimer.Value < 0f)
                {
                    state.Value = State.RoundResetting;
                    roundResetTimer.Value = roundResetTimerMax;
                    callOnce = false;
                }
                break;
            case State.RoundResetting:
                roundResetTimer.Value -= Time.deltaTime;
                if (!callOnce)
                {
                    foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
                    {
                        NetworkObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
                        player.Despawn();
                    }

                    callOnce = true;
                }
                if (roundResetTimer.Value < 0f)
                {
                    if (round.Value == roundMax)
                    {
                        state.Value = State.GameEnded;
                        break;
                    }
                    ResetRound();
                    RobbingManager.Instance.RespawnAllItems();
                    state.Value = State.CountdownToStart;
                    countdownTimer.Value = countdownTimerMax;
                }
                break;
            case State.GameEnded:
                break;
        }
    }
    public bool IsWaitingToStart()
    {
        return state.Value == State.WaitingToStart;
    }
    public bool IsCountdownActive()
    {
        return state.Value == State.CountdownToStart;
    }

    public bool IsGamePlaying()
    {
        return state.Value == State.GamePlaying;
    }

    public bool IsRoundResetting()
    {
        return state.Value == State.RoundResetting;
    }

    public bool IsGameOver()
    {
        return state.Value == State.GameEnded;
    }

    public float GetCountdownTimer()
    {
        return countdownTimer.Value;
    }

    public float GetGameTimer()
    {
        return gamePlayingTimer.Value;
    }
    [ServerRpc(RequireOwnership = false)]
    public void UpdateGameTimerServerRpc(float reductionTime)
    {
        gamePlayingTimer.Value -= reductionTime;
    }

    private void ResetRound()
    {
        if (NetworkManager.Singleton.ConnectedClientsIds.Count == 2)
        {
            roleId = (roleId * -1) + 1;
        }

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform player = Instantiate(playerPrefabs[roleId], playerSpawnPoints[roleId].position, playerSpawnPoints[roleId].rotation);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            player.GetComponent<NetworkObject>().ChangeOwnership(clientId);

            roleId = (roleId * -1) + 1;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                allClientsReady = false;
                break;
            }

        }

        if (allClientsReady)
        {
            roleId = Mathf.CeilToInt(UnityEngine.Random.Range(0, 2));
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                playerScores.Add(0);

                Transform player = Instantiate(playerPrefabs[roleId], playerSpawnPoints[roleId].position, playerSpawnPoints[roleId].rotation);
                player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
                player.GetComponent<NetworkObject>().ChangeOwnership(clientId);

                roleId = (roleId * -1) + 1;
            }
            RobbingManager.Instance.RespawnAllItems();

            int index = 0;
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (PlayerData.Instance.GetPlayerNamesDictionary().ContainsKey(clientId))
                {
                    SetPlayerNameClientRpc(index, PlayerData.Instance.GetPlayerNamesDictionary()[clientId]);
                    index++;
                }
            }

            state.Value = State.CountdownToStart;
            countdownTimer.Value = countdownTimerMax;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerJoinedServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerJoinDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsJoined = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerJoinDictionary.ContainsKey(clientId) || !playerJoinDictionary[clientId])
            {
                allClientsJoined = false;
                break;
            }

        }

        if (allClientsJoined)
        {
            CanSetReadyClientRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerScoresServerRpc(ulong clientId, int score, bool isAdditive)
    {
        if (isAdditive)
        {
            int oldScore = playerScores[(int)clientId];
            playerScores[(int)clientId] = oldScore + score;
        }
        else
        {
            playerScores[(int)clientId] = score;
        }
    }

    [ClientRpc]
    private void CanSetReadyClientRpc()
    {
        LoadingInformationUi.Instance.canInteract = true;
    }

    [ClientRpc]
    private void SetPlayerNameClientRpc(int index, string playerName)
    {
        RoundResetUI.Instance.SetPlayerName(index, playerName);
    }
}
