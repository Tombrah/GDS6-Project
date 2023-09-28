using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Flashlight : NetworkBehaviour
{
    public GameObject flashlight;
    public GameObject flashlightUi;

    public bool on;
    public bool off;

    public KeyCode flashlightKey = KeyCode.F;

    void Start()
    {
        off = true;
        flashlight.SetActive(false);
        flashlightUi.SetActive(false);
    }

    void Update()
    {
        if (!IsOwner) return;

        if (off && Input.GetKeyDown(flashlightKey))
        {
            flashlight.SetActive(true);
            flashlightUi.SetActive(true);
            off = false;
            on = true;
            UpdateFlashlightServerRpc(true);
        }
        else if (on && Input.GetKeyDown(flashlightKey))
        {
            flashlight.SetActive(false);
            flashlightUi.SetActive(false);
            off = true;
            on = false;
            UpdateFlashlightServerRpc(false);
        }
    }

    [ServerRpc]
    private void UpdateFlashlightServerRpc(bool activeState)
    {
        UpdateFlashlightClientRpc(activeState);
    }

    [ClientRpc]
    private void UpdateFlashlightClientRpc(bool activeState)
    {
        if (IsOwner) return;
        flashlight.SetActive(activeState);
    }
}