using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class ThirdPersonCam : NetworkBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;

    public float rotationSpeed;

    public Transform combatLookAt;

    public GameObject thirdPersonCam;
    public GameObject combatCam;

    public CameraStyle currentStyle;
    public enum CameraStyle
    {
        Basic,
        Combat,
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying() || !IsOwner) return;
        // switch styles
        if (combatCam != null)
        {
            if (Input.GetMouseButtonUp(1)) SwitchCameraStyle(CameraStyle.Basic);
            if (Input.GetMouseButtonDown(1)) SwitchCameraStyle(CameraStyle.Combat);
        }

        // rotate orientation
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;

        // rotate player object
        if(currentStyle == CameraStyle.Basic)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if (inputDir != Vector3.zero)
                playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized,Time.deltaTime * rotationSpeed);
        }
    }

    private void FixedUpdate()
    {
        if (currentStyle == CameraStyle.Combat)
        {
            Vector3 dirToCombatLookAt = combatLookAt.position - new Vector3(transform.position.x, combatLookAt.position.y, transform.position.z);
            orientation.forward = dirToCombatLookAt;

            playerObj.forward = dirToCombatLookAt;
        }
    }

    private void SwitchCameraStyle(CameraStyle newStyle)
    {
        thirdPersonCam.GetComponent<CinemachineFreeLook>().Priority = newStyle == CameraStyle.Basic ? 2 : 1;
        combatCam.GetComponent<CinemachineFreeLook>().Priority = newStyle == CameraStyle.Combat ? 2 : 1;

        currentStyle = newStyle;
    }
}
