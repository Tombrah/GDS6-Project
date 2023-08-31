using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerData : NetworkBehaviour
{
    public static PlayerData Instance { get; private set; }

    private Dictionary<ulong, string> playerNames;

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);

        playerNames = new Dictionary<ulong, string>();
    }

    [ServerRpc]
    public void UpdatePlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
    {
        playerNames[serverRpcParams.Receive.SenderClientId] = playerName;
    }

    public Dictionary<ulong, string> GetPlayerNamesDictionary()
    {
        return playerNames;
    }
}
