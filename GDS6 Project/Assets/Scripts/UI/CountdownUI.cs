using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountdownUI : MonoBehaviour
{
    private TMP_Text text;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;

        Show();
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (!GameManager.Instance.IsCountdownActive())
        {
            Hide();
        }
    }

    private void Update()
    {
        text.text = Mathf.Ceil(GameManager.Instance.GetCountdownTimer()).ToString();
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
