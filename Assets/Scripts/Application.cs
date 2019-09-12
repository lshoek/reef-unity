﻿using UnityEngine;

public class Application : MonoBehaviour
{
    public static Application Instance { get; private set; }
    [HideInInspector] public Transform WorldParent;

    public Camera MainCamera { get; private set; }

    public KinectFrameManager KinectFrameManager { get; private set; }
    public CreatureManager CreatureManager { get; private set; }
    public BoidManager BoidManager { get; private set; }
    public BeatInfoManager BeatInfoManager { get; private set; }
    public KinectDepthManager KinectDepthManager { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        UnityEngine.Application.targetFrameRate = 60;
        WorldParent = GameObject.FindGameObjectWithTag("WorldParent").transform;

        MainCamera = Camera.main;
        KinectFrameManager = FindObjectOfType<KinectFrameManager>();
        CreatureManager = FindObjectOfType<CreatureManager>();
        BoidManager = FindObjectOfType<BoidManager>();
        BeatInfoManager = FindObjectOfType<BeatInfoManager>();
        KinectDepthManager = FindObjectOfType<KinectDepthManager>();
    }
}
