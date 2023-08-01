using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<Color32> characterColours;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            GameObject player = Instantiate(playerPrefab);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            player.GetComponent<Renderer>().material.SetColor("_DiffuseColour", characterColours[(int)clientId]);
        }
    }
}
