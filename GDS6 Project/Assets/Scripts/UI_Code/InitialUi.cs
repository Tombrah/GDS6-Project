using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitialUi : MonoBehaviour
{
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button listLobbiesButton;
    [SerializeField] private GameObject playerNameInput;
    [SerializeField] private GameObject createLobbyUi;
    [SerializeField] private GameObject listLobbiesUi;

    private void Awake()
    {
        createLobbyButton.onClick.AddListener(() =>
        {
            createLobbyUi.SetActive(true);
            Hide();
        });
        listLobbiesButton.onClick.AddListener(() =>
        {
            listLobbiesUi.SetActive(true);
            LobbyManager.Instance.ListLobbies();
            Hide();
        });
    }

    private void Start()
    {
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
        playerNameInput.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
