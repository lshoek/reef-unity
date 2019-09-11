
using System.Collections;
using UnityEngine;

public class Creature : MonoBehaviour
{
    public Renderer Renderer { get; private set; }
    public SphereCollider Collider { get; private set; }

    public enum CreatureBehavior { Free, Tasked, Static };
    public CreatureBehavior CurrentBehavior { get; private set; }

    public bool ColliderActive { get; set; }

    public bool IsActive = true;
    public float TurningSpeed = 2.0f;
    public float Delay = 2.0f;
    public float Force = 5.0f;
    public float TargetProximity = 5.0f;

    private Vector2 position;
    private Vector2 forward;

    private Vector2 targetDir;
    private Vector2 targetLocation;

    private float originalRadius;
    private bool taskFinished = false;

    void Awake()
    {
        Renderer = GetComponentInChildren<Renderer>();
        Collider = GetComponent<SphereCollider>();
        originalRadius = Collider.radius;
    }

    void Start()
    {
        position = transform.position;
        forward = (position + Random.insideUnitCircle).normalized;

        CurrentBehavior = CreatureBehavior.Free;
        ResetTargetLocation();
        Renderer.material.SetFloat("_Alpha", 0);
    }

    void Update()
    {
        if (CurrentBehavior == CreatureBehavior.Static) return;

        position = transform.position;
        if (Vector2.Distance(position, targetLocation) < TargetProximity)
        {
            if (CurrentBehavior == CreatureBehavior.Free) ResetTargetLocation();
            else if (CurrentBehavior == CreatureBehavior.Tasked)
            {
                taskFinished = true;
            };
        }
        if (!taskFinished)
        {
            targetDir = (targetLocation - position).normalized;

            float sa = Vector2.SignedAngle(forward, targetDir);
            if (Mathf.Abs(sa) > TurningSpeed)
            {
                float turn = TurningSpeed * (Mathf.Clamp(sa, -45f, 45f) / 45f);
                forward = Quaternion.Euler(0, 0, turn) * forward;
            }
            float angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
            transform.localRotation = Quaternion.Euler(0, 0, angle - 180f);
        }
    }

    public void SetStatic()
    {
        CurrentBehavior = CreatureBehavior.Static;
    }

    public void SetTargetLocation(Vector2 target)
    {
        targetLocation = target;
        CurrentBehavior = CreatureBehavior.Tasked;
    }

    public void SetFree()
    {
        ResetTargetLocation();
        StartCoroutine(PeriodicImpulse(Delay));
        taskFinished = false;
        CurrentBehavior = CreatureBehavior.Free;
    }

    private void ResetTargetLocation()
    {
        float height = 2f * Camera.main.orthographicSize;
        float width = height * Camera.main.aspect;
        targetLocation = new Vector2(Random.Range(-width / 2, width / 2), Random.Range(height / 2, -height / 2));
    }

    public void SetScale(float scale)
    {
        transform.localScale = new Vector3(scale, scale, 1f);
        Collider.radius = originalRadius * scale;
    }

    private IEnumerator PeriodicImpulse(float period)
    {
        // force varying movement offsets
        yield return new WaitForSeconds(Random.Range(0.5f, 2.0f));

        while (!taskFinished)
        {
            if (ColliderActive)
            {
                Collider.attachedRigidbody.AddForce(forward * Force, ForceMode.Impulse);
            }
            yield return new WaitForSeconds(period);
        }
    }

    public IEnumerator FadeOut(float duration)
    {
        float time = 0;
        while (time <= duration)
        {
            time += Time.deltaTime;
            Renderer.material.SetFloat("_Alpha", Mathf.Clamp(1.0f-(time/duration), 0, 1f));
            yield return null;
        }
        Renderer.material.SetFloat("_Alpha", 0f);
    }

    public IEnumerator FadeIn(float duration)
    {
        float time = 0;
        while (time <= duration)
        {
            time += Time.deltaTime;
            Renderer.material.SetFloat("_Alpha", Mathf.Clamp(time / duration, 0, 1f));
            yield return null;
        }
        Renderer.material.SetFloat("_Alpha", 1f);
    }

    public void SetActive(bool active)
    {
        IsActive = active;
        Renderer.enabled = active;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, position + forward * 5.0f);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, position + targetDir * 5.0f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetLocation, 0.25f);
    }
}
