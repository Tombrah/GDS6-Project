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
 
    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);
    private NetworkVariable<int> round = new NetworkVariable<int>(0);
    private NetworkVariable<float> waitingToStartTimer = new NetworkVariable<float>(1f);
    private NetworkVariable<float> countdownTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);
    private NetworkVariable<float> roundResetTimer = new NetworkVariable<float>(0f);
    [Tooltip("In-game timer in seconds")]
    [SerializeField] private int roundMax = 4;
    [SerializeField] private float gamePlayingTimerMax = 90f;
    [SerializeField] private float roundResetTimerMax = 10f;

    private int roleId;
    private bool callOnce;

    private void Awake()
    {
        Instance = this;

        playerScores = new NetworkList<int>();
    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
            round.Value = 0;
        }
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        roleId = Mathf.CeilToInt(UnityEngine.Random.Range(0, 2));
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            playerScores.Add(0);

            Transform player = Instantiate(playerPrefabs[roleId], playerSpawnPoints[roleId].position, playerSpawnPoints[roleId].rotation);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

            roleId = (roleId * -1) + 1;
        }
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
                waitingToStartTimer.Value -= Time.deltaTime;
                if (waitingToStartTimer.Value < 0f)
                {
                    state.Value = State.CountdownToStart;
                    countdownTimer.Value = 3f;
                }
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
                    state.Value = State.WaitingToStart;
                }
                break;
            case State.GameEnded:
                break;
        }
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

            roleId = (roleId * -1) + 1;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerScoresServerRpc(ulong clientId, int score)
    {
        int oldScore = playerScores[(int)clientId];
        playerScores[(int)clientId] = oldScore + score;
    }
}
