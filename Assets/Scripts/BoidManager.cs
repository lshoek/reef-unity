using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using Random = UnityEngine.Random;

public class BoidManager : Show
{
    public int NumBoids = 64;
    public int Layer = 2;
    public float FadeOutDuration = 5f;

    const int THREAD_GROUPSIZE = 1024;

    private ComputeShader computeShader;
    private BoidSettings settings;
    private Boid[] boids;

    private VideoPlayer videoPlayer;
    private RenderTexture RT;

    private CreatureClips m_clips;
    private KinectDepthManager m_depthManager;
    private BeatInfoManager m_beatInfoManager;

    private int boidClipIndex = 0;
    private const int VIDEO_RT_RES = 128;

    void Start()
    {
        m_depthManager = Application.Instance.KinectDepthManager;
        m_beatInfoManager = Application.Instance.BeatInfoManager;
        m_clips = Application.Instance.CreatureClips;

        settings = new BoidSettings();
        settings.obstacleMask = 1 << LayerMask.NameToLayer("Bounds") | 1 << LayerMask.NameToLayer("ColliderMesh");
        settings.layer = Layer;

        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.isLooping = true;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.clip = m_clips.GetRandom();

        RT = new RenderTexture(VIDEO_RT_RES, VIDEO_RT_RES, 0, RenderTextureFormat.ARGB32);
        videoPlayer.targetTexture = RT;
        videoPlayer.Play();

        boids = new Boid[NumBoids];
        for (int i = 0; i < NumBoids; i++)
        {
            GameObject ob = Instantiate(Resources.Load("Prefabs/Boid") as GameObject);
            ob.name = $"Boid{i}";
            ob.transform.SetParent(Application.Instance.WorldParent);
            boids[i] = ob.GetComponent<Boid>();
            boids[i].Init(settings);
        }
        computeShader = Resources.Load("ComputeShaders/Boids") as ComputeShader;
    }

    private void ResetBoids()
    {
        for (int i = 0; i < NumBoids; i++)
        {
            Vector2 pos = Random.insideUnitCircle * Application.Instance.MainCamera.orthographicSize * Random.Range(0, 1.5f);
            Vector2 forward = Random.insideUnitCircle;

            boids[i].transform.position = new Vector3(pos.x, pos.y, Layer);
            boids[i].transform.forward = new Vector3(forward.x, forward.y);
            boids[i].Renderer.material.SetTexture("_MainTex", RT);
        }
        boidClipIndex = Random.Range(0, m_clips.Clips.Length);
        videoPlayer.clip = m_clips.Clips[boidClipIndex];
    }

    void Update()
    {
        if (boids != null && base.Active)
        {
            // calculate necessary flock data 
            int numBoids = boids.Length;
            var boidData = new BoidData[numBoids];

            for (int i = 0; i < boids.Length; i++)
            {
                boidData[i].position = boids[i].position;
                boidData[i].direction = boids[i].forward;
            }

            var boidBuffer = new ComputeBuffer(numBoids, BoidData.Size);
            boidBuffer.SetData(boidData);

            computeShader.SetBuffer(0, "boids", boidBuffer);
            computeShader.SetInt("numBoids", boids.Length);
            computeShader.SetFloat("viewRadius", settings.perceptionRadius);
            computeShader.SetFloat("avoidRadius", settings.avoidanceRadius);

            int threadGroups = Mathf.CeilToInt(numBoids / (float)THREAD_GROUPSIZE);
            computeShader.Dispatch(0, threadGroups, 1, 1);

            boidBuffer.GetData(boidData);

            for (int i = 0; i < boids.Length; i++)
            {
                boids[i].avgFlockHeading = boidData[i].flockHeading;
                boids[i].centreOfFlockmates = boidData[i].flockCentre;
                boids[i].avgAvoidanceHeading = boidData[i].avoidanceHeading;
                boids[i].numPerceivedFlockmates = boidData[i].numFlockmates;

                boids[i].UpdateBoid();
            }
            boidBuffer.Release();
        }
    }

    private IEnumerator CancelRoutine(Action callback)
    {
        foreach (Boid b in boids)
            b.FadeOut(FadeOutDuration);

        yield return new WaitForSeconds(FadeOutDuration);
        foreach (Boid b in boids)
            b.IsActive = false;

        m_depthManager.SetActive(false);
        callback();
    }

    #region "Overrides"
    public override void Renew()
    {
        if (m_beatInfoManager.IsActive) m_beatInfoManager.SetActive(false);

        ResetBoids();
        foreach (Boid b in boids)
        {
            b.IsActive = true;
            b.FadeIn(1f);
        }
        m_depthManager.SetActive(true);
        base.Renew();
    }

    public override void Cancel()
    {
        StartCoroutine(CancelRoutine(() => base.Cancel()));
    }
    #endregion

    public struct BoidData
    {
        public Vector2 position;
        public Vector2 direction;

        public Vector2 flockHeading;
        public Vector2 flockCentre;
        public Vector2 avoidanceHeading;
        public int numFlockmates;

        public static int Size { get { return sizeof(float) * 2 * 5 + sizeof(int); } }
    }
}
