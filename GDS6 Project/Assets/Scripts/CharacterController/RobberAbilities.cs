using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RobberAbilities : NetworkBehaviour
{
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject chargeWheel;
    [SerializeField] private float robTimer = 3;

    private GameObject cop;
    private GameObject robbingItem;
    private Coroutine coroutine;
    private bool canInteract = false;

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying() || !IsOwner) return;

        if (cop == null)
        {
            cop = GameObject.FindWithTag("Cop");
        }
        RobInteraction();
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

            chargeWheel.transform.LookAt(playerCamera.transform);

            wheelRenderer.material.SetFloat("_Percentage", percentage);
            percentage += Time.deltaTime / robTimer;
            Debug.Log("Interacting...");
            yield return new WaitForEndOfFrame();
        }

        if (robbingItem != null)
        {
            int points = robbingItem.GetComponent<RobbingItem>().points;
            robbingItem.GetComponent<RobbingItem>().CreatePopup(playerCamera);
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
        if (!IsOwner) return;

        int index = Random.Range(0, GameManager.Instance.respawnPoints.Count);
        if (cop != null)
        {
            while (Vector3.Distance(cop.transform.position, GameManager.Instance.respawnPoints[index].position) < 30f)
            {
                index = Random.Range(0, GameManager.Instance.respawnPoints.Count);
            }
        }

        transform.position = GameManager.Instance.respawnPoints[index].position;
        transform.rotation = GameManager.Instance.respawnPoints[index].rotation;
    }

    [ClientRpc]
    public void GetTasedClientRpc(float stunTimer)
    {
        if (!IsOwner) return;

        Debug.Log("I got stunned oh no!");
        StartCoroutine(Stun(stunTimer));
    }

    private IEnumerator Stun(float stunTimer)
    {
        StarterAssets.ThirdPersonController controller = GetComponent<StarterAssets.ThirdPersonController>();

        controller.stunned = true;

        float time = 0;
        while (time < stunTimer)
        {
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        controller.stunned = false;
    }
}
