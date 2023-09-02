using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Components;
using Cinemachine;

public class UpdatedRobberMovement : NetworkBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    [Header("Dashing")]
    public float dashSpeed;
    public float groundDrag;
    public float dashSpeedChangeFactor;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerheight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Check")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("")]
    [SerializeField] private GameObject TPSCamera;
    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private GameObject chargeWheel;

    private GameObject cop;
    private bool canInteract = false;
    [SerializeField] private float robTimer = 3;
    private GameObject robbingItem;
    private Coroutine coroutine;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;
    Dashing dashScript;

    private GameObject robberUI;
    private Image progressImage;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        dashing,
        air
    }

    public bool dashing;

    public override void OnNetworkSpawn()
    {
        SetSpawn();

        if (IsOwner)
        {
            TPSCamera.GetComponent<AudioListener>().enabled = true;
            freeLookCamera.Priority = 1;
            dashScript = GetComponent<Dashing>();
            robberUI = GameManager.Instance.playerUIs[1];
            progressImage = robberUI.GetComponentInChildren<Image>();
            robberUI.SetActive(false);
            GameManager.Instance.playerUIs[0].SetActive(false);
            InstructionsUI.Instance.SetText("Hold E near objects to steal them!");
        }
        else
        {
            TPSCamera.GetComponent<AudioListener>().enabled = false;
            freeLookCamera.Priority = 0;
            GetComponent<Dashing>().enabled = false;
            GetComponentInChildren<LightSwitchRaycast>().enabled = false;
            this.enabled = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            robberUI.SetActive(false);
            GameManager.Instance.OnStateChanged -= Instance_OnStateChanged;
        }
    }

    private void SetSpawn()
    {
        transform.position = GameManager.Instance.playerSpawnPoints[1].position;
        transform.rotation = GameManager.Instance.playerSpawnPoints[1].rotation;
    }

    private void Start()
    {
        GameManager.Instance.OnStateChanged += Instance_OnStateChanged;

        rb = gameObject.AddComponent<Rigidbody>();
        gameObject.AddComponent<NetworkRigidbody>();
        rb.isKinematic = false;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    private void Instance_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsGamePlaying())
        {
            robberUI.SetActive(true);
        }
        else
        {
            robberUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying())
        {
            return;
        }

        SetDashProgress();
        RobInteraction();

        MyInput();
        SpeedControl();
        StateHandler();

        grounded = Physics.Raycast(transform.position, Vector3.down, playerheight * 0.5f + 0.5f);

        if (state == MovementState.walking || state == MovementState.sprinting || state == MovementState.crouching)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }
    }

    private void FixedUpdate()
    {
        if (!GameManager.Instance.IsGamePlaying())
        {
            return;
        }

        MovePlayer();
    }

    private void SetDashProgress()
    {
        float backPercentage = dashScript.dashCdTimer / dashScript.dashCd;
        float percentage = Mathf.Clamp01(1 - backPercentage);

        progressImage.fillAmount = percentage;
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKey(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down, ForceMode.Impulse);
        }

        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private MovementState lastState;
    private bool keepMomentum;
    private void StateHandler()
    {
        //Dashing
        if (dashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }
        //Crouching
        else if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        //Sprinting
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        //Walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        //Air
        else
        {
            state = MovementState.air;

            if (desiredMoveSpeed < sprintSpeed)
            {
                desiredMoveSpeed = walkSpeed;
            }
            else
            {
                desiredMoveSpeed = sprintSpeed;
            }

        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if (lastState == MovementState.dashing)
        {
            keepMomentum = true;
        }

        if (desiredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                moveSpeed = desiredMoveSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;

        //turn off gravity
        rb.useGravity = !OnSlope();
    }

    private float speedChangeFactor;
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        float boostFactor = speedChangeFactor;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            time += Time.deltaTime * boostFactor;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
        keepMomentum = false;
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope())
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
        }
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 100f, ForceMode.Force);
            }
        }
    }

    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }

    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        exitingSlope = true;
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerheight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
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
        robTimer = robbingItem.GetComponent<RobbingItem>().robTimer;

        float percentage = 0;
        while (percentage < 1)
        {
            rb.velocity = Vector3.zero;
            chargeWheel.transform.LookAt(TPSCamera.transform);

            wheelRenderer.material.SetFloat("_Percentage", percentage);
            percentage += Time.deltaTime / robTimer;
            Debug.Log("Interacting...");
            yield return new WaitForEndOfFrame();
        }

        if (robbingItem != null)
        {
            int points = robbingItem.GetComponent<RobbingItem>().points;
            RobbingManager.Instance.UpdateItemStateServerRpc(RobbingManager.Instance.robbingItems.IndexOf(robbingItem), false);
            GameManager.Instance.UpdatePlayerScoresServerRpc(OwnerClientId, points);
        }
        chargeWheel.SetActive(false);
        canInteract = false;
        Debug.Log("Robbing Successful");
        yield return null;
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

    [ClientRpc]
    public void RespawnPlayerClientRpc()
    {
        if (cop == null)
        {
            cop = GameObject.FindGameObjectWithTag("Cop");
        }

        int index = Random.Range(0, GameManager.Instance.respawnPoints.Count);
        while (Vector3.Distance(cop.transform.position, GameManager.Instance.respawnPoints[index].position) < 30f)
        {
            index = Random.Range(0, GameManager.Instance.respawnPoints.Count);
        }

        transform.position = GameManager.Instance.respawnPoints[index].position;
        transform.rotation = GameManager.Instance.respawnPoints[index].rotation;
    }
}

