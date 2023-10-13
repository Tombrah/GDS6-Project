using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class Dashing : NetworkBehaviour
{
    [SerializeField] private Image fillImage;
    [Header("References")]
    public Transform playerCam;
    private Rigidbody rb;
    private RigidCharacterController pm;

    [Header("Dashing")]
    public float dashForce;
    public float dashUpwardForce;
    public float dashDuration;

    [Header("Settings")]
    public bool allowAllDirections = true;
    public bool disableGravity = false;
    public bool resetVel = true;

    [Header("Cooldown")]
    public float dashCd;
    public float dashCdTimer;

    [Header("Keybinds")]
    public KeyCode dashKey = KeyCode.E;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<RigidCharacterController>();
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying() || !IsOwner) return;

        if(Input.GetKeyDown(dashKey))
        {
            Dash();
        }

        if(dashCdTimer > 0)
        {
            dashCdTimer -= Time.deltaTime;
        }
    }

    private void Dash()
    {
        if (dashCdTimer > 0)
        {
            return;
        }
        else
        {
            dashCdTimer = dashCd;
            StartCoroutine(SetDashUi());
        }
            pm.dashing = true;

        Transform forwardT;

        forwardT = playerCam;

        Vector3 direction = GetDirection(forwardT);

        Vector3 forcetoApply = direction * dashForce + transform.up * dashUpwardForce;

        delayedForceToApply = forcetoApply;
        Invoke(nameof(DelayedDashForce), 0.025f);

        Invoke(nameof(ResetDash), dashDuration);
    }

    private IEnumerator SetDashUi()
    {
        float percentage = 0;

        while (percentage < 1)
        {
            fillImage.fillAmount = -percentage + 1;
            percentage += Time.deltaTime / dashCd;
            yield return new WaitForEndOfFrame();
        }

        fillImage.fillAmount = 0;
        yield return null;
    }

    private Vector3 delayedForceToApply;
    private void DelayedDashForce()
    {
        if(resetVel)
        {
            rb.velocity = Vector3.zero;
        }
        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    private void ResetDash()
    {
        pm.dashing = false;
    }

    private Vector3 GetDirection(Transform forwardT)
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3();

        if (allowAllDirections)
            direction = forwardT.forward * verticalInput + forwardT.right * horizontalInput;
        else
            direction = forwardT.forward;

        if (verticalInput == 0 && horizontalInput == 0)
            direction = forwardT.forward;

        return direction.normalized;
    }
}
