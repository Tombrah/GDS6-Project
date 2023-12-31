using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class MovementRobber : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;

    private float walkSpeed;
    private float sprintSpeed;
    private bool readyToJump;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public Transform orientation;
    private bool isGrounded;

    [Header("")]
    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private CinemachineFreeLook CombatCamera;
    [SerializeField] private AudioListener listener;
    [SerializeField] private GameObject chargeWheel;

    private Rigidbody rb;
    private Vector3 moveDirection;

    private float horizontalInput;
    private float verticalInput;

    private GameObject cop;
    private bool canInteract = false;
    [SerializeField] private float robTimer = 3;
    private GameObject robbingItem;
    private Coroutine coroutine;

    public override void OnNetworkSpawn()
    {
        SetSpawn();

        if (IsOwner)
        {
            listener.enabled = true;
            freeLookCamera.Priority = 1;
            CombatCamera.Priority = 1;

            InstructionsUI.Instance.SetText("Hold E near objects to steal them!");
        }
        else
        {
            listener.enabled = false;
            freeLookCamera.Priority = 0;
            CombatCamera.Priority = 0;
            this.enabled = false;
        }
    }

    private void Start()
    {
        rb = gameObject.AddComponent<Rigidbody>();
        gameObject.AddComponent<NetworkRigidbody>();
        rb.isKinematic = false;
        rb.freezeRotation = true;

        readyToJump = true;
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying())
        {
            return;
        }

        RobInteraction();
        // ground check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight / 2 + 0.5f);

        MyInput();
        SpeedControl();

        // handle drag
        if (isGrounded)
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
        if(Input.GetKey(jumpKey) && readyToJump && isGrounded)
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
        if(isGrounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if(!isGrounded)
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
            chargeWheel.SetActive(true);
            coroutine = StartCoroutine(Interact());
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            chargeWheel.SetActive(false);
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
    }

    private IEnumerator Interact()
    {
        Renderer wheelRenderer = chargeWheel.GetComponent<Renderer>();

        float percentage = 0;
        while (percentage < 1)
        {
            rb.velocity = Vector3.zero;
            chargeWheel.transform.LookAt(listener.transform);

            wheelRenderer.material.SetFloat("_Percentage", percentage);
            percentage += Time.deltaTime / robTimer;
            Debug.Log("Interacting...");
            yield return new WaitForEndOfFrame();
        }

        if (robbingItem != null)
        {
            DestroyItemServerRpc(robbingItem.GetComponent<NetworkObject>().NetworkObjectId);
            //GameManager.Instance.UpdatePlayerScoresServerRpc(OwnerClientId, 100);
        }
        chargeWheel.SetActive(false);
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
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            chargeWheel.SetActive(false);
            canInteract = false;
            robbingItem = null;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyItemServerRpc(ulong itemId)
    {
        NetworkManager.Singleton.SpawnManager.SpawnedObjects[itemId].Despawn();
    }

    [ClientRpc]
    public void RespawnPlayerClientRpc()
    {
        if (cop == null)
        {
            cop = GameObject.FindGameObjectWithTag("Cop");
        }

        int index = Random.Range(0, GameManager.Instance.respawnPoints.Count);
        while (Vector3.Distance(cop.transform.position, GameManager.Instance.respawnPoints[index].position) < 15f)
        {
            index = Random.Range(0, GameManager.Instance.respawnPoints.Count);
        }

        transform.position = GameManager.Instance.respawnPoints[index].position;
        transform.rotation = GameManager.Instance.respawnPoints[index].rotation;
    }
}