using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;

public class TrailFader : NetworkBehaviour
{
    [HideInInspector]
    public Material targetMaterial;
    private bool canFade = true;
    private float fadeDuration = 1.0f;
    private float maxAlpha = 1.0f;
    private float currentAlpha = 0.0f;

    [SerializeField] private Image fillImage;
    [SerializeField] private float abilityDuration = 5;
    [SerializeField] private float rechargeTimer = 10;

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying() || !IsOwner) return; 

        if (Input.GetKeyDown(KeyCode.R) && canFade)
        {
            Debug.Log("Sniffing");
            StartFadeIn();
        }
    }

    private void StartFadeIn()
    {
        canFade = false;
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0;
        fillImage.fillAmount = 1;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            currentAlpha = Mathf.Lerp(0.0f, maxAlpha, elapsedTime / fadeDuration);

            Color currentColor = targetMaterial.color;
            currentColor.a = currentAlpha;
            targetMaterial.color = currentColor;

            yield return new WaitForEndOfFrame();
        }

        // Wait for ability duration seconds before fading out.
        yield return new WaitForSeconds(abilityDuration);

        // Start fading out.
        StartFadeOut();
    }

    private void StartFadeOut()
    {
        StartCoroutine(FadeOut());
        StartCoroutine(SetUi());
    }

    private IEnumerator SetUi()
    {
        float percentage = 0;
        while (percentage < 1)
        {
            fillImage.fillAmount = -percentage + 1;

            percentage += Time.deltaTime / rechargeTimer;
            yield return new WaitForEndOfFrame();
        }

        fillImage.fillAmount = 0;
        canFade = true;
        yield return null;
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            currentAlpha = Mathf.Lerp(maxAlpha, 0.0f, elapsedTime / fadeDuration);

            Color currentColor = targetMaterial.color;
            currentColor.a = currentAlpha;
            targetMaterial.color = currentColor;

            yield return null;
        }
    }

    public void SetTargetMaterial(Material mat)
    {
        targetMaterial = mat;
    }
}
