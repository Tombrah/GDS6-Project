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

    private int[] previousScore;

    TMP_Text[] playerScoreText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;

        playerScoreText = new TMP_Text[] { players[0].GetComponentsInChildren<TMP_Text>()[1], players[1].GetComponentsInChildren<TMP_Text>()[1] };
        previousScore = new int[] { 0, 0 };

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
        StartCoroutine(ShowPlayerScores2());
    }

    private void Hide()
    {
        cam.Priority = 0;
        gameObject.SetActive(false);
    }

    private IEnumerator ShowPlayerScores()
    {
        foreach (int score in GameManager.Instance.playerScores)
        {
            int index = GameManager.Instance.playerScores.IndexOf(score);

            float percentage = 0;
            while (percentage < 1)
            {
                playerScoreText[index].text = ((int)Mathf.Lerp(previousScore[index], score, percentage)).ToString();

                percentage += Time.deltaTime / showcaseSpeed;
                yield return new WaitForEndOfFrame();
            }

            playerScoreText[index].text = score.ToString();
            previousScore[index] = score;
        }
        yield return null;
    }

    private IEnumerator ShowPlayerScores2()
    {
        float percentage = 0;
        while (percentage < 1)
        {
            playerScoreText[0].text = ((int)Mathf.Lerp(previousScore[0], GameManager.Instance.playerScores[0], percentage)).ToString();
            playerScoreText[1].text = ((int)Mathf.Lerp(previousScore[1], GameManager.Instance.playerScores[1], percentage)).ToString();

            percentage += Time.deltaTime / showcaseSpeed;
            yield return new WaitForEndOfFrame();
        }

        playerScoreText[0].text = GameManager.Instance.playerScores[0].ToString();
        playerScoreText[1].text = GameManager.Instance.playerScores[1].ToString();
        previousScore[0] = GameManager.Instance.playerScores[0];
        previousScore[1] = GameManager.Instance.playerScores[1];

    }

    public void SetPlayerName(int index, string playerName)
    {
        players[index].text = playerName;
    }
}
