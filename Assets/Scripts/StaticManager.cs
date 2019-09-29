using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;
using Random = UnityEngine.Random;

public class StaticManager : Show
{
    private CreatureDataAccessor m_dataAccessor;

    public int Layer = 1;
    public float Scale = 1.0f;
    public float FadeOutDuration = 5f;
    public bool AlwaysUseVideo = false;

    public Color TintColor = Color.white;

    private BeatInfoManager m_beatInfoManager;
    private VideoPlayer videoPlayer;
    private RenderTexture RT;

    private Texture[] creatureTextures;
    private StaticCreature creature;

    private bool useVideo = true;
    private const int VIDEO_RT_RES = 1024;

    void Start()
    {
        m_dataAccessor = Application.Instance.CreatureDataAccessor;
        m_beatInfoManager = Application.Instance.BeatInfoManager;

        creatureTextures = Resources.LoadAll("Textures/Particles", typeof(Texture)).Cast<Texture>().ToArray();
        videoPlayer = new VideoPlayer();

        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        RT = new RenderTexture(VIDEO_RT_RES, VIDEO_RT_RES, 0, RenderTextureFormat.ARGB32);

        videoPlayer.isLooping = true;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.clip = m_dataAccessor.GetRandomClip();
        videoPlayer.targetTexture = RT;

        GameObject ob = Instantiate(Resources.Load("Prefabs/StaticCreature") as GameObject);

        ob.name = "StaticCreature";
        ob.transform.SetParent(Application.Instance.WorldParent);
        creature = ob.GetComponent<StaticCreature>();

        creature.Renderer.material.mainTexture = videoPlayer.targetTexture;
        creature.transform.position = new Vector3(0, 0, Layer);
        creature.SetScale(Scale);
        creature.AudioSensitive = true; 
        creature.Init(this);
    }

    private void ResetCreature()
    {
        videoPlayer.clip = m_dataAccessor.GetRandomClip();

        useVideo = (AlwaysUseVideo) ? true : Random.Range(0, m_dataAccessor.Particles.Length + m_dataAccessor.Clips.Length) >= m_dataAccessor.Particles.Length;

        creature.Renderer.material.mainTexture = useVideo ? 
            videoPlayer.targetTexture : 
            creatureTextures[Random.Range(0, creatureTextures.Length)];

        creature.EnableRotation = Random.Range(0, 2) > 0;
        creature.IsActive = true;
    }

    private IEnumerator CancelRoutine(Action callback)
    {
        bool fadeOver = false;
        creature.FadeOut(FadeOutDuration, () => fadeOver = true);
        yield return new WaitUntil(() => fadeOver);

        videoPlayer.Stop();
        callback();
    }

    #region "Overrides"
    public override void Renew()
    {
        m_beatInfoManager.SetActive(true);
        ResetCreature();

        videoPlayer.Play();
        creature.FadeIn(1f, null);

        base.Renew();
    }

    public override void Cancel()
    {
        StartCoroutine(CancelRoutine(() => base.Cancel()));
    }
    #endregion
}
