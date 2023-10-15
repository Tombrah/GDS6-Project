using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingInformationUi : MonoBehaviour
{
    public static LoadingInformationUi Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (!GameManager.Instance.IsWaitingToStart())
        {
            Hide();
        }
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
