using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    private void Update()
    {
        if (Input.anyKey)
        {
            Loader.Load(Loader.Scene.LobbyScene);
        }
    }
}
