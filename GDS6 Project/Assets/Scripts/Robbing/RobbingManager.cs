using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RobbingManager : NetworkBehaviour
{
    public static RobbingManager Instance { get; private set; }

    public List<GameObject> robbingItems;
    [SerializeField] private int maxActiveItems = 10;

    private GameObject playerCamera;

    private int activeCount;

    private void Awake()
    {
        Instance = this;

        foreach (GameObject item in robbingItems)
        {
            item.GetComponent<RobbingItem>().Hide();
        }
    }
    public void RespawnAllItems()
    {
        if (!IsServer) return;

        foreach (GameObject item in robbingItems)
        {
            item.GetComponent<RobbingItem>().Hide();
        }
        activeCount = 0;

        while (activeCount < maxActiveItems)
        {
            int index = Random.Range(0, robbingItems.Count);
            if (robbingItems[index].GetComponent<RobbingItem>().isActive) continue;

            UpdateItemStateClientRpc(index, true);
            activeCount++;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateItemStateServerRpc(int itemIndex, bool setActive)
    {
        UpdateItemStateClientRpc(itemIndex, setActive);

        if (activeCount == maxActiveItems)
        {
            activeCount--;
            int previousIndex = itemIndex;
            while (activeCount < maxActiveItems)
            {
                int index = Random.Range(0, robbingItems.Count);
                if (robbingItems[index].GetComponent<RobbingItem>().isActive) continue;
                if (index == previousIndex) continue;

                UpdateItemStateClientRpc(index, true);
                activeCount++;
            }
        }
    }

    [ClientRpc]
    private void UpdateItemStateClientRpc(int itemIndex, bool setActive)
    {
        robbingItems[itemIndex].GetComponent<RobbingItem>().SetActiveState(setActive);
    }

    public void SetPlayerCamera(GameObject cam)
    {
        playerCamera = cam;
    }

    public GameObject GetPlayerCamera()
    {
        return playerCamera;
    }
}
