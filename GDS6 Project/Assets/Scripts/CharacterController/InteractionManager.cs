using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class InteractionManager : NetworkBehaviour
{
    public static InteractionManager Instance { get; private set; }

    private bool isStunned;
    private bool isCaught;
    private bool canStun = true;
    private bool canCatch = true;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SetIsStunned(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CatchRobberServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (canCatch)
        {
            ulong senderId = serverRpcParams.Receive.SenderClientId;
            canCatch = false;

            int robberPoints = 0;
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (clientId != senderId)
                {
                    robberPoints = GameManager.Instance.playerRoundScores[(int)clientId];
                }
            }

            int copPoints = Mathf.CeilToInt(robberPoints * 0.75f);
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (clientId == senderId)
                {
                    GameManager.Instance.playerRoundScores[(int)clientId] += copPoints;
                }
                else
                {
                    GameManager.Instance.playerRoundScores[(int)clientId] -= copPoints;
                }
            }

            SetCaughtClientRpc(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetCaughtServerRpc(bool caught)
    {
        SetCaughtClientRpc(caught);
    }

    [ClientRpc]
    private void SetCaughtClientRpc(bool caught)
    {
        if (!caught) canCatch = true;
        SetIsCaught(caught);
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

    public bool GetIsCaught()
    {
        return isCaught;
    }

    public void SetIsCaught(bool caught)
    {
        isCaught = caught;
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
