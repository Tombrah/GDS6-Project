using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Components;
using Cinemachine;

public class RigidCharacterController : NetworkBehaviour
{
    public int Id;
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    [Header("Dashing")]
    public float dashSpeed;
    public float dashSpeedChangeFactor;

    [Header("Drag")]
    public float groundDrag;
    public float slopeDrag;

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

    [Header("Ground Check")]
    public float playerheight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Check")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject TPSCamera;
    [SerializeField] private CinemachineVirtualCamera basicCam;
    [SerializeField] private CinemachineVirtualCamera combatCam;

    [SerializeField] private float stunTimer = 3;
    [SerializeField] private AudioSource stunAudio;

    private Vector2 movement;
    Vector3 moveDirection;

    //Rotation
    private float targetRotation = 0.0f;
    private float rotationVelocity;
    [Range(0.0f, 0.3f)]
    public float rotationSmoothTime = 0.12f;
    private float blend = 0;
    public float blendSpeed = 1f;

    Rigidbody rb;
    Dashing dashScript;

    [Header("UI")]
    public GameObject playerUi;
    public GameObject postProcess;
    public GameObject ruleUi;
    public GameObject zapParticle;

    [Header("Footsteps")]
    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;

    public MovementState state;
    public enum MovementState
    {
        idle,
        walking,
        sprinting,
        crouching,
        dashing,
        air,
        stunned,
        aiming
    }

    public bool dashing;
    private bool stunned;
    private bool triggerOnce = true;

    public override void OnNetworkSpawn()
    {
        if (GameManager.Instance.IsGamePlaying())
        {
            SetRespawn(InteractionManager.Instance.index.Value);
            ruleUi.SetActive(false);
        }
        else
        {
            SetSpawn();
        }

        if (IsOwner)
        {
            Debug.Log("I own: " + transform.parent.name);
            TPSCamera.GetComponent<AudioListener>().enabled = true;
            basicCam.Priority = 2;
            if (combatCam != null)
            {
                combatCam.Priority = 1;
            }

            if(RobbingManager.Instance != null) RobbingManager.Instance.SetPlayerCamera(TPSCamera);

            playerUi.SetActive(GameManager.Instance.IsGamePlaying());
        }
        else
        {
            ruleUi.SetActive(false);
            playerUi.SetActive(false);
        }
    }

    private void Start()
    {
        if (!IsOwner) return;

        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;

        rb = gameObject.AddComponent<Rigidbody>();
        gameObject.AddComponent<NetworkRigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            GameManager.Instance.OnStateChanged -= GameManager_OnStateChanged;
        }
    }

    private void SetSpawn()
    {
        transform.parent.SetPositionAndRotation(GameManager.Instance.playerSpawnPoints[Id].position, GameManager.Instance.playerSpawnPoints[Id].rotation);
    }

    public void SetRespawn(int index)
    {
        transform.parent.SetPositionAndRotation(GameManager.Instance.respawnPoints[index].position, GameManager.Instance.respawnPoints[index].rotation);
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsGamePlaying())
        {
            playerUi.SetActive(true);
            ruleUi.SetActive(false);
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        grounded = Physics.Raycast(transform.position, Vector3.down, playerheight * 0.5f + 0.3f, whatIsGround);
        animator.SetBool("Grounded", grounded);

        if (!GameManager.Instance.IsGamePlaying()) return;

        CheckStun();

        MyInput();
        StateHandler();


        if (OnSlope())
        {
            rb.drag = slopeDrag;
        }
        else if (grounded)
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
        if (!GameManager.Instance.IsGamePlaying() || !IsOwner) return;

        MovePlayer();
        SpeedControl();
    }

    private void MyInput()
    {
        if (PauseUi.IsPaused)
        {
            movement = Vector2.zero;
            return;
        }

        movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetKey(jumpKey) && readyToJump && grounded && !stunned)
        {
            readyToJump = false;

            Jump();
            animator.SetTrigger("Jump");

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //if (Input.GetKey(crouchKey))
        //{
        //    transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        //    rb.AddForce(Vector3.down, ForceMode.Impulse);
        //}
        //
        //if (Input.GetKeyUp(crouchKey))
        //{
        //    transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        //}

        if (Id == 1) return;

        if (Input.GetMouseButtonDown(1))
        {
            animator.SetLayerWeight(animator.GetLayerIndex("Aiming"), 1);
        }
        if (Input.GetMouseButtonUp(1))
        {
            animator.SetLayerWeight(animator.GetLayerIndex("Aiming"), 0);
        }
    }

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private MovementState lastState;
    private bool keepMomentum;
    private void StateHandler()
    {
        if (stunned)
        {
            state = MovementState.stunned;
            moveSpeed = 0;
            desiredMoveSpeed = 0;
        }
        //Aiming
        else if (TPSCamera.GetComponent<TestCamera>().currentStyle == TestCamera.CameraStyle.Combat)
        {
            state = MovementState.aiming;
            desiredMoveSpeed = movement != Vector2.zero ? crouchSpeed : 0;
            
        }
        //Dashing
        else if (dashing)
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
        else if (grounded && Input.GetKey(sprintKey) && movement != Vector2.zero)
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        //Walking
        else if (grounded && movement != Vector2.zero)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        //Idle
        else if (grounded)
        {
            state = MovementState.idle;
            desiredMoveSpeed = 0;
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

        blend = Mathf.Lerp(blend, desiredMoveSpeed, Time.deltaTime * blendSpeed);
        if (blend < 0.01f) blend = 0;

        animator.SetFloat("Blend", blend);

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;

        //turn off gravity

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

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    private void MovePlayer()
    {
        RotatePlayer();

        moveDirection = movement != Vector2.zero? Quaternion.Euler(0, targetRotation, 0) * Vector3.forward : Vector3.zero;

        if (OnSlope())
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 100f, ForceMode.Force);
            }
        }
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        rb.useGravity = !OnSlope();
    }

    private void RotatePlayer()
    {
        Vector3 inputDirection = new Vector3(movement.x, 0, movement.y).normalized;

        if (movement != Vector2.zero && TPSCamera.GetComponent<TestCamera>().currentStyle == TestCamera.CameraStyle.Basic)
        {
            targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              TPSCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity,
                rotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }
        else if (TPSCamera.GetComponent<TestCamera>().currentStyle == TestCamera.CameraStyle.Combat)
        {
            Vector3 shoulder = TPSCamera.transform.position + TPSCamera.transform.forward * 4.5f;
            Vector3 dirToShoulder = shoulder - new Vector3(TPSCamera.transform.position.x, shoulder.y, TPSCamera.transform.position.z);
            transform.forward = dirToShoulder;
            targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              TPSCamera.transform.eulerAngles.y;
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
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerheight * 0.5f + 0.2f, whatIsGround))
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

    private void CheckStun()
    {
        if (InteractionManager.Instance == null || Id == 0) return;

        stunned = InteractionManager.Instance.GetIsStunned();
        if (stunned && triggerOnce)
        {
            stunAudio.Play();
            animator.SetTrigger("Stun");
            StartCoroutine(ResetStun());
            Debug.Log("Stun Coroutine Started");
        }
    }

    private IEnumerator ResetStun()
    {
        GameObject zap = Instantiate(zapParticle, transform.position, Quaternion.identity);
        Destroy(zap, 1.5f);
        triggerOnce = false;

        yield return new WaitForSeconds(stunTimer);

        InteractionManager.Instance.SetStunServerRpc(false);
        InteractionManager.Instance.SetIsStunned(false);
        triggerOnce = true;

        Debug.Log("Player is no longer stunned");
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.position, 1);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.position, 1);
        }
    }
}