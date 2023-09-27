using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private TMP_Text winLoseText;
    [SerializeField] private Button mainMenuButton;
    public static GameOverUI Instance { get; private set; }

    [SerializeField] private CinemachineVirtualCamera cam;
    [SerializeField] float showcaseSpeed = 2;
    public TMP_Text[] players;
    public Transform[] playerScoreText;
    public TextMeshProUGUI[] finalScoreText;

    private int[,] roundScores;

    private void Awake()
    {
        Instance = this;

        mainMenuButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.MainMenu);
        });
    }

    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;

        roundScores = new int[4, 2];

        Hide();
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsGameOver())
        {
            Show();
        }
        else if (GameManager.Instance.IsRoundResetting())
        {
            for (int i = 0; i < GameManager.Instance.playerRoundScores.Count; i++)
            {
                SetRoundScores(GameManager.Instance.GetRoundNumber(), i, GameManager.Instance.playerRoundScores[i]);
            }
            Hide();
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
        mainMenuButton.gameObject.SetActive(false);
        StartCoroutine(ShowPlayerScores());
    }

    private void Hide()
    {
        cam.Priority = 0;
        gameObject.SetActive(false);
    }

    private IEnumerator ShowPlayerScores()
    {
        for (int i = 0; i < playerScoreText.Length; i++)
        {
            float percentage = 0;
            while (percentage < 1)
            {
                playerScoreText[i].GetChild(0).GetComponent<TextMeshProUGUI>().text = ((int)Mathf.Lerp(0, roundScores[i, 0], percentage)).ToString();
                if (GameManager.Instance.playerScores.Count == 2)
                {
                    playerScoreText[i].GetChild(1).GetComponent<TextMeshProUGUI>().text = ((int)Mathf.Lerp(0, roundScores[i, 1], percentage)).ToString();
                }

                percentage += Time.deltaTime / showcaseSpeed;
                yield return new WaitForEndOfFrame();
            }

            playerScoreText[i].GetChild(0).GetComponent<TextMeshProUGUI>().text = roundScores[i, 0].ToString();
            if (GameManager.Instance.playerScores.Count == 2)
            {
                playerScoreText[i].GetChild(1).GetComponent<TextMeshProUGUI>().text = roundScores[i, 1].ToString();
            }

            yield return new WaitForSeconds(0.5f);
        }

        float p = 0;
        while (p < 1)
        {
            finalScoreText[0].text = ((int)Mathf.Lerp(0, GameManager.Instance.playerScores[0], p)).ToString();
            if (GameManager.Instance.playerScores.Count == 2)
            {
                finalScoreText[1].text = ((int)Mathf.Lerp(0, GameManager.Instance.playerScores[1], p)).ToString();
            }

            p += Time.deltaTime / showcaseSpeed;
            yield return new WaitForEndOfFrame();
        }

        finalScoreText[0].text = GameManager.Instance.playerScores[0].ToString();
        if (GameManager.Instance.playerScores.Count == 2)
        {
            finalScoreText[1].text = GameManager.Instance.playerScores[1].ToString();
        }

        CheckDidWin();
        mainMenuButton.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CheckDidWin()
    {
        if (GameManager.Instance.playerScores.Count < 2)
        {
            winLoseText.text = "You Win!";
            return;
        }

        int index = 0;
        foreach (TMP_Text text in players)
        {
            if (text.text == PlayerData.Instance.PlayerName)
            {
                index = System.Array.IndexOf(players, text);
            }
        }

        int decide = GameManager.Instance.playerScores[0] - GameManager.Instance.playerScores[1];
        int winningIndex = 0;
        if (decide < 0)
        {
            winningIndex = 1;
        }
        else if (decide > 0)
        {
            winningIndex = 0;
        }
        else
        {
            winLoseText.text = "Draw!";
            winLoseText.color = new Color(1, 0.64f, 0);
            return;
        }

        winLoseText.text = index == winningIndex ? "You Win!" : "You Lose!";
        winLoseText.color = index == winningIndex ? Color.green : Color.red;
    }

    public void SetPlayerName(int index, string playerName)
    {
        players[index].text = playerName;
    }

    public void SetRoundScores(int round, int playerIndex, int score)
    {
        roundScores[round - 1, playerIndex] = score;
    }
}
