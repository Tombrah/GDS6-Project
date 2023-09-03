using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyCountdownUI : MonoBehaviour
{
    public static LobbyCountdownUI Instance { get; private set; }

    private TMP_Text text;

    private void Awake()
    {
        Instance = this;
        text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (LobbyManager.Instance.GetCountdownTimer() <= 3f)
        {
            text.text = Mathf.Ceil(LobbyManager.Instance.GetCountdownTimer()).ToString();
        }
        else
        {
            text.text = "";
        }
    }
}
