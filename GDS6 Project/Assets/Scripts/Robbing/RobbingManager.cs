using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RobbingManager : NetworkBehaviour
{
    public static RobbingManager Instance { get; private set; }

    public List<GameObject> robbingItems;
    [SerializeField] private int maxActiveItems = 5;
    [SerializeField] private bool isQuickTimeRandom = true;
    [Range(0,1), Tooltip("Percentage chance of an object being a quick time event")]
    [SerializeField] private float quickTimePercentage = 0.75f;

    private GameObject playerCamera;

    private int activeCount;

    private void Awake()
    {
        Instance = this;

        foreach (GameObject item in robbingItems)
        {
            item.SetActive(false);
        }
    }
    public void RespawnAllItems()
    {
        if (!IsServer) return;

        foreach (GameObject item in robbingItems)
        {
            item.SetActive(false);
        }
        activeCount = 0;

        while (activeCount < maxActiveItems)
        {
            int index = Random.Range(0, robbingItems.Count);
            if (robbingItems[index].activeSelf) continue;

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
                if (robbingItems[index].activeSelf) continue;
                if (index == previousIndex) continue;

                UpdateItemStateClientRpc(index, true);
                activeCount++;
            }
        }
    }

    [ClientRpc]
    private void UpdateItemStateClientRpc(int itemIndex, bool setActive)
    {
        robbingItems[itemIndex].GetComponent<RobbingItem>().SetActiveState(setActive, isQuickTimeRandom,  1 - quickTimePercentage);
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
