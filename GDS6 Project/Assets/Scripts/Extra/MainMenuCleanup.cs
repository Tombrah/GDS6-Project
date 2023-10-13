using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MainMenuCleanup : MonoBehaviour
{
    public bool ResetPlayerData;

    private void Awake()
    {
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }
    }

    private void Start()
    {
        if (ResetPlayerData)
        {
            PlayerPrefs.DeleteAll();
        }
    }
}
