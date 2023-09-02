using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RobbingItemIconAnimator : MonoBehaviour
{
    private GameObject playerCam;

    [SerializeField] private float bopHeight;

    private float variation;
    private float time = 0;
    private Vector3 origin;

    private void Awake()
    {
        variation = Random.Range(0, 0.3f);
        origin = transform.position;
    }

    private void Update()
    {
        if (playerCam == null)
        {
            SetPlayerCamera();
            return;
        }

        transform.forward = playerCam.transform.forward;
        transform.position = origin + new Vector3(0, Mathf.PingPong(time, bopHeight + variation), 0);
        time += Time.deltaTime;
    }

    public void SetPlayerCamera()
    {
        playerCam = RobbingManager.Instance.GetPlayerCamera();
    }
}
