using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class StartCameraController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private CinemachineVirtualCamera vCam;

    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsCountdownActive())
        {
            vCam.Priority = 0;
        }
        else if (GameManager.Instance.IsGamePlaying())
        {
            cam.GetComponent<CinemachineBrain>().m_DefaultBlend.m_Time = 0.2f;
            Debug.Log("Camera set to smooth");
        }
        else
        {
            vCam.Priority = 10;
            cam.GetComponent<CinemachineBrain>().m_DefaultBlend.m_Time = 0;
        }
    }
}
