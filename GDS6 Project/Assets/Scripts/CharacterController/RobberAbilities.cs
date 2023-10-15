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

    private readonly Collider[] colliders = new Collider[3];
    private int numFound;

    private GameObject cop;
    private GameObject robbingItem;
    private Coroutine coroutine;
    private bool isRunning = false;

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying() || !IsOwner) return;

        if (cop == null)
        {
            cop = GameObject.FindWithTag("Cop");
        }

        CheckRespawnPlayer();
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
            }

            chargeWheel.SetActive(false);
        }
    }

    private float CalculatePercentageHeight(Transform fillMeter)
    {
        float height = fillMeter.GetComponent<RectTransform>().rect.height;
        return fillMeter.position.y + height / 2 - (height - (height * fillMeter.GetComponent<Image>().fillAmount));
    }

    private IEnumerator Interact()
    {
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

        isRunning = false;
        yield return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectable"))
        {
            Debug.Log("Can Interact");
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
            robbingItem = null;
        }
    }

    public void CheckRespawnPlayer()
    {
        if (InteractionManager.Instance == null) return;

        if (InteractionManager.Instance.GetIsCaught())
        {
            GetComponent<RigidCharacterController>().enabled = false;
            int index = Random.Range(0, GameManager.Instance.respawnPoints.Count);
            if (cop != null)
            {
                while (Vector3.Distance(cop.transform.position, GameManager.Instance.respawnPoints[index].position) < 40f)
                {
                    index = Random.Range(0, GameManager.Instance.respawnPoints.Count);
                }
            }

            transform.position = GameManager.Instance.respawnPoints[index].position;
            transform.rotation = GameManager.Instance.respawnPoints[index].rotation;

            GetComponent<RigidCharacterController>().enabled = true;
            InteractionManager.Instance.SetCaughtServerRpc(false);
            InteractionManager.Instance.SetIsCaught(false);
        }
    }
}
