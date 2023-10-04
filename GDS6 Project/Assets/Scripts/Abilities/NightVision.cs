using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NightVision : NetworkBehaviour
{
    [SerializeField] private GameObject nightVision;
    [SerializeField] private GameObject progress;

    private bool active = false;

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying() || !IsOwner) return;

        if (Input.GetKeyDown(KeyCode.G))
        {
            active = !active;
            nightVision.SetActive(active);
            progress.SetActive(active);
        }
    }
}
