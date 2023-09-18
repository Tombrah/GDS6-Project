using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;

public class TrailFader : NetworkBehaviour
{
    [HideInInspector]
    public Material targetMaterial;
    private GameObject TrailUi;
    private bool canFade = true;
    private float fadeDuration = 1.0f;
    private float maxAlpha = 1.0f;
    private float currentAlpha = 0.0f;
    private bool isVisible = false;

    [SerializeField] private float abilityDuration = 5;
    [SerializeField] private float rechargeTimer = 10;

    private void Start()
    {
        TrailUi = GetComponent<RigidCharacterController>().playerUi.transform.GetChild(1).gameObject;
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying() || !IsOwner) return; 

        if (Input.GetKeyDown(KeyCode.G) && canFade)
        {
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
        TrailUi.GetComponentInChildren<Image>().fillAmount = 0;

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
            TrailUi.GetComponentInChildren<Image>().fillAmount = percentage;

            percentage += Time.deltaTime / rechargeTimer;
            yield return new WaitForEndOfFrame();
        }

        TrailUi.GetComponentInChildren<Image>().fillAmount = 1;
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

        // Material is fully transparent after fading out.
        isVisible = false;
    }

    public void SetTargetMaterial(Material mat)
    {
        targetMaterial = mat;
    }
}
