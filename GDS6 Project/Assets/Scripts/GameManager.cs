using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;

    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameEnded
    }

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<Transform> playerSpawnPoints;
    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);
    private NetworkVariable<float> waitingToStartTimer = new NetworkVariable<float>(1f);
    private NetworkVariable<float> countdownTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);
    [Tooltip("In-game timer in seconds")]
    [SerializeField] private float gamePlayingTimerMax = 90f;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            GameObject player = Instantiate(playerPrefab, playerSpawnPoints[(int)clientId]);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
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
                    Debug.Log("Waiting Finished");
                }
                break;
            case State.CountdownToStart:
                countdownTimer.Value -= Time.deltaTime;
                if (countdownTimer.Value < 0f)
                {
                    state.Value = State.GamePlaying;
                    gamePlayingTimer.Value = gamePlayingTimerMax;
                }
                break;
            case State.GamePlaying:
                gamePlayingTimer.Value -= Time.deltaTime;
                if (gamePlayingTimer.Value < 0f)
                {
                    state.Value = State.GameEnded;
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
}
