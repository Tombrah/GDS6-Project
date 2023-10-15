using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class InteractionManager : NetworkBehaviour
{
    public static InteractionManager Instance { get; private set; }

    public NetworkVariable<bool> isCaught;
    private bool isStunned;
    private bool canStun = true;
    private bool canCatch = true;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsRoundResetting())
        {
            isCaught.Value = false;
            SetIsStunned(false);
            canCatch = true;
            canStun = true;
        }
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

            isCaught.Value = true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetCaughtServerRpc(bool caught)
    {
        isCaught.Value = caught;
        if (!caught) canCatch = true;
        SetStunClientRpc(false);
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
        return isCaught.Value;
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
