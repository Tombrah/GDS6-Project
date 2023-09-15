using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RobbingItem : MonoBehaviour
{
    [SerializeField] private GameObject pointPopup;
    public bool isQuickTime;
    public int points;
    private float robTimer = 0.5f;

    public void CreatePopup(GameObject playerCam, bool failed = false)
    {
        GameObject popup = Instantiate(pointPopup, gameObject.transform.position, Quaternion.identity);
        TextMeshProUGUI popupText = popup.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        PointsPopupAnimator popupAnimation = popup.GetComponentInChildren<PointsPopupAnimator>();

        popupText.text = points.ToString();
        if (failed)
        {
            popupText.text = "Failed!";
        }
        popupAnimation.SetPlayerCamera(playerCam);
        Destroy(popup, 1.5f);
    }

    public void SetActiveState(bool isActive, bool isQuickTimeRandom, float quickTimePercentage)
    {
        if (isActive)
        {
            Show(isQuickTimeRandom, quickTimePercentage);
        }
        else
        {
            Hide();
        }
    }

    private void Show(bool isQuickTimeRandom, float quickTimePercentage)
    {
        gameObject.SetActive(true);
        if (isQuickTimeRandom)
        {
            isQuickTime = Random.value > quickTimePercentage;
        }

        if (isQuickTime)
        {
            points = 100;
        }
        else
        {
            points = 20;
        }
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public float GetRobTimer()
    {
        return robTimer;
    }
}
