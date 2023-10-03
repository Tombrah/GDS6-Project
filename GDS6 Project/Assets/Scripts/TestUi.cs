using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TestUi : MonoBehaviour
{
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        transform.GetChild(0).gameObject.SetActive(false);
    }
}
