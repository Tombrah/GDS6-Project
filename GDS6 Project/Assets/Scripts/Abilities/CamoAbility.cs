using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class CamoAbility : NetworkBehaviour
{
    public KeyCode activationKey = KeyCode.X; // Change this to the desired key

    [SerializeField] private List<Renderer> meshRenderer;
    [SerializeField] private float rechargeTimer = 10f;
    [SerializeField] private Image progress;
    private bool canCamo = true;

    private void Start()
    {
        canCamo = true;
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying() || !IsOwner) return;

        // Check if the activation key is pressed
        if (Input.GetKeyDown(activationKey) && canCamo)
        {
            Debug.Log("Going invisible");
            canCamo = false;
            StartCoroutine(ToggleRendererForDuration(5.0f));
            ToggleRendererForDurationServerRpc();
            progress.fillAmount = 0;
        }
    }

    private IEnumerator ToggleRendererForDuration(float duration)
    {
        // Turn off the MeshRenderer
        foreach (Renderer renderer in meshRenderer)
        {
            renderer.enabled = false;
        }

        // Wait for the specified duration
        yield return new WaitForSeconds(duration);

        // Turn the MeshRenderer back on
        foreach (Renderer renderer in meshRenderer)
        {
            renderer.enabled = true;
        }
        StartCoroutine(SetUi());
    }

    private IEnumerator SetUi()
    {
        float percentage = 0;
        while (percentage < 1)
        {
            percentage += Time.deltaTime / rechargeTimer;
            progress.fillAmount = percentage;
            yield return new WaitForEndOfFrame();
        }

        progress.fillAmount = 1;
        canCamo = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleRendererForDurationServerRpc()
    {
        ToggleRendererForDurationClientRpc();
    }

    [ClientRpc]
    private void ToggleRendererForDurationClientRpc()
    {
        if (IsOwner) return;

        StartCoroutine(ToggleRendererForDuration(5.0f));
    }
}
