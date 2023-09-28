using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class CamoAbility : NetworkBehaviour
{
    public KeyCode activationKey = KeyCode.X; // Change this to the desired key
    private List<MeshRenderer> meshRenderer;

    [SerializeField] private float rechargeTimer = 10f;
    [SerializeField] private Image progress;
    private bool canCamo = true;

    private void Awake()
    {
        meshRenderer = new List<MeshRenderer>();
    }

    private void Start()
    {
        // Get the MeshRenderer component from the GameObject
        foreach (Transform child in transform.GetChild(1).transform)
        {
            if (child.GetComponent<MeshRenderer>() != null)
            {
                meshRenderer.Add(child.GetComponent<MeshRenderer>());
            }
        }
        canCamo = true;
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Check if the activation key is pressed
        if (Input.GetKeyDown(activationKey) && canCamo)
        {
            canCamo = false;
            StartCoroutine(ToggleRendererForDuration(5.0f));
            ToggleRendererForDurationServerRpc();
            progress.fillAmount = 0;
        }
    }

    private IEnumerator ToggleRendererForDuration(float duration)
    {
        // Turn off the MeshRenderer
        foreach (MeshRenderer renderer in meshRenderer)
        {
            renderer.enabled = false;
        }

        // Wait for the specified duration
        yield return new WaitForSeconds(duration);

        // Turn the MeshRenderer back on
        foreach (MeshRenderer renderer in meshRenderer)
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
