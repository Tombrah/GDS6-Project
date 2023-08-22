using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;
using Unity.Services.Lobbies;

public class RoundResetUI : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera cam;
    [SerializeField] private TMP_Text player1;
    [SerializeField] private TMP_Text player2;
    [SerializeField] private TMP_Text[] players;
    [SerializeField] float showcaseSpeed = 3;

    private int previousScore1;
    private int previousScore2;
    private int[] previousScore;

    TMP_Text player1Score;
    TMP_Text player2Score;
    TMP_Text[] playerScoreText;

    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;

        player1Score = player1.transform.GetComponentsInChildren<TMP_Text>()[1];
        player2Score = player2.transform.GetComponentsInChildren<TMP_Text>()[1];

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
        previousScore1 = GameManager.Instance.playerScores[0];
        previousScore2 = GameManager.Instance.playerScores[1];

        float percentage = 0;
        while (percentage < 1)
        {
            player1Score.text = ((int)Mathf.Lerp(previousScore1, GameManager.Instance.playerScores[0], percentage)).ToString();
            player2Score.text = ((int)Mathf.Lerp(previousScore1, GameManager.Instance.playerScores[1], percentage)).ToString();

            percentage += Time.deltaTime / showcaseSpeed;
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    private IEnumerator ShowPlayerScores2()
    {
        foreach (int score in GameManager.Instance.playerScores)
        {
            Debug.Log(score);
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
}
