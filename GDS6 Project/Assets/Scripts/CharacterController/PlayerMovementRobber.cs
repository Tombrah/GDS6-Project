using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class PlayerMovementRobber : NetworkBehaviour
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

    private bool canInteract = false;
    [SerializeField] private float robTimer = 3;
    private GameObject robbingItem;

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
        RobInteraction();

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

    private void RobInteraction()
    {
        if (canInteract && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(Interact());
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            StopCoroutine(Interact());
        }
    }

    private IEnumerator Interact()
    {
        float percentage = 0;
        while (percentage < 1)
        {
            percentage += Time.deltaTime / robTimer;
            Debug.Log("Interacting...");
            yield return new WaitForEndOfFrame();
        }

        DestroyItemServerRpc(robbingItem.GetComponent<NetworkObject>().NetworkObjectId);
        canInteract = false;
        Debug.Log("Robbing Successful");
        yield return null;
    }

    private void SetSpawn()
    {
        transform.position = GameManager.Instance.playerSpawnPoints[1].position;
        transform.rotation = GameManager.Instance.playerSpawnPoints[1].rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectable"))
        {
            Debug.Log("Can Interact");
            canInteract = true;
            robbingItem = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Collectable"))
        {
            Debug.Log("Can't Interact");
            canInteract = false;
            robbingItem = null;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyItemServerRpc(ulong itemId)
    {
        NetworkManager.Singleton.SpawnManager.SpawnedObjects[itemId].Despawn();
    }
}
