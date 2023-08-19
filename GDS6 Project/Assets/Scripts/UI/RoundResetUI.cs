using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;

public class RoundResetUI : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera cam;

    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        Hide();
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsRoundResetting() || GameManager.Instance.IsGameOver())
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        cam.Priority = 10;
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        cam.Priority = 0;
        gameObject.SetActive(false);
    }
}
