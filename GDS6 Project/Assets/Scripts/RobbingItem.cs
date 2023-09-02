using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RobbingItem : MonoBehaviour
{
    [SerializeField] private GameObject pointPopup;
    public int points = 100;
    public float robTimer = 3;

    public void CreatePopup(GameObject playerCam)
    {
        GameObject popup = Instantiate(pointPopup, gameObject.transform.position, Quaternion.identity);
        TextMeshProUGUI popupText = popup.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        PointsPopupAnimator popupAnimation = popup.GetComponentInChildren<PointsPopupAnimator>();

        popupText.text = points.ToString();
        popupAnimation.SetPlayerCamera(playerCam);
        Destroy(popup, 1.5f);
    }
}
