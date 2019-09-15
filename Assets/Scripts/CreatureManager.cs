using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using Random = UnityEngine.Random;

public class CreatureManager : Show
{
    public Creature.CreatureBehavior CurrentBehavior;
    public VideoClip[] VideoClips;

    public bool VideoCreaturesOnly = true;
    public int MaxCreatures = 4;
    public float CreatureScale = 0.75f;
    public int Layer = 1;

    public float EscapeDuration = 10f;
    public float FadeOutDuration = 10f;

    public Color TintColor = Color.white;

    private VideoPlayer[] videoPlayers;
    private RenderTexture[] RT;

    private Texture[] creatureTextures;
    private Creature[] creatures;

    private Creature alphaCreature;

    private const int VIDEO_RT_RES = 1024;

    private float elapsedTime;
    private float lastSpawned;
    private float lastPeak;
    private bool beatToggle = true;

    void Start()
    {
        creatureTextures = Resources.LoadAll<Texture>("Textures/Creatures");
        videoPlayers = new VideoPlayer[VideoClips.Length];
        RT = new RenderTexture[VideoClips.Length];

        for (int i = 0; i < VideoClips.Length; i++)
        {
            videoPlayers[i] = gameObject.AddComponent<VideoPlayer>();
            RT[i] = new RenderTexture(VIDEO_RT_RES, VIDEO_RT_RES, 0, RenderTextureFormat.ARGB32);

            videoPlayers[i].isLooping = true;
            videoPlayers[i].audioOutputMode = VideoAudioOutputMode.None;
            videoPlayers[i].renderMode = VideoRenderMode.RenderTexture;
            videoPlayers[i].clip = VideoClips[i];
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
        alphaCreature = creatures[0];
    }

    private void ResetCreatures()
    {
        if (CurrentBehavior == Creature.CreatureBehavior.Reactive)
        {
            for (int i = 0; i < MaxCreatures; i++)
            {
                Vector2 pos = new Vector2((i / (float)MaxCreatures * ReefHelper.DisplayWidth) - ReefHelper.DisplayWidth / 2,
                    Random.Range(ReefHelper.DisplayHeight / 2, -ReefHelper.DisplayHeight / 2));

                creatures[i].IsActive = true;
                creatures[i].CurrentBehavior = Creature.CreatureBehavior.Reactive;
                creatures[i].AudioSensitive = true;
                creatures[i].ColliderActive = false;
                creatures[i].transform.position = new Vector3(pos.x, pos.y, Layer);
                creatures[i].SetScale(CreatureScale);

                creatures[i].TurningSpeed = Random.Range(0.25f, 1f);
                creatures[i].Force = Random.Range(3f, 5f);
                creatures[i].Delay = Random.Range(0.75f, 2f);
            }
        }
        else if (CurrentBehavior == Creature.CreatureBehavior.Static)
        {
            for (int i = 0; i < MaxCreatures; i++)
            {
                creatures[i].IsActive = creatures[i].Equals(alphaCreature);
                creatures[i].CurrentBehavior = Creature.CreatureBehavior.Static;
                creatures[i].AudioSensitive = true;
                creatures[i].ColliderActive = false;
            }
            // display a single creature
            alphaCreature.Collider.attachedRigidbody.isKinematic = true;
            alphaCreature.transform.position = new Vector3(0, 0, Layer);
            alphaCreature.SetScale(2f);
        }
        StartCoroutine(DelayColliderActivation());

        // reset textures and start movement
        for (int i = 0; i < MaxCreatures; i++)
        {
            if (i < VideoClips.Length) creatures[i].Renderer.material.mainTexture = videoPlayers[i].targetTexture;
            else if (VideoCreaturesOnly) creatures[i].Renderer.material.mainTexture = videoPlayers[Random.Range(0, videoPlayers.Length)].targetTexture;
            else creatures[i].Renderer.material.mainTexture = creatureTextures[i - VideoClips.Length];
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
            c.FadeOut(FadeOutDuration);

        yield return new WaitForSeconds(FadeOutDuration);

        // stop video
        foreach (VideoPlayer vp in videoPlayers) vp.Stop();
        alphaCreature.Collider.attachedRigidbody.isKinematic = false;

        // mark as canceled
        callback();
    }

    #region "Overrides"
    public override void Renew()
    {
        CurrentBehavior = Random.Range(0, 4) > 0 ? Creature.CreatureBehavior.Static : Creature.CreatureBehavior.Reactive;
        ResetCreatures();

        foreach (VideoPlayer vp in videoPlayers)
            vp.Play();

        foreach (Creature c in creatures)
        {
            c.FadeIn(1f);
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
