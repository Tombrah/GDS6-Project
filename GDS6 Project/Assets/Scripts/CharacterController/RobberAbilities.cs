using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class RobberAbilities : NetworkBehaviour
{
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject chargeWheel;
    [SerializeField] private GameObject buttonPrompt;
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float interactionRadius = 0.5f;
    [SerializeField] private float robTimer = 1;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private AudioSource stealAudio;

    private readonly Collider[] colliders = new Collider[3];
    private int numFound;

    private GameObject cop;
    private GameObject robbingItem;
    private Coroutine coroutine;
    private bool isRunning = false;

    public Animator animator;

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying() || !IsOwner) return;

        if (cop == null)
        {
            cop = GameObject.FindWithTag("Cop");
        }

        if (InteractionManager.Instance != null && InteractionManager.Instance.GetIsStunned())
        {
            if (coroutine != null) StopCoroutine(coroutine);
            return;
        }

        numFound = Physics.OverlapSphereNonAlloc(interactionPoint.position, interactionRadius, colliders, interactableMask, QueryTriggerInteraction.Collide);
        if (numFound > 0)
        {
            robbingItem = colliders[0].gameObject;
            if (robbingItem != null && robbingItem.GetComponent<RobbingItem>().isActive)
            {
                buttonPrompt.SetActive(!isRunning);
                buttonPrompt.transform.LookAt(playerCamera.transform);
                RobInteraction();
            }
        }
        else
        {
            if (buttonPrompt.activeSelf) buttonPrompt.SetActive(false);
            if (robbingItem != null) robbingItem = null;
            if (isRunning) StopCoroutine(coroutine);
            isRunning = false;
            animator.SetBool("Stealing", false);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(interactionPoint.position, interactionRadius);
    }

    private void RobInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            coroutine = StartCoroutine(Interact());
        }

        if (Input.GetKeyUp(KeyCode.E))
        {      
            if (isRunning)
            {
                StopCoroutine(coroutine);
                isRunning = false;
                animator.SetBool("Stealing", false);
            }

            chargeWheel.SetActive(false);
        }
    }

    private IEnumerator Interact()
    {
        animator.SetBool("Stealing", true);
        isRunning = true;
        chargeWheel.SetActive(true);
        Image wheelImage = chargeWheel.GetComponent<Image>();
        robTimer = robbingItem.GetComponent<RobbingItem>().GetRobTimer();
       
        float percentage = 0;
        while (percentage < 1)
        {

            chargeWheel.transform.LookAt(playerCamera.transform);

            wheelImage.fillAmount = percentage;
            percentage += Time.deltaTime / robTimer;
            Debug.Log("Interacting...");
            yield return new WaitForEndOfFrame();
        }

        if (robbingItem != null)
        {
            int points = robbingItem.GetComponent<RobbingItem>().points;
            robbingItem.GetComponent<RobbingItem>().CreatePopup(playerCamera);
            RobbingManager.Instance.UpdateItemStateServerRpc(RobbingManager.Instance.robbingItems.IndexOf(robbingItem), false);
            GameManager.Instance.UpdatePlayerRoundScoresServerRpc(points, true);
        }

        robbingItem = null;
        chargeWheel.SetActive(false);
        Debug.Log("Robbing Successful");
        stealAudio.Play();

        isRunning = false;
        animator.SetBool("Stealing", false);
        yield return null;
    }
}
