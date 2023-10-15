using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RobbingItem : MonoBehaviour
{
    [SerializeField] private GameObject pointPopup;
    public bool isFruitBox;
    public bool isClothing;
    public bool isActive;
    public int points;
    private float robTimer = 0.5f;

    public void CreatePopup(GameObject playerCam)
    {
        GameObject popup = Instantiate(pointPopup, gameObject.transform.position, Quaternion.identity);
        TextMeshProUGUI popupText = popup.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        PointsPopupAnimator popupAnimation = popup.GetComponentInChildren<PointsPopupAnimator>();

        popupText.text = points.ToString();

        popupAnimation.SetPlayerCamera(playerCam);
        Destroy(popup, 1.5f);
    }

    public void SetActiveState(bool isActive)
    {
        if (isActive)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    public void Show()
    {
        isActive = true;
        if (isFruitBox)
        {
            foreach (Transform child in transform)
            {
                child.GetChild(1).gameObject.SetActive(true);
            }
        }
        else if (isClothing)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (i == 0) continue;
                transform.GetChild(i).gameObject.SetActive(true);
            }
        }
        else
        {
            gameObject.SetActive(true);
        }

        points = 20;
    }

    public void Hide()
    {
        isActive = false;
        if (isFruitBox)
        {
            foreach (Transform child in transform)
            {
                child.GetChild(1).gameObject.SetActive(false);
            }
        }
        else if (isClothing)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (i == 0) continue;
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        else
        {
            gameObject.SetActive(false);    
        }
    }

    public float GetRobTimer()
    {
        return robTimer;
    }
}
