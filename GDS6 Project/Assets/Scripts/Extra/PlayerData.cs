using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    private const string PLAYER_NAME_KEY = "Name";
    private const string PLAYER_SENSITIVITY_KEY = "Sensitivity";

    public static PlayerData Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public string GetPlayerName()
    {
        return PlayerPrefs.GetString(PLAYER_NAME_KEY);
    }

    public void SetPlayerName(string name)
    {
        PlayerPrefs.SetString(PLAYER_NAME_KEY, name);
    }

    public float GetSensitivity()
    {
        return PlayerPrefs.GetFloat(PLAYER_SENSITIVITY_KEY);
    }

    public void SetSensitivity(float newSens)
    {
        PlayerPrefs.SetFloat(PLAYER_SENSITIVITY_KEY, newSens);
    }
}
