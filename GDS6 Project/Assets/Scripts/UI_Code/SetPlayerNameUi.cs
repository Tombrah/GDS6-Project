using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SetPlayerNameUi : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button enterButton;
    [SerializeField] private InitialUi initialUi;

    private void Awake()
    {
        enterButton.onClick.AddListener(() =>
        {
            if (string.IsNullOrWhiteSpace(nameInputField.text))
            {
                MessageUi.Instance.ShowMessage("Must input a valid name!", true);
                nameInputField.text = null;
            }
            else
            {
                PlayerData.Instance.SetPlayerName(nameInputField.text);
                initialUi.Show();
                Hide();
            }
        });
    }

    private void Start()
    {
        if (!string.IsNullOrWhiteSpace(PlayerData.Instance.GetPlayerName()))
        {
            Hide();
        }
        Debug.Log(PlayerData.Instance.GetPlayerName());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (string.IsNullOrWhiteSpace(nameInputField.text))
            {
                MessageUi.Instance.ShowMessage("Must input a valid name!", true);
                nameInputField.text = null;
            }
            else
            {
                PlayerData.Instance.SetPlayerName(nameInputField.text);
                initialUi.Show();
                Hide();
            }
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
