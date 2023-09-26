using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class RobberAbilities : NetworkBehaviour
{
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject chargeWheel;
    [SerializeField] private GameObject quickTimeEvent;
    [SerializeField] private float quickTimeFillSpeed = 2;
    [SerializeField] private float robTimer = 1;

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
        CheckRespawnPlayer();
    }

    private void RobInteraction()
    {
        if (canInteract && Input.GetKeyDown(KeyCode.E))
        {
            coroutine = StartCoroutine(Interact());
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            if (robbingItem == null) return;

            
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            if (robbingItem.GetComponent<RobbingItem>().isQuickTime)
            {
                Transform fillMeter = quickTimeEvent.transform.GetChild(1);
                Transform threshold = fillMeter.GetChild(0);
                float thresholdHeightRangeMin = threshold.position.y - threshold.GetComponent<RectTransform>().rect.height / 2;
                float thresholdHeightRangeMax = threshold.position.y + threshold.GetComponent<RectTransform>().rect.height / 2;

                float percentageheight = CalculatePercentageHeight(fillMeter);
                if (percentageheight >= thresholdHeightRangeMin && percentageheight <= thresholdHeightRangeMax)
                {
                    int points = robbingItem.GetComponent<RobbingItem>().points;
                    robbingItem.GetComponent<RobbingItem>().CreatePopup(playerCamera);
                    RobbingManager.Instance.UpdateItemStateServerRpc(RobbingManager.Instance.robbingItems.IndexOf(robbingItem), false);
                    robbingItem = null;
                    GameManager.Instance.UpdatePlayerRoundScoresServerRpc(points, true);

                    canInteract = false;
                    Debug.Log("Robbing Successful");
                }
                else
                {
                    robbingItem.GetComponent<RobbingItem>().CreatePopup(playerCamera, true);
                    RobbingManager.Instance.UpdateItemStateServerRpc(RobbingManager.Instance.robbingItems.IndexOf(robbingItem), false);
                    robbingItem = null;

                    canInteract = false;
                    Debug.Log("Failed Quick Time, Reason: Missed Target");
                }
            }

            chargeWheel.SetActive(false);
            quickTimeEvent.SetActive(false);
        }
    }

    private float CalculatePercentageHeight(Transform fillMeter)
    {
        float height = fillMeter.GetComponent<RectTransform>().rect.height;
        return fillMeter.position.y + height / 2 - (height - (height * fillMeter.GetComponent<Image>().fillAmount));
    }

    private IEnumerator Interact()
    {
        bool isQuickTime = robbingItem.GetComponent<RobbingItem>().isQuickTime;

        if (isQuickTime)
        {
            quickTimeEvent.SetActive(true);

            Transform fillMeter = quickTimeEvent.transform.GetChild(1);
            Image meterImage = fillMeter.GetComponent<Image>();
            Transform threshold = fillMeter.GetChild(0);
            float thresholdHeightRangeMin = fillMeter.position.y + threshold.GetComponent<RectTransform>().rect.height / 2;
            float thresholdHeightRangeMax = fillMeter.position.y + fillMeter.GetComponent<RectTransform>().rect.height / 2 - threshold.GetComponent<RectTransform>().rect.height / 2;

            threshold.position = new Vector3(threshold.position.x, Random.Range(thresholdHeightRangeMin, thresholdHeightRangeMax), threshold.position.z);

            float percentage = 0;
            while (percentage < 1)
            {
                quickTimeEvent.transform.LookAt(playerCamera.transform);

                meterImage.fillAmount = percentage;
                percentage += Time.deltaTime / quickTimeFillSpeed;
                yield return new WaitForEndOfFrame();
            }

            robbingItem.GetComponent<RobbingItem>().CreatePopup(playerCamera, true);
            RobbingManager.Instance.UpdateItemStateServerRpc(RobbingManager.Instance.robbingItems.IndexOf(robbingItem), false);

            robbingItem = null;
            quickTimeEvent.SetActive(false);
            canInteract = false;
            Debug.Log("Failed Quick Time, Reason: Didn't Let Go");
            yield return null;
        }
        else
        {
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
            canInteract = false;
            Debug.Log("Robbing Successful");
        }
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
            quickTimeEvent.SetActive(false);
            canInteract = false;
            robbingItem = null;
        }
    }

    public void CheckRespawnPlayer()
    {
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
