﻿using UnityEngine;

public class Application : MonoBehaviour
{
    public static Application Instance { get; private set; }
    [HideInInspector] public Transform WorldParent;

    public Camera MainCamera { get; private set; }
    public float FarClipPlaneDistance { get; private set; }

    public KinectFrameManager KinectFrameManager { get; private set; }
    public StaticManager StaticManager { get; private set; }
    public CreatureManager CreatureManager { get; private set; }
    public BoidManager BoidManager { get; private set; }
    public BeatInfoManager BeatInfoManager { get; private set; }
    public KinectDepthManager KinectDepthManager { get; private set; }
    public TitleManager TitleManager { get; private set; }
    public VideoPlaneManager VideoPlaneManager { get; private set; }
    public Scheduler Scheduler { get; private set; }
    public CreatureDataAccessor CreatureDataAccessor { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        Cursor.visible = false;

        MainCamera = Camera.main;
        FarClipPlaneDistance = MainCamera.farClipPlane;
        UnityEngine.Application.targetFrameRate = 60;
        WorldParent = GameObject.FindGameObjectWithTag("WorldParent").transform;

        KinectFrameManager = FindObjectOfType<KinectFrameManager>();
        StaticManager = FindObjectOfType<StaticManager>();
        CreatureManager = FindObjectOfType<CreatureManager>();
        BoidManager = FindObjectOfType<BoidManager>();
        BeatInfoManager = FindObjectOfType<BeatInfoManager>();
        KinectDepthManager = FindObjectOfType<KinectDepthManager>();
        TitleManager = FindObjectOfType<TitleManager>();
        VideoPlaneManager = FindObjectOfType<VideoPlaneManager>();
        Scheduler = FindObjectOfType<Scheduler>();
        CreatureDataAccessor = FindObjectOfType<CreatureDataAccessor>();
    }
}
