using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerPointRoundUi : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI points;
    [SerializeField] private TextMeshProUGUI round;

    private int previousScore = 0;
    private float lerpSpeed = 1;

    private bool canIncrease = true;

    private void Start()
    {
        round.text = (GameManager.Instance.GetRoundNumber()) + "/4";
        points.text = GameManager.Instance.playerRoundScores[(int)OwnerClientId].ToString();
    }

    private void Update()
    {
        if (GameManager.Instance.playerRoundScores[(int)OwnerClientId] != previousScore && canIncrease)
        {
            canIncrease = false;
            StartCoroutine(IncrementScore(GameManager.Instance.playerRoundScores[(int)OwnerClientId]));
        }
    }

    private IEnumerator IncrementScore(int endScore)
    {
        float percentage = 0;
        while (percentage < 1)
        {
            points.text = ((int)Mathf.Lerp(previousScore, endScore, percentage)).ToString();

            percentage += Time.deltaTime / lerpSpeed;
            yield return new WaitForEndOfFrame();
        }

        points.text = endScore.ToString();
        previousScore = endScore;
        canIncrease = true;
        yield break;
    }
}
