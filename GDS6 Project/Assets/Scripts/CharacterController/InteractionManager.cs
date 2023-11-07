using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class InteractionManager : NetworkBehaviour
{
    public static InteractionManager Instance { get; private set; }

    private bool canCatch = true;
    private bool isStunned;
    private bool canStun = true;

    public NetworkVariable<int> index;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;

        if (IsServer) index.Value = Random.Range(0, GameManager.Instance.respawnPoints.Count);
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsRoundResetting())
        {
            SetIsStunned(false);
            canCatch = true;
            canStun = true;
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void CatchRobberServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (!canCatch) return;

        SetStunClientRpc(false);
        ulong senderID = serverRpcParams.Receive.SenderClientId;
        StartCoroutine(RespawnPlayer(senderID));
    }

    private IEnumerator RespawnPlayer(ulong senderId)
    {
        Debug.Log("catching...");
        canCatch = false;
        ulong robberId = 0;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId != senderId)
            {
                robberId = clientId;
                NetworkObject robber = NetworkManager.Singleton.ConnectedClients[robberId].PlayerObject;
                robber.Despawn();
            }
        }

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { robberId }
            }
        };

        float startScore = GameManager.Instance.playerRoundScores[(int)robberId];
        SetCaughtUiClientRpc(true, robberId, startScore, clientRpcParams);
        GameManager.Instance.playerRoundScores[(int)senderId] += Mathf.CeilToInt(GameManager.Instance.playerRoundScores[(int)robberId] * 0.70f);
        GameManager.Instance.playerRoundScores[(int)robberId] = Mathf.CeilToInt(GameManager.Instance.playerRoundScores[(int)robberId] * 0.30f);

        yield return new WaitForSeconds(2);

        while (Vector3.Distance(NetworkManager.Singleton.ConnectedClients[senderId].PlayerObject.transform.position, GameManager.Instance.respawnPoints[index.Value].position) < 40f)
        {
            index.Value = Random.Range(0, GameManager.Instance.respawnPoints.Count);
        }

        Transform player = Instantiate(GameManager.Instance.playerPrefabs[1], GameManager.Instance.respawnPoints[index.Value].position, GameManager.Instance.respawnPoints[index.Value].rotation);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(robberId, true);
        player.GetComponent<NetworkObject>().ChangeOwnership(robberId);

        SetCaughtUiClientRpc(false, robberId, startScore, clientRpcParams);

        index.Value = Random.Range(0, GameManager.Instance.respawnPoints.Count);
        canCatch = true;
    }

    [ClientRpc]
    private void SetCaughtUiClientRpc(bool isActive, ulong robberId, float startScore, ClientRpcParams clientRpcParams = default)
    {
        if (isActive)
        {
            CaughtUi.Instance.Show(robberId, startScore);
        }
        else
        {
            CaughtUi.Instance.Hide();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetStunServerRpc(bool stunned)
    {
        if (canStun)
        {
            canStun = false;
            SetStunClientRpc(stunned);
        }
    }

    [ClientRpc]
    private void SetStunClientRpc(bool stunned)
    {
        SetIsStunned(stunned);
        canStun = true;
    }

    public bool GetIsStunned()
    {
        return isStunned;
    }

    public void SetIsStunned(bool stunned)
    {
        isStunned = stunned;
    }
}
