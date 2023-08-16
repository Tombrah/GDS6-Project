using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.Netcode;
using TMPro;

public class PlayerMovementTutorial : NetworkBehaviour
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

    private bool canInteract = true;
    [SerializeField] private float robTimer = 3;

    Vector3 moveDirection;

    Rigidbody rb;

    [SerializeField] private List<Color> characterColours;

    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private AudioListener listener;

    public string[] roles = { "Cop", "Robber" };

    public enum Role
    {
        Cop,
        Robber
    }
    public Role playerRole;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
    }

    public override void OnNetworkSpawn()
    {
        //AssignRole(roles[(int)OwnerClientId]);

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

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying())
        {
            return;
        }

        RobInteraction();
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight / 2 + 0.5f);

        MyInput();
        SpeedControl();

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

    private void RobInteraction()
    {
        if (canInteract && Input.GetKeyDown(KeyCode.E))
        {
            canInteract = false;
            StartCoroutine(Interact());
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            canInteract = true;
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

        canInteract = true;
        Debug.Log("Robbing Successful");
        yield return null;
    }

    public void AssignRole(int roleId)
    {
        switch ((Role)roleId)
        {
            case Role.Cop:
                playerRole = Role.Cop;

                transform.position = GameManager.Instance.playerSpawnPoints[0].position;
                transform.rotation = GameManager.Instance.playerSpawnPoints[0].rotation;

                Material mat1 = new Material(gameObject.GetComponent<Renderer>().material);
                mat1.SetColor("_DiffuseColour", characterColours[0]);
                gameObject.GetComponent<Renderer>().material = mat1;
                break;
            case Role.Robber:
                playerRole = Role.Robber;

                transform.position = GameManager.Instance.playerSpawnPoints[1].position;
                transform.rotation = GameManager.Instance.playerSpawnPoints[1].rotation;

                Material mat2 = new Material(gameObject.GetComponent<Renderer>().material);
                mat2.SetColor("_DiffuseColour", characterColours[1]);
                gameObject.GetComponent<Renderer>().material = mat2;
                break;
            default:
                break;
        }
    }
}