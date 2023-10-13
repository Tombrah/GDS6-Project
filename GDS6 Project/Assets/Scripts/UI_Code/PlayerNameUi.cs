using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerNameUi : MonoBehaviour
{
    private void Start()
    {
        LobbyManager.Instance.OnCreateLobbyStarted += LobbyManager_OnCreateLobbyStarted;
        LobbyManager.Instance.OnJoinLobbyStarted += LobbyManager_OnJoinLobbyStarted;

        if (string.IsNullOrWhiteSpace(PlayerData.Instance.GetPlayerName()))
        {
            Hide();
        }

        GetComponent<TMP_InputField>().text = PlayerData.Instance.GetPlayerName();
    }

    private void LobbyManager_OnJoinLobbyStarted(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void LobbyManager_OnCreateLobbyStarted(object sender, System.EventArgs e)
    {
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        GetComponent<TMP_InputField>().text = PlayerData.Instance.GetPlayerName();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
