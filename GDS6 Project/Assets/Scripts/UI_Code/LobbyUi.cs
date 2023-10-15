using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUi : MonoBehaviour
{
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private TextMeshProUGUI lobbyName;
    [SerializeField] private TextMeshProUGUI countdownTimerText;
    [SerializeField] private Transform container;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject initialUi;

    private int previousPlayerCount = 0;

    private Coroutine co;
    private float countdownTimer = 3;
    private bool callOnce = true;

    private bool isLocalPlayerReady = false;

    private void Awake()
    {
        leaveButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.LeaveLobby();
            initialUi.GetComponent<InitialUi>().Show();
            Hide();
        });
        readyButton.onClick.AddListener(() =>
        {
            isLocalPlayerReady = !isLocalPlayerReady;
            if (isLocalPlayerReady)
            {
                readyButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Unready";
            }
            else
            {
                readyButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Ready";
            }
            LobbyManager.Instance.TogglePlayerReady(isLocalPlayerReady);
            Debug.Log(isLocalPlayerReady);
        });
    }



    private void Start()
    {
        LobbyManager.Instance.OnJoinLobbyFinished += LobbyManager_OnJoinLobbyFinished;
        LobbyManager.Instance.OnCreateLobbyFinished += LobbyManager_OnCreateLobbyFinished;

        Hide();
    }

    private void LobbyManager_OnCreateLobbyFinished(object sender, System.EventArgs e)
    {
        Show();
    }

    private void LobbyManager_OnJoinLobbyFinished(object sender, System.EventArgs e)
    {
        Show();
    }

    private void Update()
    {
        UpdateReadyUi();

        if (previousPlayerCount != LobbyManager.Instance.GetPlayersInLobby().Count)
        {
            UpdatePlayerList();
            previousPlayerCount = LobbyManager.Instance.GetPlayersInLobby().Count;
        }

        bool allPlayersReady = true;
        foreach (Player player in LobbyManager.Instance.GetPlayersInLobby())
        {
            if (player.Data["ReadyState"].Value == "Unready")
            {
                allPlayersReady = false;
                if (co != null)
                {
                    StopCoroutine(co);
                    leaveButton.gameObject.SetActive(true);
                    countdownTimer = 3;
                    callOnce = true;
                }
            }
        }

        if (allPlayersReady && callOnce)
        {
            callOnce = false;
            co = StartCoroutine(StartCountdown());
            leaveButton.gameObject.SetActive(false);
        }

        if (countdownTimer != 3)
        {
            countdownTimerText.text = Mathf.CeilToInt(countdownTimer).ToString();
        }
        else
        {
            countdownTimerText.text = "";
        }
    }

    private IEnumerator StartCountdown()
    {
        countdownTimer = 3;
        while (countdownTimer > 0)
        {
            countdownTimer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        LobbyManager.Instance.StartGame();
    }

    private void UpdatePlayerList()
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        foreach (Player player in LobbyManager.Instance.GetPlayersInLobby())
        {
            GameObject prefab = Instantiate(playerPrefab, container);
            prefab.GetComponent<PlayerPrefab>().SetName(player.Data["PlayerName"].Value);
        }
    }

    private void UpdateReadyUi()
    {
        foreach (Player player in LobbyManager.Instance.GetPlayersInLobby())
        {
            foreach (Transform prefab in container)
            {
                if (prefab.GetComponent<PlayerPrefab>().GetName() == player.Data["PlayerName"].Value)
                {
                    prefab.GetComponent<PlayerPrefab>().SetReady(player.Data["ReadyState"].Value);
                }
            }
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
        readyButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Ready";
        isLocalPlayerReady = false;
        lobbyName.text = LobbyManager.Instance.GetJoinedLobby().Name;
        previousPlayerCount = 0;
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
