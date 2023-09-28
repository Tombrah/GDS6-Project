using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;
using Unity.Services.Lobbies;

public class RoundResetUI : MonoBehaviour
{
    public static RoundResetUI Instance { get; private set; }

    [SerializeField] private CinemachineVirtualCamera cam;
    public TMP_Text[] players;
    [SerializeField] float showcaseSpeed = 3;

    TMP_Text[] playerScoreText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;

        playerScoreText = new TMP_Text[] { players[0].GetComponentsInChildren<TMP_Text>()[1], players[1].GetComponentsInChildren<TMP_Text>()[1] };

        Hide();
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsRoundResetting())
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
        StartCoroutine(ShowPlayerScores());
    }

    private void Hide()
    {
        cam.Priority = 0;
        gameObject.SetActive(false);
    }

    private IEnumerator ShowPlayerScores()
    {
        float percentage = 0;
        while (percentage < 1)
        {
            playerScoreText[0].text = ((int)Mathf.Lerp(0, GameManager.Instance.playerRoundScores[0], percentage)).ToString();
            if (GameManager.Instance.playerScores.Count == 2)
            {
                playerScoreText[1].text = ((int)Mathf.Lerp(0, GameManager.Instance.playerRoundScores[1], percentage)).ToString();
            }

            percentage += Time.deltaTime / showcaseSpeed;
            yield return new WaitForEndOfFrame();
        }

        playerScoreText[0].text = GameManager.Instance.playerRoundScores[0].ToString();
        if (GameManager.Instance.playerScores.Count == 2)
        {
            playerScoreText[1].text = GameManager.Instance.playerRoundScores[1].ToString();
        }
    }

    public void SetPlayerName(int index, string playerName)
    {
        players[index].text = playerName;
    }
}
