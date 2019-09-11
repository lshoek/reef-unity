using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using Random = UnityEngine.Random;

public class VideoPlaneManager : MonoBehaviour
{
    private VideoPlayer[] videoPlayer;
    private RenderTexture[] videoFrameBuffer;
    private VideoPlane[] videoPlane;

    public VideoClip[] videoClips;

    private const int NUM_BUFFERS = 2;
    private int videoIndex = 0;
    private int previousVideoIndex;

    private float defaultDuration = 5.0f;
    private float defaultWait = 5.0f;
    private float lastVideoSwitchTime;

    private bool enableRandomization = true;

    private int _front { get; set; } = 0;
    private int _back { get { return (_front + 1) % NUM_BUFFERS; } }

    private Coroutine activeRoutine;

    void Start()
    {
        videoFrameBuffer = new RenderTexture[NUM_BUFFERS];
        videoPlayer = new VideoPlayer[NUM_BUFFERS];
        videoPlane = new VideoPlane[NUM_BUFFERS];

        for (int i = 0; i < NUM_BUFFERS; i++)
        {
            videoFrameBuffer[i] = new RenderTexture(1920, 1080, 0, RenderTextureFormat.BGRA32);

            GameObject ob = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ob.transform.SetParent(Camera.main.transform);

            videoPlane[i] = ob.AddComponent<VideoPlane>();
            videoPlane[i].Init(i);
            videoPlane[i].SetVideoTexture(videoFrameBuffer[i]);

            videoPlayer[i] = gameObject.AddComponent<VideoPlayer>();
            videoPlayer[i].playOnAwake = false;
            videoPlayer[i].isLooping = true;

            videoPlayer[i].audioOutputMode = VideoAudioOutputMode.None;
            videoPlayer[i].renderMode = VideoRenderMode.RenderTexture;
            videoPlayer[i].targetTexture = videoFrameBuffer[i];
            videoPlayer[i].prepareCompleted += (vp) => { vp.Play(); };
        }
        _front = NUM_BUFFERS - 1;

        videoPlayer[_back].clip = videoClips[videoIndex + 1];
        videoPlayer[_front].clip = videoClips[videoIndex];
        videoPlayer[_front].Play();

        PrepareClip(videoPlayer[_back]);

        activeRoutine = StartCoroutine(ActionDelay(3f));
    }

    IEnumerator ActionDelay(float duration)
    {
        yield return new WaitForSeconds(duration);

        IEnumerator nextAction = SwapTransition(Random.Range(defaultDuration/5, defaultDuration*2), defaultWait);
        activeRoutine = StartCoroutine(nextAction);
    }

    IEnumerator SwapTransition(float duration, float wait)
    {
        videoPlane[_back].PlaneMaterial.SetFloat("_Alpha", 0);
        SwapBuffers();

        float time = 0;
        while (time <= duration)
        {
            time += Time.deltaTime;
            videoPlane[_front].PlaneMaterial.SetFloat("_Alpha", Mathf.Clamp(time / duration, 0, 1f));
            yield return null;
        }
        videoPlane[_front].PlaneMaterial.SetFloat("_Alpha", 1f);

        IEnumerator nextAction = SwapTransition(Random.Range(defaultDuration / 5, defaultDuration * 2), defaultWait);
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
            previousVideoIndex = videoIndex;
            videoIndex = Random.Range(0, videoClips.Length);
            videoIndex = (videoIndex == previousVideoIndex) ? (videoIndex + 1) % videoClips.Length : videoIndex;

            vp.clip = videoClips[videoIndex];
        }
        vp.Prepare();
    }
}
