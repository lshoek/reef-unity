using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using Random = UnityEngine.Random;

public class CreatureManager : Show
    {
    public VideoClip[] VideoClips;

    public bool VideoCreaturesOnly = true;
    public int MaxCreatures = 4;
    public float CreatureScale = 0.75f;
    public int Layer = 1;

    public float EscapeDuration = 10f;
    public float FadeOutDuration = 10f;

    VideoPlayer[] videoPlayers;
    RenderTexture[] RT;

    Texture[] creatureTextures;
    Creature[] creatures;

    const int VIDEO_RT_RES = 1024;

    float elapsedTime;
    float lastSpawned;

    void Start()
    {
        creatureTextures = Resources.LoadAll<Texture>("Textures/Creatures");
        videoPlayers = new VideoPlayer[VideoClips.Length];
        RT = new RenderTexture[VideoClips.Length];

        for (int i = 0; i < VideoClips.Length; i++)
        {
            videoPlayers[i] = gameObject.AddComponent<VideoPlayer>();
            RT[i] = new RenderTexture(VIDEO_RT_RES, VIDEO_RT_RES, 16, RenderTextureFormat.ARGB32);

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
        }
    }

    private void ResetCreatures()
    {
        for (int i = 0; i < MaxCreatures; i++)
        {
            float height = 2f * Camera.main.orthographicSize;
            float width = height * Camera.main.aspect;
            Vector2 pos = new Vector2((i/(float)MaxCreatures*width)-width/2, Random.Range(height / 2, -height / 2));

            creatures[i].ColliderActive = false;
            creatures[i].transform.position = new Vector3(pos.x, pos.y, Layer);
            creatures[i].SetScale(CreatureScale);
            creatures[i].TurningSpeed = Random.Range(1.5f, 2.0f);
            creatures[i].Force = Random.Range(5.0f, 7.0f);
            creatures[i].Delay = Random.Range(1.0f, 3.0f);

            if (i < VideoClips.Length) creatures[i].Renderer.material.mainTexture = videoPlayers[i].targetTexture;
            else if (VideoCreaturesOnly) creatures[i].Renderer.material.mainTexture = videoPlayers[Random.Range(0, videoPlayers.Length)].targetTexture;
            else creatures[i].Renderer.material.mainTexture = creatureTextures[i - VideoClips.Length];
        }
        StartCoroutine(DelayColliderActivation());
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

    private Vector2 GetOffScreenVector()
    {
        return Random.insideUnitCircle * Camera.main.orthographicSize * 2.0f;
    }

    private IEnumerator CancelRoutine(Action callback)
    {
        // plan their escape
        foreach (Creature c in creatures)
            c.SetTargetLocation(GetOffScreenVector());

        // allocate time for escape
        yield return new WaitForSeconds(EscapeDuration);

        // fade out
        foreach (Creature c in creatures)
            StartCoroutine(c.FadeOut(FadeOutDuration));

        yield return new WaitForSeconds(FadeOutDuration);

        // stop video
        foreach (VideoPlayer vp in videoPlayers) vp.Stop();

        // mark as canceled
        callback();
    }

    #region "Overrides"
    public override void Renew()
    {
        ResetCreatures();

        foreach (VideoPlayer vp in videoPlayers)
            vp.Play();

        foreach (Creature c in creatures)
        {
            StartCoroutine(c.FadeIn(1f));
            c.SetFree();
        }
        base.Renew();
    }

    public override void Cancel()
    {
        StartCoroutine(CancelRoutine(() => base.Cancel()));
    }
    #endregion
}
