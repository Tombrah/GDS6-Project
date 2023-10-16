using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

public class LightSwitchController : NetworkBehaviour
{
    public static LightSwitchController Instance { get; private set; }

    [SerializeField] private Texture2D[] darkLightmapDir, darkLightmapColour, darkLightmapShadow;
    [SerializeField] private Texture2D[] brightLightmapDir, brightLightmapColour, brightLightmapShadow;

    public bool isLightOn;
    [SerializeField] private UnityEvent lightOnEvent;
    [SerializeField] private UnityEvent lightOffEvent;

    public event EventHandler lightOn;
    public event EventHandler lightOff;

    private LightmapData[] darkLightmap, brightLightmap;

    private Animator animator;

    private void Awake()
    {
        Instance = this;
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;

        List<LightmapData> dLightMap = new List<LightmapData>();

        for (int i = 0; i < darkLightmapDir.Length; i++)
        {
            LightmapData lightmapData = new LightmapData();

            lightmapData.lightmapDir = darkLightmapDir[i];
            lightmapData.lightmapColor = darkLightmapColour[i];
            lightmapData.shadowMask = darkLightmapShadow[i];

            dLightMap.Add(lightmapData);
        }

        darkLightmap = dLightMap.ToArray();

        List<LightmapData> bLightMap = new List<LightmapData>();

        for (int i = 0; i < brightLightmapDir.Length; i++)
        {
            LightmapData lightmapData = new LightmapData();

            lightmapData.lightmapDir = brightLightmapDir[i];
            lightmapData.lightmapColor = brightLightmapColour[i];
            lightmapData.shadowMask = brightLightmapShadow[i];

            bLightMap.Add(lightmapData);
        }

        brightLightmap = bLightMap.ToArray();
    }

    private void GameManager_OnStateChanged(object sender, EventArgs e)
    {
        if (GameManager.Instance.IsRoundResetting())
        {
            if (!isLightOn)
            {
                InteractSwitch();
            }
        }
    }

    public void InteractSwitch()
    {
        if(!isLightOn)
        {
            isLightOn = true;
            lightOnEvent.Invoke();
            lightOn?.Invoke(this, EventArgs.Empty);
            animator.SetBool("LightOn", true);
            LightmapSettings.lightmaps = brightLightmap;
        }
        else
        {
            isLightOn = false;
            lightOffEvent.Invoke();
            lightOff?.Invoke(this, EventArgs.Empty);
            animator.SetBool("LightOn", false);
            LightmapSettings.lightmaps = darkLightmap;
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
