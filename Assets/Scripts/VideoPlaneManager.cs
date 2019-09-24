using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using Random = UnityEngine.Random;

public class VideoPlaneManager : MonoBehaviour
{
    private BeatInfoManager m_beatInfoManager;

    private VideoPlayer[] videoPlayer;
    private RenderTexture[] videoFrameBuffer;
    private VideoPlane[] videoPlane;
    private VideoPlane brightnessPlane;

    public VideoClip[] videoClips;
    private VideoIndexStack videoIndexStack;

    public float MinDuration;
    public float MaxDuration;
    public float MinTransitionTime;
    public float MaxTransitionTime;

    public float TintMultiplier = 1f;
    public float DarknessLevel = 0.25f;

    public bool EnableColorizer = true;
    private bool colorizerOn = false;

    public Color AtmosphereColor = Color.white;
    public float AtmosphereRate = 0;
    private float colorizerDamping = 0;
    private float colorizerTime;
    private float noiseSeed;

    private const int NUM_BUFFERS = 2;
    private int videoIndex = 0;
    private int previousVideoIndex;

    private bool enableRandomization = true;

    private int _front { get; set; } = 0;
    private int _back { get { return (_front + 1) % NUM_BUFFERS; } }

    private Coroutine activeRoutine;

    void Start()
    {
        m_beatInfoManager = Application.Instance.BeatInfoManager;
        m_beatInfoManager.OnNormalizedAudioLevelInputHP += (x) => NormalizedAudioLevelInput(x);

        videoFrameBuffer = new RenderTexture[NUM_BUFFERS];
        videoPlayer = new VideoPlayer[NUM_BUFFERS];
        videoPlane = new VideoPlane[NUM_BUFFERS];

        for (int i = 0; i < NUM_BUFFERS; i++)
        {
            videoFrameBuffer[i] = new RenderTexture(1920, 1080, 0, RenderTextureFormat.BGRA32);

            GameObject ob = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ob.transform.SetParent(Application.Instance.MainCamera.transform);

            videoPlane[i] = ob.AddComponent<VideoPlane>();
            videoPlane[i].Init(i);
            videoPlane[i].SetVideoTexture(videoFrameBuffer[i]);
            videoPlane[i].PlaneMaterial.SetColor("_Tint", Color.black);
            videoPlane[i].PlaneMaterial.SetFloat("_TintPct", 0);

            videoPlayer[i] = gameObject.AddComponent<VideoPlayer>();
            videoPlayer[i].playOnAwake = false;
            videoPlayer[i].isLooping = true;

            videoPlayer[i].audioOutputMode = VideoAudioOutputMode.None;
            videoPlayer[i].renderMode = VideoRenderMode.RenderTexture;
            videoPlayer[i].targetTexture = videoFrameBuffer[i];
            videoPlayer[i].prepareCompleted += (vp) => { vp.Play(); };
        }
        // brightnessPlane
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        obj.transform.SetParent(Application.Instance.MainCamera.transform);
        brightnessPlane = obj.AddComponent<VideoPlane>();
        brightnessPlane.Init(NUM_BUFFERS + 1);
        brightnessPlane.SetVideoTexture(Resources.Load("Textures/Etc/Black") as Texture);
        brightnessPlane.PlaneMaterial.SetFloat("_Alpha", 0);

        _front = NUM_BUFFERS - 1;

        videoIndexStack = new VideoIndexStack(videoClips.Length/2);
        videoIndex = Random.Range(0, videoClips.Length);
        previousVideoIndex = videoIndex;

        videoPlayer[_back].clip = videoClips[(videoIndex + 1)%videoClips.Length];
        videoPlayer[_front].clip = videoClips[videoIndex];
        videoPlayer[_front].Play();

        PrepareClip(videoPlayer[_back]);

        activeRoutine = StartCoroutine(ActionDelay(3f));
        noiseSeed = Random.Range(0, 1024f);

        colorizerTime = Random.Range(8f, 30f);
        StartCoroutine(ColorizerRoutine());
    }

    void Update()
    {
        if (EnableColorizer)
        {
            AtmosphereColor = Color.HSVToRGB(Time.time/32%1f, 1, 1);
            AtmosphereRate = (Perlin.Noise(Time.time / 4 + noiseSeed)+1f)/2;
            for (int i = 0; i < NUM_BUFFERS; i++)
            {
                videoPlane[i].PlaneMaterial.SetColor("_AtmosCol", AtmosphereColor);
                videoPlane[i].PlaneMaterial.SetFloat("_AtmosPct", AtmosphereRate * colorizerDamping);
            }
        }
    }

