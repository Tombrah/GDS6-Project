using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    private void Update()
    {
        if (Input.anyKeyDown)
        {
            PlayerPrefs.DeleteAll();
            Loader.Load(Loader.Scene.LobbyScene);
        }
    }
}
