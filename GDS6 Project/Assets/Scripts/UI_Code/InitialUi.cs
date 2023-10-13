using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitialUi : MonoBehaviour
{
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button listLobbiesButton;
    [SerializeField] private Button HtpButton;
    [SerializeField] private Button SettingsButton;

    [SerializeField] private GameObject playerNameInput;
    [SerializeField] private GameObject createLobbyUi;
    [SerializeField] private GameObject listLobbiesUi;
    [SerializeField] private GameObject HtpUi;
    [SerializeField] private GameObject SettingsUi;


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
        HtpButton.onClick.AddListener(() =>
        {
            HtpUi.SetActive(true);
            Hide();
        });
        SettingsButton.onClick.AddListener(() =>
        {
            SettingsUi.SetActive(true);
            Hide();
        });
    }

    private void Start()
    {
        if (string.IsNullOrWhiteSpace(PlayerData.Instance.GetPlayerName()))
        {
            Hide();
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        playerNameInput.GetComponent<PlayerNameUi>().Show();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
