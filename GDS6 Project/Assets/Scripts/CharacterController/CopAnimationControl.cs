using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopAnimationControl : MonoBehaviour
{
    Animator animator;
    private RigidCharacterController controller;
    // Start is called before the first frame update
    void Start()
    {
        controller = transform.parent.parent.GetComponent<RigidCharacterController>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.GetMoveSpeed() < 0.01)
        {
            animator.SetFloat("Blend", 0);
        }

        if (controller.state == RigidCharacterController.MovementState.walking)
        {
            animator.SetBool("isWalking", true);
        }

        if (controller.state == RigidCharacterController.MovementState.sprinting)
        {
            animator.SetBool("isRunning", true);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger("Jump");
        }
    }
}
