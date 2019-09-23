using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class StaticCreature : MonoBehaviour
{
    private StaticManager m_manager;
    private BeatInfoManager m_beatInfoManager;

    public Renderer Renderer { get; private set; }

    public bool AudioSensitive = true;

    public Color TintColor = Color.white;
    public float NoiseMult = 8f;
    public float LevelDamping = 0.0625f;

    private Vector2 position;
    private Vector2 forward;
    private float cachedScale = 1.0f;

    private bool isActive = false;
    public bool IsActive
    {
        get { return isActive; }
        set
        {
            if (value) m_beatInfoManager.OnNormalizedAudioLevelInput += (x) => NormalizedAudioLevelInput(x);
            else m_beatInfoManager.OnNormalizedAudioLevelInput -= (x) => NormalizedAudioLevelInput(x);
            Renderer.enabled = value;
            isActive = value;
        }
    }

    void Awake()
    {
        Renderer = GetComponentInChildren<Renderer>();
    }

    void Start()
    {
        m_beatInfoManager = Application.Instance.BeatInfoManager;

        position = transform.position;
        forward = (position + Random.insideUnitCircle).normalized;
    }

    public void Init(StaticManager manager)
    {
        m_manager = manager;
    }

    void Update()
    {
        float angle = Perlin.Noise(Time.time / 2) * NoiseMult;
        transform.localRotation = Quaternion.Euler(0, 0, angle - 180f);
    }

    public void SetScale(float scale)
    {
        cachedScale = scale;
        transform.localScale = new Vector3(cachedScale, cachedScale, 1f);
    }

    private void NormalizedAudioLevelInput(float level)
    {
        Renderer.material.SetFloat("_TintPct", level);
        Renderer.material.SetColor("_Tint", TintColor);
        transform.localScale = new Vector3(cachedScale + level*LevelDamping, cachedScale + level*LevelDamping, 1f);
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
}
