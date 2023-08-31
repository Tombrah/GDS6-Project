using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InstructionsUI : MonoBehaviour
{
    public static InstructionsUI Instance { get; private set; }

    private TMP_Text text;

    private void Awake()
    {
        Instance = this;
        text = GetComponentInChildren<TMP_Text>();
    }

    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;

        Hide();
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsCountdownActive())
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
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetText(string inputText)
    {
        text.text = inputText;
    }
}
