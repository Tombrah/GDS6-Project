using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CaughtUi : MonoBehaviour
{
    public static CaughtUi Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI text;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;

        Hide();
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        Hide();
    }

    public void Show(ulong clientId, float startScore)
    {
        gameObject.SetActive(true);
        StartCoroutine(DeductPoints(clientId, startScore));
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator DeductPoints(ulong clientId, float startScore)
    {
        float percentage = 0;
        float newPoints = startScore * 0.30f;
        while (percentage < 1)
        {
            text.text = ((int)Mathf.Lerp(startScore, newPoints, percentage)).ToString();         

            percentage += Time.deltaTime / 1;
            yield return new WaitForEndOfFrame();
        }

        text.text = newPoints.ToString();
        yield return null;
    }
}
