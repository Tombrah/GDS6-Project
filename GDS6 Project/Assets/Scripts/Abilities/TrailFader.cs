using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class TrailFader : NetworkBehaviour
{
    [HideInInspector]
    public Material targetMaterial;
    private bool isFading = false;
    private float fadeDuration = 1.0f;
    private float maxAlpha = 1.0f;
    private float currentAlpha = 0.0f;
    private bool isVisible = false;

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying() || !IsOwner) return; 

        if (Input.GetKeyDown(KeyCode.G) && !isFading)
        {
            StartFadeIn();
        }
    }

    private void StartFadeIn()
    {
        isFading = true;
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            currentAlpha = Mathf.Lerp(0.0f, maxAlpha, elapsedTime / fadeDuration);

            Color currentColor = targetMaterial.color;
            currentColor.a = currentAlpha;
            targetMaterial.color = currentColor;

            yield return new WaitForEndOfFrame();
        }

        // Material is fully visible after fading in.
        isVisible = true;

        // Wait for 5 seconds before fading out.
        yield return new WaitForSeconds(5.0f);

        // Start fading out.
        StartFadeOut();
    }

    private void StartFadeOut()
    {
        StartCoroutine(FadeOut());
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

        // Material is fully transparent after fading out.
        isVisible = false;
        isFading = false;
    }

    public void SetTargetMaterial(Material mat)
    {
        targetMaterial = mat;
    }
}
