using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class BulletProjectile : NetworkBehaviour
{
    [SerializeField] private float speed = 100f;
    [SerializeField] private float stunTimer;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            this.enabled = false;
        }
    }

    private void Start()
    {
        DestroyBulletAfterSpawnServerRpc(1);
    }

    private void Update()
    {
        transform.position += speed * Time.deltaTime * transform.forward;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        Debug.Log("Collided With: " + other.gameObject.name);

        if (other.CompareTag("Robber"))
        {
            Debug.Log("Hit the robber");
            StunRobberServerRpc(other.gameObject.GetComponent<NetworkObject>().OwnerClientId);
            DestroyBulletServerRpc();
        }
    }

    [ServerRpc]
    private void StunRobberServerRpc(ulong clientId)
    {
        NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<UpdatedRobberMovement>().GetTasedClientRpc(stunTimer);
    }

    [ServerRpc]
    private void DestroyBulletServerRpc()
    {
        gameObject.GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }

    [ServerRpc]
    private void DestroyBulletAfterSpawnServerRpc(float destroyTimer)
    {
        DestroyBullet(destroyTimer);
    }

    private IEnumerator DestroyBullet(float destroyTimer)
    {
        float timeElapsed = 0;
        while (timeElapsed < destroyTimer)
        {
            timeElapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        gameObject.GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}
