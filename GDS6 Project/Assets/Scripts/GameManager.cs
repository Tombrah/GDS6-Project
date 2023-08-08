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
        RoundResetting,
        GameEnded
    }

    [SerializeField] private GameObject playerPrefab;
    public List<Transform> playerSpawnPoints;
    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);
    private NetworkVariable<int> round = new NetworkVariable<int>(0);
    private NetworkVariable<float> waitingToStartTimer = new NetworkVariable<float>(1f);
    private NetworkVariable<float> countdownTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);
    private NetworkVariable<float> roundResetTimer = new NetworkVariable<float>(0f);
    [Tooltip("In-game timer in seconds")]
    [SerializeField] private float gamePlayingTimerMax = 90f;
    [SerializeField] private float roundResetTimerMax = 10f;

    private bool callOnce;

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
            round.Value = 0;
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
            GameObject player = Instantiate(playerPrefab);
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
                if (roundResetTimer.Value < 0f)
                {
                    if (round.Value == 4) state.Value = State.GameEnded;

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
        if (!callOnce)
        {
            Debug.Log("Round Reset!");

            callOnce = true;
        }
    }
}
