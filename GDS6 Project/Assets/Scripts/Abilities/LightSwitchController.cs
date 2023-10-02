using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

public class LightSwitchController : NetworkBehaviour
{
    [SerializeField] private Texture2D _dir;
    [SerializeField] private Texture2D _light;
    [SerializeField] private Texture2D _shadow;

    [SerializeField] private bool isLightOn;
    [SerializeField] private UnityEvent lightOnEvent;
    [SerializeField] private UnityEvent lightOffEvent;

    private LightmapData[] lightsOnData;
    private LightmapData[] lightsOffData;

    private void Start()
    {
        lightsOnData = LightmapSettings.lightmaps;

        LightmapData mapdata = new LightmapData();
        for (var i = 0; i < lightsOnData.Length; i++)
        {
            mapdata.lightmapDir = _dir;
            mapdata.lightmapColor = _light;
            mapdata.shadowMask = _shadow;

            lightsOffData[i] = mapdata;
        }
    }
    public void InteractSwitch()
    {
        if(!isLightOn)
        {
            isLightOn = true;
            lightOnEvent.Invoke();
            LightmapSettings.lightmaps = lightsOnData;
        }
        else
        {
            isLightOn = false;
            lightOffEvent.Invoke();
            LightmapSettings.lightmaps = lightsOffData;
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
