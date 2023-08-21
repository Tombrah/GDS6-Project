using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class PlayerMovementCop : NetworkBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float crouchSpeed = 3f;

    [Header("Jumping")]
    public float jumpForce = 5f;
    public float gravity = -9.81f;
    private Vector3 velocity;

    [Header("Mouse")]
    public float mouseSensitivity = 50f;

    [Header("Crouching")]
    public float crouchHeight = 1f;
    public float playerHeight = 2f;
    public float crouchRate = 0.1f;
    private bool detectRoof;

    [Header("Dashing")]
    public float dashSpeed = 10f;
    public float dashTime = 0.5f;
    public float dashCooldown = 1.5f;

    [Header("Is Grounded")]
    public float checkRadius = 0.4f;
    public LayerMask groundLayer;
    private bool isGrounded;

    [Header("")]
    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private AudioListener listener;
    private CharacterController controller;

    private GameObject robber;
    [SerializeField] private float catchRadius = 3;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        controller = GetComponent<CharacterController>();

        controller.height = playerHeight;

        controller.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        SetSpawn();

        if (IsOwner)
        {
            controller.enabled = true;
            listener.enabled = true;
            freeLookCamera.Priority = 1;
        }
        else
        {
            listener.enabled = false;
            freeLookCamera.Priority = 0;
            this.enabled = false;
        }
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying())
        {
            return;
        }

        isGrounded = Physics.Raycast(transform.position, Vector3.down, controller.height / 2 + 0.5f);

        PlayerMove();
        CatchRobber();

        if (isGrounded)
        {
            PlayerJump();
            PlayerCrouch();
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -3f;
        }

    }

    private void PlayerMove()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * walkSpeed * Time.deltaTime);
    }

    private void PlayerJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2 * gravity);
        }
    }

    private void PlayerCrouch()
    {
        detectRoof = Physics.Raycast(transform.position, Vector3.up, playerHeight);
        if (Input.GetKey(KeyCode.LeftShift))
        {
            controller.height -= crouchRate;
        }
        else if (!detectRoof)
        {
            controller.height += crouchRate;
        }
        controller.height = Mathf.Clamp(controller.height, crouchHeight, playerHeight);
    }

    private void SetSpawn()
    {
        transform.position = GameManager.Instance.playerSpawnPoints[0].position;
        transform.rotation = GameManager.Instance.playerSpawnPoints[0].rotation;
    }

    private void CatchRobber()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if ((transform.position - robber.transform.position).sqrMagnitude < catchRadius * catchRadius)
            {
                Debug.Log("Player Caught");
            }
        }
    }
}
