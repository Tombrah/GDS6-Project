using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BulletProjectile : NetworkBehaviour
{
    private Rigidbody bulletRigidbody;
    [SerializeField] private float stunTimer;

    private void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
        Destroy(gameObject, 0.5f);
    }

    private void Start()
    {
        float speed = 100f;
        bulletRigidbody.velocity = transform.forward * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Robber"))
        {
            Debug.Log("Hit the robber");
            StunRobberServerRpc(other.gameObject.GetComponent<NetworkObject>().OwnerClientId);
            DespawnBulletServerRpc();
        }
        if(!other.CompareTag("Robber") || ! other.CompareTag("Cop"))
        {
            DespawnBulletServerRpc();
        }    
    }

    [ServerRpc]
    private void StunRobberServerRpc(ulong clientId)
    {
        NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<UpdatedRobberMovement>().GetTasedClientRpc(stunTimer);
    }

    [ServerRpc]
    private void DespawnBulletServerRpc()
    {
        gameObject.GetComponent<NetworkObject>().Despawn();
    }
}
