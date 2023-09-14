using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class PingUi : NetworkBehaviour
{
    private TextMeshProUGUI text;
    private float elapsed = 0;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= 1f)
        {
            elapsed %= 1f;
            SetPing();
        }
    }

    private void SetPing()
    {
        text.text = NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.Singleton.LocalClientId).ToString();
    }
}
