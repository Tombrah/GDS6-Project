using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

public class LightSwitchController : NetworkBehaviour
{
    [SerializeField] private bool isLightOn;
    [SerializeField] private UnityEvent lightOnEvent;
    [SerializeField] private UnityEvent lightOffEvent;
    public void InteractSwitch()
    {
        if(!isLightOn)
        {
            isLightOn = true;
            lightOnEvent.Invoke();
        }
        else
        {
            isLightOn = false;
            lightOffEvent.Invoke();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void InteractSwitchServerRpc()
    {
        InteractSwitchClientRpc();
    }

    [ClientRpc]
    private void InteractSwitchClientRpc()
    {
        InteractSwitch();
    }
}
