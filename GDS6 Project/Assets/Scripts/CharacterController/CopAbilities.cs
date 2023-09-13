using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CopAbilities : NetworkBehaviour
{
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private Transform hand;
    [SerializeField] private float catchRadius = 3;
    [SerializeField] private float onCatchTimeReduction = 3;
    [SerializeField] private int catchPoints = 50;
    [SerializeField] private float ShootCD;

    private GameObject robber;
    private bool canShoot = true;

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying() || !IsOwner) return;

        if (robber == null)
        {
            robber = GameObject.FindWithTag("Robber");
        }
        CatchRobber();
        ShootTaser();
    }

    private void ShootTaser()
    {
        if (canShoot && Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            int layerMask = LayerMask.GetMask("Ignore Raycast");

            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 100, ~layerMask))
            {
                Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward);
                Debug.DrawRay(hit.point, hit.point - hand.position);
            }
        }
    }

    private void CatchRobber()
    {
        if (Input.GetKeyDown(KeyCode.E) && robber != null)
        {
            if ((transform.position - robber.transform.position).sqrMagnitude < catchRadius * catchRadius)
            {
                CatchRobberServerRpc(robber.GetComponent<NetworkObject>().OwnerClientId);

                Debug.Log("Capture Successful");
            }
        }
    }

    [ServerRpc]
    private void CatchRobberServerRpc(ulong clientId, ServerRpcParams serverRpcParams = default)
    {
        NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponentInChildren<RobberAbilities>().RespawnPlayerClientRpc();

        ulong ownerId = serverRpcParams.Receive.SenderClientId;

        GameManager.Instance.UpdatePlayerScoresServerRpc(ownerId, catchPoints);
        GameManager.Instance.UpdateGameTimerServerRpc(onCatchTimeReduction);
    }
}
