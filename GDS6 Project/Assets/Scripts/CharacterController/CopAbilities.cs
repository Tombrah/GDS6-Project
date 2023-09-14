using Unity.Netcode;
using UnityEngine;

public class CopAbilities : NetworkBehaviour
{
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private Transform hand;
    [SerializeField] private GameObject zapParticle;
    [SerializeField] private float catchRadius = 3;
    //[SerializeField] private float onCatchTimeReduction = 3;
    //[SerializeField] private int catchPoints = 50;
    [SerializeField] private float ShootCD;

    private GameObject robber;
    private bool canShoot = true;

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying() || !IsOwner) return;

        if (robber == null)
        {
            robber = GameObject.FindWithTag("Robber");
            Debug.Log("Found Robber GameObject");
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
                Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward, Color.blue, 3f);
                Debug.DrawRay(hit.point, hit.point - hand.position, Color.red, 3f);
                Debug.Log("Hit " + hit.collider.gameObject.name);
                GameObject particles = Instantiate(zapParticle, hit.point, Quaternion.identity);
                Destroy(particles, 1.1f);

                if (hit.collider.gameObject.CompareTag("Player"))
                {
                    var player = hit.transform.parent.GetComponent<NetworkObject>();
                    StunRobberServerRpc(player.OwnerClientId);
                }
            }
        }
    }

    private void CatchRobber()
    {
        if (Input.GetKeyDown(KeyCode.E) && robber != null)
        {
            if ((transform.position - robber.transform.GetChild(0).transform.position).sqrMagnitude < catchRadius * catchRadius)
            {
                CatchRobberServerRpc(robber.GetComponent<NetworkObject>().OwnerClientId);
            
                Debug.Log("Capture Successful");
            }
        }
    }

    [ServerRpc]
    private void StunRobberServerRpc(ulong clientId)
    {
        var robberMovement = NetworkManager.Singleton.ConnectedClients[clientId]
            .PlayerObject.GetComponentInChildren<StarterAssets.ThirdPersonController>();

        if (robberMovement != null && !robberMovement.stunned.Value)
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { clientId }
                }
            };

            robberMovement.UpdateStunClientRpc(clientRpcParams);
        }
    }

    [ServerRpc]
    private void CatchRobberServerRpc(ulong clientId, ServerRpcParams serverRpcParams = default)
    {
        var robberAbilities = NetworkManager.Singleton.ConnectedClients[clientId]
            .PlayerObject.GetComponentInChildren<RobberAbilities>();

        if (robberAbilities != null)
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { clientId }
                }
            };
            robberAbilities.RespawnPlayerClientRpc(clientRpcParams);

            int robberFullScore = GameManager.Instance.playerScores[(int)clientId];

            if (robberFullScore == 0) return;
            int newCopScore = Mathf.CeilToInt(robberFullScore * 0.75f);
            int newRobberScore = Mathf.CeilToInt(robberFullScore - newCopScore);

            ulong ownerId = serverRpcParams.Receive.SenderClientId;
            GameManager.Instance.UpdatePlayerScoresServerRpc(ownerId, newCopScore, false);
            GameManager.Instance.UpdatePlayerScoresServerRpc(clientId, newRobberScore, false);
        }

    }
}
