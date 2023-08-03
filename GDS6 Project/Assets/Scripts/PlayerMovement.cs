using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
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
    private float xRotation = 0f;

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
    private bool isGrounded;
    public float checkRadius = 0.4f;
    public LayerMask groundLayer;

    [SerializeField] private List<Color32> characterColours;

    private Camera cam;
    private CharacterController controller;



    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        controller = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();

        controller.height = playerHeight;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            cam.enabled = false;
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

        MouseLook();
        PlayerMove();

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

        if (Input.GetMouseButtonDown(1) && controller.height == playerHeight)
        {
            StartCoroutine(PlayerDash());
        }

    }

    private void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.Rotate(Vector3.up * mouseX);
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
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

    IEnumerator PlayerDash()
    {
        float startTime = Time.time;
        while (Time.time < startTime + dashTime)
        {
            controller.Move(transform.forward * dashSpeed * Time.deltaTime);

            yield return null;
        }
    }
}
