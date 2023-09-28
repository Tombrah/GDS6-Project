using System.Collections;
using UnityEngine;

public class CamoAbility : MonoBehaviour
{
    public KeyCode activationKey = KeyCode.X; // Change this to the desired key
    private MeshRenderer meshRenderer;

    private void Start()
    {
        // Get the MeshRenderer component from the GameObject
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        // Check if the activation key is pressed
        if (Input.GetKeyDown(activationKey))
        {
            StartCoroutine(ToggleRendererForDuration(5.0f));
        }
    }

    private IEnumerator ToggleRendererForDuration(float duration)
    {
        // Turn off the MeshRenderer
        meshRenderer.enabled = false;

        // Wait for the specified duration
        yield return new WaitForSeconds(duration);

        // Turn the MeshRenderer back on
        meshRenderer.enabled = true;
    }
}
