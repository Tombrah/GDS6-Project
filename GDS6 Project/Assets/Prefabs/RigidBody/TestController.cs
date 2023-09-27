using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestController : MonoBehaviour
{
    public GameObject mainCamera;

    //Movement
    public float moveSpeed = 7;
    public float sprintSpeed = 10;
    private Rigidbody rb;
    private Vector2 movement;

    //Grounded
    private bool isGrounded;
    private float groundedRadius = 0.5f;
    public LayerMask groundLayers;

    //Rotation
    private float targetRotation = 0.0f;
    private float rotationVelocity;
    [Range(0.0f, 0.3f)]
    public float rotationSmoothTime = 0.12f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        Inputs();
        GroundCheck();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Inputs()
    {
        movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    private void Move()
    {
        Vector3 inputDirection = new Vector3(movement.x, 0, movement.y).normalized;

        if (movement != Vector2.zero)
        {
            targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity,
                rotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0, targetRotation, 0) * Vector3.forward;

        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;

        rb.AddForce(targetDirection.normalized * speed * 10 * Time.deltaTime, ForceMode.Force);
    }

    private void GroundCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y + 0.14f,
                transform.position.z);
        isGrounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers,
            QueryTriggerInteraction.Ignore);
    }
}
