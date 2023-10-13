using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class CopAbilities : NetworkBehaviour
{
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private Transform gun;
    [SerializeField] private Image fillImage;

    [Header("Cathing")]
    [SerializeField] private float catchRadius = 3;
    //[SerializeField] private float onCatchTimeReduction = 3;
    //[SerializeField] private int catchPoints = 50;

    [Header("Shooting")]
    [SerializeField] private float ShootCD = 5;
    [SerializeField] private float maxDistance = 20f;
    private bool canShoot = true;

    private GameObject robber;
    private GameObject zapParticle;

    [Header("Line")]
    public int ropeQuality;
    public float damper;
    public float strength;
    public float velocity;
    public float waveCount;
    public float waveHeight;
    public AnimationCurve ropeCurve;
    private Spring spring;
    private LineRenderer lr;
    private Vector3 endPoint;
    private Vector3 currentPoint;
    private bool drawLine;

    private void Start()
    {
        if (IsOwner) zapParticle = GetComponent<RigidCharacterController>().zapParticle;
        lr = gun.GetComponent<LineRenderer>();
        lr.positionCount = 0;
        spring = new Spring();
        spring.SetTarget(0);
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
                Debug.Log(robber.GetComponentInChildren<TrailRenderer>().material.name);
            }
        }
        CatchRobber();
        ShootTaser();
    }

    private void LateUpdate()
    {
        DrawLine();
    }

    private void ShootTaser()
    {
        if (canShoot && Input.GetMouseButtonDown(0) && playerCamera.GetComponent<TestCamera>().currentStyle == TestCamera.CameraStyle.Combat)
        {
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

            //StartCoroutine(FadeOut());
            StartCoroutine(ResetTaser());
        }
    }

    private void DrawLine()
    {
        if (!drawLine)
        {
            currentPoint = gun.position;
            spring.Reset();
            if (lr.positionCount > 0) lr.positionCount = 0;
            return;
        }

        if (lr.positionCount == 0)
        {
            spring.SetVelocity(velocity);
            lr.positionCount = ropeQuality + 1;
        }

        spring.SetDamper(damper);
        spring.SetStrength(strength);
        spring.Update(Time.deltaTime);

        Vector3 up = Quaternion.LookRotation((endPoint - gun.position).normalized) * Vector3.up;
        //var right = Quaternion.LookRotation((endPoint - gun.position).normalized) * Vector3.right;


        currentPoint = Vector3.Lerp(currentPoint, endPoint, Time.deltaTime * 12f);

        for (int i = 0; i < ropeQuality + 1; i++)
        {
            float delta = i / (float)ropeQuality;
            Vector3 offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * spring.Value * ropeCurve.Evaluate(delta);
            //Vector3 offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * spring.Value *
            //                         ropeCurve.Evaluate(delta) +
            //                         right * waveHeight * Mathf.Cos(delta * waveCount * Mathf.PI) * spring.Value *
            //                         ropeCurve.Evaluate(delta);

            lr.SetPosition(i, Vector3.Lerp(gun.position, currentPoint, delta) + offset);
        }

        if (spring.Value < 0)
        {
            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(0.5f);

        drawLine = false;
        yield return null;
    }

    private IEnumerator ResetTaser()
    {
        float percentage = 0;

        while (percentage < 1)
        {
            fillImage.fillAmount = -percentage + 1;
            percentage += Time.deltaTime / ShootCD;
            yield return new WaitForEndOfFrame();
        }

        fillImage.fillAmount = 0;

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
