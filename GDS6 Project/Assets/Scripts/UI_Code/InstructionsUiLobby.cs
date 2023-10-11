using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InstructionsUiLobby : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject initialUi;

    private void Awake()
    {
        backButton.onClick.AddListener(() =>
        {
            initialUi.SetActive(true);
            Hide();
        });
    }

    private void Start()
    {
        LobbyManager.Instance.OnCreateLobbyStarted += LobbyManager_OnCreateLobbyStarted;

        Hide();
    }
    private void LobbyManager_OnCreateLobbyStarted(object sender, System.EventArgs e)
    {
        Hide();
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
