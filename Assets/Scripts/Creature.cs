using Lasp;
using System.Collections;
using UnityEngine;

public class Creature : MonoBehaviour
{
    private CreatureManager m_manager;

    public Renderer Renderer { get; private set; }
    public SphereCollider Collider { get; private set; }

    private AudioLevelTracker audioLevelTracker;

    public enum CreatureBehavior { Static, Reactive };
    private CreatureBehavior _currentBehavior;

    public bool ColliderActive { get; set; }

    public bool IsActive = true;
    public bool AudioReactive = false;
    public float TurningSpeed = 2.0f;
    public float Delay = 2.0f;
    public float Force = 5.0f;
    public float TargetProximity = 5.0f;

    private Vector2 position;
    private Vector2 forward;

    private Vector2 targetDir;
    private Vector2 targetLocation;

    public float AudioLevel = 0;
    private float lastPeak;
    private float originalRadius;

    /// <summary>
    /// Whether the creature is moving about aimlessly.
    /// </summary>
    private bool isWandering = false;

    /// <summary>
    /// Whether the creature has fulfilled its duties and escaped.
    /// </summary>
    private bool taskFinished = false;

    public CreatureBehavior CurrentBehavior
    {
        get { return _currentBehavior; }
        set { _currentBehavior = value; }
    }

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

        audioLevelTracker = FindObjectOfType<AudioLevelTracker>();

        CurrentBehavior = CreatureBehavior.Static;
        ResetTargetLocation();
        Renderer.material.SetFloat("_Alpha", 0);
    }

    void Update()
    {
        if (CurrentBehavior == CreatureBehavior.Static) return;
        if (CurrentBehavior == CreatureBehavior.Reactive)
        {
            position = transform.position;
            if (Vector2.Distance(position, targetLocation) < TargetProximity)
            {
                if (isWandering) ResetTargetLocation();
                else taskFinished = true; // the creature has reached its final destination
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
    }

    public void SetFree()
    {
        isWandering = true;
        taskFinished = false;
        ResetTargetLocation();

        if (AudioReactive)
            m_manager.OnAudioBeat += () => Impulse(); 
        else
            StartCoroutine(PeriodicImpulse(Delay));
    }

    public void Escape()
    {
        targetLocation = Random.insideUnitCircle.normalized * Camera.main.orthographicSize * 3f;
        isWandering = false;
    }

    private void ResetTargetLocation()
    {
        targetLocation = new Vector2(Random.Range(-ReefHelper.DisplayWidth / 2, ReefHelper.DisplayWidth / 2), Random.Range(ReefHelper.DisplayHeight / 2, -ReefHelper.DisplayHeight / 2));
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
            Impulse();
            yield return new WaitForSeconds(period);
        }
    }

    private void Impulse()
    {
        if (ColliderActive)
        {
            Collider.attachedRigidbody.AddForce(forward * Force, ForceMode.Impulse);
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

    public void SetManager(CreatureManager manager)
    {
        m_manager = manager;
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
