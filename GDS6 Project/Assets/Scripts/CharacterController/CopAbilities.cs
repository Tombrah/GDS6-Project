using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class CopAbilities : NetworkBehaviour
{
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private Transform hand;
    [SerializeField] private float catchRadius = 3;
    //[SerializeField] private float onCatchTimeReduction = 3;
    //[SerializeField] private int catchPoints = 50;
    [SerializeField] private float ShootCD = 5;

    private GameObject robber;
    private GameObject zapParticle;
    private bool canShoot = true;

    private void Start()
    {
        if (IsOwner) zapParticle = GetComponent<RigidCharacterController>().zapParticle;
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying() || !IsOwner) return;

        if (robber == null)
        {
            robber = GameObject.FindWithTag("Robber");
            if (robber != null)
            {
                Debug.Log("Found Robber GameObject");
                GetComponent<TrailFader>().SetTargetMaterial(robber.GetComponentInChildren<TrailRenderer>().material);
            }
        }
        CatchRobber();
        ShootTaser();
    }

    private void ShootTaser()
    {
        if (canShoot && Input.GetMouseButtonDown(0) && playerCamera.GetComponent<TestCamera>().currentStyle == TestCamera.CameraStyle.Combat)
        {
            canShoot = false;
            RaycastHit hit;
            int layerMask = LayerMask.GetMask("Ignore Raycast");

            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 20, ~layerMask))
            {
                Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward, Color.blue, 3f);
                Debug.DrawRay(hit.point, hit.point - hand.position, Color.red, 3f);
                Debug.Log("Hit " + hit.collider.gameObject.name);
                GameObject particles = Instantiate(zapParticle, hit.point, Quaternion.identity);
                Destroy(particles, 1.1f);

                if (hit.collider.gameObject.transform.parent.gameObject.CompareTag("Player"))
                {
                    InteractionManager.Instance.SetStunServerRpc(true);
                }
            }

            StartCoroutine(ResetTaser());
        }
    }

    private IEnumerator ResetTaser()
    {
        float percentage = 0;
        Image progress = GetComponent<RigidCharacterController>().playerUi.transform.GetChild(0).GetComponentInChildren<Image>();

        while (percentage < 1)
        {
            progress.fillAmount = percentage;
            percentage += Time.deltaTime / ShootCD;
            yield return new WaitForEndOfFrame();
        }

        progress.fillAmount = 1;

        canShoot = true;
        yield return null;
    }

    private void CatchRobber()
    {
        if (Input.GetKeyDown(KeyCode.E) && robber != null)
        {
            if ((transform.position - robber.transform.GetChild(0).transform.position).sqrMagnitude < catchRadius * catchRadius)
            {
                InteractionManager.Instance.CatchRobberServerRpc();
            }
        }
    }
}
