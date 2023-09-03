using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class PlayerPrefab : MonoBehaviour
{
    [SerializeField] private TMP_Text playerName;

    public void SetName(string name)
    {
        playerName.text = name;
    }
}
