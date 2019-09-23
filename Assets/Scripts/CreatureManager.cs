﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using Random = UnityEngine.Random;

public class CreatureManager : Show
{
    public bool VideoCreaturesOnly = true;
    public int MaxCreatures = 4;
    public float CreatureScale = 0.75f;
    public int Layer = 1;

    public float EscapeDuration = 5f;
    public float FadeOutDuration = 5f;

    public Color TintColor = Color.white;

    private CreatureClips m_clips;
    private BeatInfoManager m_beatInfoManager;
    private VideoPlayer[] videoPlayers;
    private RenderTexture[] RT;

    private Texture[] creatureTextures;
    private Creature[] creatures;

    private const int VIDEO_RT_RES = 1024;

    void Start()
    {
        m_clips = Application.Instance.CreatureClips;
        m_beatInfoManager = Application.Instance.BeatInfoManager;

        creatureTextures = Resources.LoadAll<Texture>("Textures/Creatures");
        videoPlayers = new VideoPlayer[m_clips.Clips.Length];
        RT = new RenderTexture[m_clips.Clips.Length];

        for (int i = 0; i < m_clips.Clips.Length; i++)
        {
            videoPlayers[i] = gameObject.AddComponent<VideoPlayer>();
            RT[i] = new RenderTexture(VIDEO_RT_RES, VIDEO_RT_RES, 0, RenderTextureFormat.ARGB32);

            videoPlayers[i].isLooping = true;
            videoPlayers[i].audioOutputMode = VideoAudioOutputMode.None;
            videoPlayers[i].renderMode = VideoRenderMode.RenderTexture;
            videoPlayers[i].clip = m_clips.Clips[i];
            videoPlayers[i].targetTexture = RT[i];
        }

        creatures = new Creature[MaxCreatures];
        for (int i = 0; i < MaxCreatures; i++)
        {
            GameObject ob = Instantiate(Resources.Load("Prefabs/Creature") as GameObject);

            ob.name = $"Creature{i}";
            ob.transform.SetParent(Application.Instance.WorldParent);
            creatures[i] = ob.GetComponent<Creature>();
            creatures[i].Init(this);
        }
    }

    private void ResetCreatures()
    {
        for (int i = 0; i < MaxCreatures; i++)
        {
            Vector2 pos = new Vector2((i / (float)MaxCreatures * ReefHelper.DisplayWidth) - ReefHelper.DisplayWidth / 2,
                Random.Range(ReefHelper.DisplayHeight / 2, -ReefHelper.DisplayHeight / 2));

            creatures[i].IsActive = true;
            creatures[i].AudioSensitive = true;
            creatures[i].ColliderActive = false;
            creatures[i].transform.position = new Vector3(pos.x, pos.y, Layer);
            creatures[i].SetScale(CreatureScale);

            creatures[i].TurningSpeed = Random.Range(0.25f, 1f);
            creatures[i].Force = Random.Range(2f, 4f);
            creatures[i].Delay = Random.Range(0.75f, 2f);
        }
        StartCoroutine(DelayColliderActivation());

        // reset textures and start movement
        for (int i = 0; i < MaxCreatures; i++)
        {
            if (i < m_clips.Clips.Length) creatures[i].Renderer.material.mainTexture = videoPlayers[i].targetTexture;
            else creatures[i].Renderer.material.mainTexture = videoPlayers[Random.Range(0, videoPlayers.Length)].targetTexture;
        }
    }

    private IEnumerator DelayColliderActivation()
    {
        // reset rigidbodies
        for (int i = 0; i < MaxCreatures; i++)
        {
            creatures[i].Collider.attachedRigidbody.velocity = new Vector3(0f, 0f, 0f);
            creatures[i].Collider.attachedRigidbody.angularVelocity = new Vector3(0f, 0f, 0f);
        }
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < MaxCreatures; i++) creatures[i].ColliderActive = true;
    }

    private IEnumerator CancelRoutine(Action callback)
    {
        // plan their escape
        foreach (Creature c in creatures)
            c.Escape();

        // allocate time for escape
        yield return new WaitForSeconds(EscapeDuration);

        // fade out
        foreach (Creature c in creatures)
            c.FadeOut(FadeOutDuration, null);

        yield return new WaitForSeconds(FadeOutDuration);

        // stop video
        foreach (VideoPlayer vp in videoPlayers) vp.Stop();

        // mark as canceled
        callback();
    }

    #region "Overrides"
    public override void Renew()
    {
        m_beatInfoManager.SetActive(true);
        ResetCreatures();

        foreach (VideoPlayer vp in videoPlayers)
            vp.Play();

        foreach (Creature c in creatures)
        {
            c.FadeIn(1f, null);
            c.StartMovement();
        }
        base.Renew();
    }

    public override void Cancel()
    {
        StartCoroutine(CancelRoutine(() => base.Cancel()));
    }
    #endregion
}
