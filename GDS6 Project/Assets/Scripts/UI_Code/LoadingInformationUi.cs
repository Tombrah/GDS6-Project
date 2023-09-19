using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingInformationUi : MonoBehaviour
{
    public static LoadingInformationUi Instance { get; private set; }

    [SerializeField] private GameObject information;
    [SerializeField] private GameObject waitingText;

    public bool canInteract = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        Show();
        information.SetActive(true);
        waitingText.SetActive(false);
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (!GameManager.Instance.IsWaitingToStart())
        {
            Hide();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && canInteract)
        {
            GameManager.Instance.SetPlayerReadyServerRpc();
            information.SetActive(false);
            waitingText.SetActive(true);
            canInteract = false;
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
