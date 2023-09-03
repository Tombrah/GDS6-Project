using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerReadyUI : MonoBehaviour
{
    [SerializeField] private GameObject readyText;
    [SerializeField] private GameObject unreadyText;
    [SerializeField] private Button readyButton;

    private void Awake()
    {
        readyButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.UpdatePlayerReady();
            readyText.SetActive(!readyText.gameObject.activeSelf);
            unreadyText.SetActive(!unreadyText.gameObject.activeSelf);
        });
    }
}
