using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessLights : MonoBehaviour
{
    [SerializeField] private float lerpSpeed = 5;

    private Volume volume;
    private float height;

    private void Start()
    {
        LightSwitchController.Instance.lightOn += Lights_lightOn;
        LightSwitchController.Instance.lightOff += Lights_lightOff;

        volume = GetComponent<Volume>();
        height = transform.localPosition.y;
    }

    private void Lights_lightOff(object sender, System.EventArgs e)
    {
        float percentage = 0;
        Vector3 startPos = new Vector3(0, height, 0);
        Vector3 endPos = new Vector3(0, 0, 0); ;
        while (percentage < 1)
        {
            percentage += Time.deltaTime / lerpSpeed;
            transform.localPosition = Vector3.Lerp(startPos, endPos, percentage);
        }
    }

    private void Lights_lightOn(object sender, System.EventArgs e)
    {
        float percentage = 0;
        Vector3 startPos = new Vector3(0, 0, 0);
        Vector3 endPos = new Vector3(0, height, 0);
        while (percentage < 1)
        {
            percentage += Time.deltaTime / lerpSpeed;
            transform.localPosition = Vector3.Lerp(startPos, endPos, percentage);
        }
    }

    private void OnDestroy()
    {
        LightSwitchController.Instance.lightOn -= Lights_lightOn;
        LightSwitchController.Instance.lightOff -= Lights_lightOff;
    }
}
