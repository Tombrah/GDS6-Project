using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PointsPopupAnimator : MonoBehaviour
{
    private GameObject playerCam;

    [SerializeField] private AnimationCurve opacityCurve;
    [SerializeField] private AnimationCurve scaleCurve;
    [SerializeField] private AnimationCurve heightCurve;

    private TextMeshProUGUI text;
    private float time = 0;
    private Vector3 origin;

    private void Awake()
    {
        text = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        origin = transform.position;
    }

    private void Update()
    {
        if (playerCam == null)
        {
            return;
        }

        transform.forward = playerCam.transform.forward;
        text.color = new Color(1, 1, 1, opacityCurve.Evaluate(time));
        transform.localScale = Vector3.one * scaleCurve.Evaluate(time);
        transform.position = origin + new Vector3(0, 1 + heightCurve.Evaluate(time), 0);
        time += Time.deltaTime;
    }

    public void SetPlayerCamera(GameObject cam)
    {
        playerCam = cam;
    }
}
