﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoPlane : MonoBehaviour
{
    public int depth = 0;
    Camera m_camera;

    public Material PlaneMaterial;

    public void Init(int d)
    {
        m_camera = GetComponentInParent<Camera>();
        depth = d;
        gameObject.name = $"VideoPlane{depth}";

        if (!m_camera.orthographic) Debug.Log("Warning! This script only works with orthographic cameras");

        transform.localRotation = Quaternion.Euler(-90.0f, 0, 0) * Quaternion.Euler(0, 180.0f, 0);
        transform.localPosition = new Vector3(0, 0, m_camera.farClipPlane + depth);

        float height = 2f * m_camera.orthographicSize;
        float width = height * m_camera.aspect;
        transform.localScale = new Vector3(width / m_camera.orthographicSize, 1.0f, height / m_camera.orthographicSize);

        GetComponent<Renderer>().material = Resources.Load("Materials/VideoPlaneMaterial") as Material;
        PlaneMaterial = GetComponent<Renderer>().material;
    }

    public void SetVideoTexture(RenderTexture rt)
    {
        PlaneMaterial.mainTexture = rt;
    }

    void Update()
    {
        transform.localPosition = new Vector3(0, 0, m_camera.farClipPlane - depth);
    }
}
