using Lasp;
using System.Collections;
using UnityEngine;

public class Creature : MonoBehaviour
{
    private CreatureManager m_manager;
    private BeatInfoManager m_beatInfoManager;

    public Renderer Renderer { get; private set; }
    public SphereCollider Collider { get; private set; }

    public enum CreatureBehavior { Static, Reactive };
    private CreatureBehavior _currentBehavior;

    public bool ColliderActive { get; set; }
    public bool IsActive = false;
    public bool AudioSensitive = false;

    public Color TintColor = Color.white;
    public float TurningSpeed = 1f;
    public float Delay = 2f;
    public float Force = 5f;
    public float NoiseMult = 8f;

    private float defaultAudioReactiveForce = 4f;
    private float targetProximity = 8f;
    private float originalRadius;

    private Vector2 position;
    private Vector2 forward;

    private Vector2 targetDir;
    private Vector2 targetLocation;

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
        m_beatInfoManager = Application.Instance.BeatInfoManager;

        position = transform.position;
        forward = (position + Random.insideUnitCircle).normalized;

        CurrentBehavior = CreatureBehavior.Static;
        ResetTargetLocation();
    }

    public void Init(CreatureManager manager)
    {
        m_manager = manager;
    }

    void Update()
    {
        if (CurrentBehavior == CreatureBehavior.Static)
        {
            float angle = Perlin.Noise(Time.time/2) * NoiseMult;
            transform.localRotation = Quaternion.Euler(0, 0, angle - 180f);
        }
        if (CurrentBehavior == CreatureBehavior.Reactive)
        {
            position = transform.position;
            if (Vector2.Distance(position, targetLocation) < targetProximity)
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

    public void StartMovement()
    {
        if (CurrentBehavior == CreatureBehavior.Reactive)
        {
            isWandering = true;
            taskFinished = false;
            ResetTargetLocation();
        }
        if (AudioSensitive)
        {
            m_beatInfoManager.OnAudioBeat += () => Impulse();
            m_beatInfoManager.OnNormalizedAudioLevelInput += (x) => NormalizedAudioLevelInput(x);
        }
        else
            StartCoroutine(PeriodicImpulse(Delay));
    }

    public void Escape()
    {
        if (CurrentBehavior == CreatureBehavior.Reactive)
        {
            targetLocation = Random.insideUnitCircle.normalized * Application.Instance.MainCamera.orthographicSize * 3f;
            isWandering = false;
        }
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
        yield return new WaitForSeconds(Random.Range(0.5f, 2f));

        while (!taskFinished)
        {
            Impulse();
            yield return new WaitForSeconds(period);
        }
    }

    private void NormalizedAudioLevelInput(float level)
    {
        Renderer.material.SetFloat("_TintPct", level);
        Renderer.material.SetColor("_Tint", TintColor);
    }

    private void Impulse()
    {
        if (ColliderActive && CurrentBehavior == CreatureBehavior.Reactive)
        {
            Collider.attachedRigidbody.AddForce(forward * (AudioSensitive ? defaultAudioReactiveForce : Force), ForceMode.Impulse);
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
        SetActive(false);
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

        if (!active)
        {
            m_beatInfoManager.OnAudioBeat -= () => Impulse();
            m_beatInfoManager.OnNormalizedAudioLevelInput -= (x) => NormalizedAudioLevelInput(x);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, position + forward.normalized * 2.0f);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, position + targetDir.normalized * 2.0f);

        //Gizmos.color = Color.green;
        //Gizmos.DrawWireSphere(targetLocation, 0.25f);
    }
}
