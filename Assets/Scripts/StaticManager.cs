﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using Random = UnityEngine.Random;

public class StaticManager : Show
{
    private CreatureClips m_clips;

    public int Layer = 1;
    public float Scale = 2f;
    public float FadeOutDuration = 5f;

    public Color TintColor = Color.white;

    private BeatInfoManager m_beatInfoManager;
    private VideoPlayer videoPlayer;
    private RenderTexture RT;

    //private Texture[] creatureTextures;
    private StaticCreature creature;

    private const int VIDEO_RT_RES = 1024;

    void Start()
    {
        m_clips = Application.Instance.CreatureClips;
        m_beatInfoManager = Application.Instance.BeatInfoManager;

        //creatureTextures = Resources.LoadAll<Texture>("Textures/Creatures");
        videoPlayer = new VideoPlayer();

        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        RT = new RenderTexture(VIDEO_RT_RES, VIDEO_RT_RES, 0, RenderTextureFormat.ARGB32);

        videoPlayer.isLooping = true;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.clip = m_clips.GetRandom();
        videoPlayer.targetTexture = RT;

        GameObject ob = Instantiate(Resources.Load("Prefabs/StaticCreature") as GameObject);

        ob.name = $"StaticCreature";
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
        videoPlayer.clip = m_clips.GetRandom();
        creature.Renderer.material.mainTexture = videoPlayer.targetTexture;
        //creatureTextures[Random.Range(0, creatureTextures.Length)];

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