using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class ListLobbiesUi : MonoBehaviour
{
    [SerializeField] private Button refreshButton;
    [SerializeField] private Transform container;
    [SerializeField] private GameObject lobbyPrefab;

    private void Awake()
    {
        refreshButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.ListLobbies();
        });
    }
    private void Start()
    {
        LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
        LobbyManager.Instance.OnJoinLobbyStarted += LobbyManager_OnJoinLobbyStarted;

        Hide();
    }

    private void LobbyManager_OnJoinLobbyStarted(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager.OnlobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList)
        {
            GameObject prefab = Instantiate(lobbyPrefab, container);
            prefab.GetComponent<LobbyPrefab>().Initialise(lobby);
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
