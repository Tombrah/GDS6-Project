using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class CopAbilities : NetworkBehaviour
{
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private Transform gun;
    [SerializeField] private float catchRadius = 3;
    //[SerializeField] private float onCatchTimeReduction = 3;
    //[SerializeField] private int catchPoints = 50;
    [SerializeField] private float ShootCD = 5;
    [SerializeField] private float maxDistance = 20f;

    private LineRenderer lr;
    private Vector3 endPoint;
    private float fadeDuration = 2;
    private bool drawLine;
    private GameObject robber;
    private GameObject zapParticle;
    private bool canShoot = true;

    private void Start()
    {
        if (IsOwner) zapParticle = GetComponent<RigidCharacterController>().zapParticle;
        lr = gun.GetComponent<LineRenderer>();
        lr.positionCount = 0;
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

    private void LateUpdate()
    {
        if (drawLine)
        {
            DrawLine();
        }
    }

    private void ShootTaser()
    {
        if (canShoot && Input.GetMouseButtonDown(0) && playerCamera.GetComponent<TestCamera>().currentStyle == TestCamera.CameraStyle.Combat)
        {
            lr.positionCount = 2;
            drawLine = true;
            canShoot = false;
            int layerMask = LayerMask.GetMask("Ignore Raycast");

            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, maxDistance, ~layerMask))
            {
                endPoint = hit.point;

                Debug.Log("Hit " + hit.collider.gameObject.name);

                if (hit.collider.transform.parent != null && hit.collider.transform.parent.gameObject.CompareTag("Player"))
                {
                    GameObject particles = Instantiate(zapParticle, hit.point, Quaternion.identity);
                    Destroy(particles, 1.1f);
                    InteractionManager.Instance.SetStunServerRpc(true);
                }
            }
            else
            {
                endPoint = playerCamera.transform.position + playerCamera.transform.forward * maxDistance;
            }

            StartCoroutine(FadeOut());
            StartCoroutine(ResetTaser());
        }
    }

    private void DrawLine()
    {
        lr.SetPosition(0, gun.position);
        lr.SetPosition(1, endPoint);
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0;

        while (elapsedTime < fadeDuration)
        {
            if (elapsedTime > fadeDuration / 2)lr.SetPosition(1, endPoint -= Vector3.up * 2 * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);

            Color currentColor = Color.white;
            currentColor.a = currentAlpha;
            lr.material.SetColor("_TintColor", currentColor);

            yield return new WaitForEndOfFrame();
        }

        // Material is fully transparent after fading out.
        drawLine = false;
        lr.positionCount = 0;
        yield return null;
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
