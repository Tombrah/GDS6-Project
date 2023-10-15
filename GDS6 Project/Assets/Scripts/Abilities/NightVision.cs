using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NightVision : NetworkBehaviour
{
    [SerializeField] private GameObject nightVision;
    [SerializeField] private GameObject fillImage;
    [SerializeField] private AudioSource goggleAudio;

    private bool active = false;

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying() || !IsOwner) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            active = !active;
            float pitch = active ? 1 : 0.8f;
            goggleAudio.pitch = pitch;
            goggleAudio.Play();
            nightVision.SetActive(active);
            fillImage.SetActive(!active);
        }
    }
}
