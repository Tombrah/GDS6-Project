using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostProcessLights : MonoBehaviour
{
    [SerializeField] private float lerpSpeed = 5;

    private float height;

    private void Start()
    {
        LightSwitchController.Instance.lightOn += Lights_lightOn;
        LightSwitchController.Instance.lightOff += Lights_lightOff;

        height = transform.localPosition.y;

        gameObject.SetActive(false);
    }

    private void Lights_lightOff(object sender, System.EventArgs e)
    {
        gameObject.SetActive(true);
       //Vector3 startPos = new Vector3(0, height, 0);
       //Vector3 endPos = new Vector3(0, 0, 0); ;
       //StartCoroutine(LerpPosition(startPos, endPos));
    }

    private void Lights_lightOn(object sender, System.EventArgs e)
    {
        gameObject.SetActive(false);
        //Vector3 startPos = new Vector3(0, 0, 0);
        //Vector3 endPos = new Vector3(0, height, 0);
        //
        //StartCoroutine(LerpPosition(startPos, endPos));
    }

    private void OnDestroy()
    {
        LightSwitchController.Instance.lightOn -= Lights_lightOn;
        LightSwitchController.Instance.lightOff -= Lights_lightOff;
    }

    private IEnumerator LerpPosition(Vector3 startPos, Vector3 endPos)
    {
        float percentage = 0;
        while (percentage < 1)
        {
            percentage += Time.deltaTime / lerpSpeed;
            transform.localPosition = Vector3.Lerp(startPos, endPos, percentage);
            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }
}
