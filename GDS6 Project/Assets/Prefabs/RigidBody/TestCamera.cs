using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class TestCamera : NetworkBehaviour
{
    [SerializeField] private GameObject cinemachineTarget;
    public GameObject thirdPersonCam;
    public GameObject combatCam;
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;

    private float topClamp = 70.0f;
    private float bottomClamp = -30.0f;

    public float normalSensitivity;
    public float aimSensitivity;
    private float sensitivity;

    public CameraStyle currentStyle;
    public enum CameraStyle
    {
        Basic,
        Combat,
    }

    private void Start()
    {
        cinemachineTargetYaw = cinemachineTarget.transform.eulerAngles.y;
        sensitivity = normalSensitivity;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying() || !IsOwner) return;

        if (combatCam != null)
        {
            if (Input.GetMouseButtonUp(1)) SwitchCameraStyle(CameraStyle.Basic);
            if (Input.GetMouseButtonDown(1)) SwitchCameraStyle(CameraStyle.Combat);
        }
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;

        RotateCamera();
    }

    private void RotateCamera()
    {
        cinemachineTargetYaw += Input.GetAxis("Mouse X") * sensitivity;
        cinemachineTargetPitch -= Input.GetAxis("Mouse Y") * sensitivity;

        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, bottomClamp, topClamp);

        // Cinemachine will follow this target
        cinemachineTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch, cinemachineTargetYaw, 0.0f);
    }

    private void SwitchCameraStyle(CameraStyle newStyle)
    {
        thirdPersonCam.GetComponent<CinemachineVirtualCamera>().Priority = newStyle == CameraStyle.Basic ? 2 : 1;
        combatCam.GetComponent<CinemachineVirtualCamera>().Priority = newStyle == CameraStyle.Combat ? 2 : 1;

        sensitivity = newStyle == CameraStyle.Basic ? normalSensitivity : aimSensitivity;

        currentStyle = newStyle;
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}
