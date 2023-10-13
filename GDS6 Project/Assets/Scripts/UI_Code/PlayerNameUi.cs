using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerNameUi : MonoBehaviour
{
    private TMP_InputField nameInputField;

    private void Start()
    {
        nameInputField = GetComponent<TMP_InputField>();

        LobbyManager.Instance.OnCreateLobbyStarted += LobbyManager_OnCreateLobbyStarted;
        LobbyManager.Instance.OnJoinLobbyStarted += LobbyManager_OnJoinLobbyStarted;
        nameInputField.onValueChanged.AddListener(delegate { ValueChanged(); });

        if (string.IsNullOrWhiteSpace(PlayerData.Instance.GetPlayerName()))
        {
            Hide();
        }

        nameInputField.text = PlayerData.Instance.GetPlayerName();
    }

    private void ValueChanged()
    {
        PlayerData.Instance.SetPlayerName(nameInputField.text);
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
        nameInputField.text = PlayerData.Instance.GetPlayerName();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
