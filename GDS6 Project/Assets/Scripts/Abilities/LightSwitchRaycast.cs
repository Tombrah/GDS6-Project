using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;


public class LightSwitchRaycast : NetworkBehaviour
{
    [SerializeField] private int rayLength = 5;
    [SerializeField] private Image crosshair;
    [SerializeField] private LayerMask playerLayer;

    private LightSwitchController interactiveObj;
    private RigidCharacterController controller;

    private void Start()
    {
        controller = transform.parent.GetComponentInChildren<RigidCharacterController>();
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying() || !IsOwner) return;

        Vector3 fwd = transform.TransformDirection(Vector3.forward);

        if(Physics.Raycast(transform.position, fwd, out RaycastHit hit, rayLength, ~playerLayer))
        {
            var raycastObj = hit.collider.gameObject.GetComponent<LightSwitchController>();
            if(raycastObj != null)
            {
                interactiveObj = raycastObj;
                CrosshairChange(true);
            }
            else
            {
                ClearInteraction();
            }
        }
        else
        {
            ClearInteraction();
        }

        if(interactiveObj != null)
        {
            if(Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (controller.Id == 0 && !interactiveObj.isLightOn)
                {
                    interactiveObj.InteractSwitchServerRpc();  
                }

                if (controller.Id == 1 && interactiveObj.isLightOn)
                {
                    interactiveObj.InteractSwitchServerRpc();
                }
                Debug.Log("Clicking");
            }
        }
    }

    private void ClearInteraction()
    {
        if(interactiveObj != null)
        {
            CrosshairChange(false);
            interactiveObj = null;
        }
    }

    void CrosshairChange (bool on)
    {
        if(on)
        {
            crosshair.color = Color.green;
        }
        else
        {
            crosshair.color = Color.white;
        }
    }
}