    IEnumerator ColorizerRoutine()
    {
        while (EnableColorizer)
        {
            yield return new WaitForSeconds(colorizerTime);
            colorizerOn = !colorizerOn;
            colorizerTime = colorizerOn ? Random.Range(8f, 30f) : Random.Range(30f, 120f);

            if (colorizerOn)
            {
                for (int i = 0; i < NUM_BUFFERS; i++)
                {
                    StartCoroutine(ReefHelper.FadeNormalized(ReefHelper.FadeType.In, 4f,
                        (x) => colorizerDamping = x, null
                    ));
                }
            }
            else
            {
                for (int i = 0; i < NUM_BUFFERS; i++)
                {
                    StartCoroutine(ReefHelper.FadeNormalized(ReefHelper.FadeType.Out, 4f,
                        (x) => colorizerDamping = x, null
                    ));
                }
            }
        }
    }

    private void NormalizedAudioLevelInput(float level)
    {
        for (int i = 0; i < NUM_BUFFERS; i++)
            videoPlane[i].PlaneMaterial.SetFloat("_TintPct", level * TintMultiplier);
    }

    /// <summary>
    /// This routine is called once at Start
    /// </summary>
    IEnumerator ActionDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        IEnumerator nextAction = SwapTransition(Random.Range(MinTransitionTime, MaxTransitionTime), Random.Range(MinDuration, MaxDuration));
        activeRoutine = StartCoroutine(nextAction);
    }

    IEnumerator SwapTransition(float transitionTime, float wait)
    {
        videoPlane[_back].PlaneMaterial.SetFloat("_Alpha", 0);
        SwapBuffers();

        float time = 0;
        while (time <= transitionTime)
        {
            time += Time.deltaTime;
            videoPlane[_front].PlaneMaterial.SetFloat("_Alpha", Mathf.Clamp(time / transitionTime, 0, 1f));
            yield return null;
        }
        videoPlane[_front].PlaneMaterial.SetFloat("_Alpha", 1f);
        PrepareClip(videoPlayer[_back]);

        yield return new WaitForSeconds(wait);

        IEnumerator nextAction = SwapTransition(Random.Range(MinDuration, MaxDuration), Random.Range(MinTransitionTime, MaxTransitionTime));
        activeRoutine = StartCoroutine(nextAction);
    }

    void SwapBuffers()
    {
        for (int i = 0; i < NUM_BUFFERS; i++)
            videoPlane[i].depth = (videoPlane[i].depth + 1) % NUM_BUFFERS;

        _front = _back;
    }

    void PrepareClip(VideoPlayer vp)
    {
        if (enableRandomization)
        {
            int attempts = 4;
            int next = 0;
            for (int i = 0; i < attempts; i++)
            {
                next = Random.Range(0, videoClips.Length);
                if (!videoIndexStack.Contains(next)) break;
            }
            previousVideoIndex = videoIndex;
            videoIndex = next;
            videoIndexStack.Push(next);

            vp.clip = videoClips[videoIndex];
        }
        vp.Prepare();
    }

    public void DarkenBackground(bool enable)
    {
        if (enable)
        {
            StartCoroutine(ReefHelper.FadeNormalized(ReefHelper.FadeType.In, 2f,
                (x) => brightnessPlane.PlaneMaterial.SetFloat("_Alpha", x * DarknessLevel), 
                null));
        }
        else
        {
            StartCoroutine(ReefHelper.FadeNormalized(ReefHelper.FadeType.Out, 2f,
                (x) => brightnessPlane.PlaneMaterial.SetFloat("_Alpha", x * DarknessLevel),
                null));
        }
    }
}

public class VideoIndexStack
{
    public int[] Stack { get; private set; }
    private int index = 0;

    public VideoIndexStack(int size)
    {
        Stack = new int[size];
    }

    public void Push(int i)
    {
        Stack[index] = i;
        index = (index + 1) % Stack.Length;
    }

    public bool Contains(int i)
    {
        for (int j=0; i<Stack.Length; i++)
            if (i == Stack[j]) return true;

        return false;
    }
}
