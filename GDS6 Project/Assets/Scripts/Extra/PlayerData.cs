using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;

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

    private async void Start()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            try
            {
                await UnityServices.InitializeAsync();

                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (AuthenticationException e)
            {
                Debug.Log(e);
                Debug.Log("Failed to sign in");
                MessageUi.Instance.ShowMessage("Failed to sign in");
                MessageUi.Instance.restart = true;
            }
        }
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
        return PlayerPrefs.GetFloat(PLAYER_SENSITIVITY_KEY, 1);
    }

    public void SetSensitivity(float newSens)
    {
        PlayerPrefs.SetFloat(PLAYER_SENSITIVITY_KEY, newSens);
    }
}
