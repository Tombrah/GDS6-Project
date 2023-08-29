using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using Cinemachine;


public class UpdatedCopMovement : NetworkBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;


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
    public KeyCode crouchKey = KeyCode.C;
    public KeyCode shootKey = KeyCode.Mouse0;

    [Header("Ground Check")]
    public float playerheight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Check")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Shooting")]
    [SerializeField] private Transform BulletProjectile;
    [SerializeField] private Transform BulletProjectileTra;
    [SerializeField] private GameObject TPSCamera;
    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private CinemachineFreeLook combatCamera;

    [SerializeField] private float ShootCD;
    private bool canShoot;

    private GameObject robber;
    [SerializeField] private float catchRadius = 3;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
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
            combatCamera.Priority = 1;

            InstructionsUI.Instance.SetText("Left click near the robber to catch them!");
        }
        else
        {
            TPSCamera.GetComponent<AudioListener>().enabled = false;
            freeLookCamera.Priority = 0;
            combatCamera.Priority = 0;
            this.enabled = false;
        }
    }

    private void SetSpawn()
    {
        transform.position = GameManager.Instance.playerSpawnPoints[0].position;
        transform.rotation = GameManager.Instance.playerSpawnPoints[0].rotation;
    }

    private void Start()
    {
        rb = gameObject.AddComponent<Rigidbody>();
        gameObject.AddComponent<NetworkRigidbody>();
        rb.isKinematic = false;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        readyToJump = true;
        canShoot = true;

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying())
        {
            return;
        }

        MyInput();
        SpeedControl();
        StateHandler();

        CatchRobber();
        
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

        if (Input.GetKey(shootKey) && canShoot)
        {
            Vector3 position = TPSCamera.transform.position + TPSCamera.transform.forward * 30f;
            Vector3 aimDir = (position - BulletProjectileTra.position).normalized;
            Instantiate(BulletProjectile, BulletProjectileTra.position, Quaternion.LookRotation(aimDir, Vector3.up));
            StartCoroutine(ShootReset());
        }
    }

    private IEnumerator ShootReset()
    {
        canShoot = false;
        float percentage = 0;
        while (percentage < 1)
        {
            percentage += Time.deltaTime / ShootCD;
            yield return new WaitForEndOfFrame();
        }
        canShoot = true;
    }

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private MovementState lastState;
    private bool keepMomentum;
    private void StateHandler()
    {
        //Crouching
        if (Input.GetKey(crouchKey))
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

    private void CatchRobber()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (robber == null)
            {
                robber = GameObject.FindGameObjectWithTag("Robber");
            }
            if ((transform.position - robber.transform.position).sqrMagnitude < catchRadius * catchRadius)
            {
                CatchRobberServerRpc(robber.GetComponent<NetworkObject>().OwnerClientId);
                GameManager.Instance.UpdatePlayerScoresServerRpc(OwnerClientId, 20);
            }
        }
    }

    [ServerRpc]
    private void CatchRobberServerRpc(ulong clientId)
    {
        NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<MovementRobber>().RespawnPlayerClientRpc();
    }
}
