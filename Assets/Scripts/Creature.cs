using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Creature : MonoBehaviour
{
    private CreatureManager m_manager;
    private BeatInfoManager m_beatInfoManager;

    public Renderer Renderer { get; private set; }
    public SphereCollider Collider { get; private set; }

    public bool ColliderActive { get; set; }
    public bool AudioSensitive = false;

    public Color TintColor = Color.white;
    public float TurningSpeed = 1f;
    public float Delay = 2f;
    public float Force = 2f;
    public float NoiseMult = 8f;
    public float LevelDamping = 0.0625f;

    private float targetProximity = 8f;
    private float originalRadius;
    private float noiseSeed;

    private Vector2 position;
    private Vector2 forward;
    private float cachedScale = 1.0f;

    private Vector2 targetDir;
    private Vector2 targetLocation;

    private bool isActive = false;
    public bool IsActive
    {
        get { return isActive; }
        set
        {
            if (value && AudioSensitive)
            {
                m_beatInfoManager.OnNormalizedAudioLevelInput += (x) => NormalizedAudioLevelInput(x);
                m_beatInfoManager.OnAudioBeat += () => Impulse();
            }
            else if (!value && AudioSensitive)
            {
                m_beatInfoManager.OnNormalizedAudioLevelInput -= (x) => NormalizedAudioLevelInput(x);
                m_beatInfoManager.OnAudioBeat -= () => Impulse();
            }
            Renderer.enabled = value;
            isActive = value;
        }
    }

    /// <summary>
    /// Whether the creature is moving about aimlessly.
    /// </summary>
    private bool isWandering = false;

    /// <summary>
    /// Whether the creature has fulfilled its duties and escaped.
    /// </summary>
    private bool taskFinished = false;

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

        noiseSeed = Random.Range(0, 64f);
        ResetTargetLocation();
    }

    public void Init(CreatureManager manager)
    {
        m_manager = manager;
    }

    void Update()
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
                forward = (Quaternion.Euler(0, 0, turn) * forward).normalized;
            }
            float noiseAngle = Perlin.Noise(Time.time / 2) * NoiseMult;
            float angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg + noiseAngle;
            transform.localRotation = Quaternion.Euler(0, 0, angle - 180f);
        }
    }

    public void StartMovement()
    {
        isWandering = true;
        taskFinished = false;
        ResetTargetLocation();

        if (!AudioSensitive) StartCoroutine(PeriodicImpulse(Delay));
    }

    public void Escape()
    {
        targetLocation = Random.insideUnitCircle.normalized * Application.Instance.MainCamera.orthographicSize * 3f;
        isWandering = false;
    }

    private void ResetTargetLocation()
    {
        targetLocation = new Vector2(Random.Range(-ReefHelper.DisplayWidth / 2, ReefHelper.DisplayWidth / 2), Random.Range(ReefHelper.DisplayHeight / 2, -ReefHelper.DisplayHeight / 2));
    }

    public void SetScale(float scale)
    {
        cachedScale = scale;
        transform.localScale = new Vector3(cachedScale, cachedScale, 1f);
        Collider.radius = originalRadius * cachedScale;
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
        transform.localScale = new Vector3(cachedScale + level * LevelDamping, cachedScale + level * LevelDamping, 1f);
    }

    private void Impulse()
    {
        if (ColliderActive)
        {
            Collider.attachedRigidbody.AddForce(forward * Force, ForceMode.Impulse);
        }
    }

    public void FadeOut(float duration, Action callback)
    {
        StartCoroutine(ReefHelper.FadeNormalized(ReefHelper.FadeType.Out, 3f,
            (x) => { Renderer.material.SetFloat("_Alpha", x); },
            () => {
                IsActive = false;
                callback?.Invoke();
            }));
    }

    public void FadeIn(float duration, Action callback)
    {
        StartCoroutine(ReefHelper.FadeNormalized(ReefHelper.FadeType.In, 3f, 
            (x) => {
                Renderer.material.SetFloat("_Alpha", x);
                callback?.Invoke();
            }, 
            null));
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.ClampMagnitude(forward, 10f));

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.ClampMagnitude(targetDir, 10f));

        //Gizmos.color = Color.green;
        //Gizmos.DrawWireSphere(targetLocation, 0.25f);
    }
}
