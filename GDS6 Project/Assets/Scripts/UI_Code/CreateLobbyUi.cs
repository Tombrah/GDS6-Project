using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreateLobbyUi : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_InputField lobbyNameInput;
    [SerializeField] private GameObject initialUi;

    private void Awake()
    {
        createButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.CreateLobby(lobbyNameInput.text);
            Hide();
        });
        backButton.onClick.AddListener(() =>
        {
            initialUi.SetActive(true);
            Hide();
        });
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
