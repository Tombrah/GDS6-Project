using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.Netcode;
using TMPro;

public class MovementCop : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [HideInInspector] public float walkSpeed;
    [HideInInspector] public float sprintSpeed;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private AudioListener listener;

    private GameObject robber;
    [SerializeField] private float catchRadius = 3;

    public override void OnNetworkSpawn()
    {
        SetSpawn();

        if (IsOwner)
        {
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
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.freezeRotation = true;

        readyToJump = true;

        robber = GameObject.FindGameObjectWithTag("Robber");
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying())
        {
            return;
        }

        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight / 2 + 0.5f);

        MyInput();
        SpeedControl();

        CatchRobber();

        // handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        if (!GameManager.Instance.IsGamePlaying())
        {
            return;
        }
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on ground
        if(grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
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