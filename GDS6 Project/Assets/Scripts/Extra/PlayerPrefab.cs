using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerPrefab : MonoBehaviour
{
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private Image background;
    [SerializeField] Color normalColour;
    [SerializeField] Color readyColour;

    public void SetName(string name)
    {
        playerName.text = name;
    }

    public string GetName()
    {
        return playerName.text;
    }

    public void SetReady(string readyState)
    {
        if (readyState == "Ready")
        {
            background.color = readyColour;
        }
        else
        {
            background.color = normalColour;
        }
    }
}
