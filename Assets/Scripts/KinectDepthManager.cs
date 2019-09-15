using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinectDepthManager : MonoBehaviour
{
    private KinectFrameManager m_frameManager;
    private Camera m_camera;
    private DepthMesh m_kinectDepthMesh;
    private ushort maxDepth;

    [Range(0, 0.1f)]
    public float DepthScale = 0.05f;
    public bool IsActive { get; private set; } = false;

    public event Action<bool> OnActivationChanged;

    public DepthMesh GetDepthMesh()
    {
        return m_kinectDepthMesh;
    }

    void Start()
    {
        m_frameManager = Application.Instance.KinectFrameManager;
        m_camera = Application.Instance.MainCamera;

        // downsample to lower resolution
        m_kinectDepthMesh = new DepthMesh(m_frameManager.DepthWidthDownsampled, m_frameManager.DepthHeightDownsampled);

        float depthAspect = m_frameManager.DepthWidthDownsampled / m_frameManager.DepthHeightDownsampled;
        Vector2 meshSize = new Vector2(ReefHelper.DisplayHeight * depthAspect, ReefHelper.DisplayHeight);

        m_kinectDepthMesh.SetOffset(new Vector2(0.5f, 0.5f));
        m_kinectDepthMesh.Init(meshSize, false);

        maxDepth = m_frameManager.MaxReliableDistance;
        DepthScale = Application.Instance.MainCameraToBackgroundPlaneDistance*1.2f / maxDepth;
    }

    void Update()
    {
        if (IsActive)
        {
            ushort[] depthData = m_frameManager.GetDepthData();
            UpdateDepthMesh(m_kinectDepthMesh, depthData, DepthScale, m_frameManager.ColliderMeshDownsampling);
        }
    }

    private void UpdateDepthMesh(DepthMesh depthMesh, ushort[] depthData, float scale, int downSampleSize)
    {
        int maxIndex = m_frameManager.MaxDepthSamples;
        for (int y = 0; y < m_frameManager.DepthFrameHeight; y += downSampleSize)
        {
            for (int x = 0; x < m_frameManager.DepthFrameWidth; x += downSampleSize)
            {
                int idx = x / downSampleSize;
                int idy = y / downSampleSize;

                int fullSampleIndex = y * m_frameManager.DepthFrameWidth + x;
                int downSampleIndex = (idy * (m_frameManager.DepthFrameWidth / downSampleSize)) + idx;
                downSampleIndex = (downSampleIndex >= maxIndex) ? maxIndex - 1 : downSampleIndex;

                ushort d = depthData[fullSampleIndex + downSampleSize / 2];
                d = (d < maxDepth) ? d : maxDepth;
                d = (d == 0) ? maxDepth : d;
                float depth = d*scale + Application.Instance.MainCameraToBackgroundPlaneDistance - maxDepth * scale;

                depthMesh.verts[downSampleIndex].z = depth;
            }
        }
        depthMesh.Apply();
    }

    /// <summary>
    /// When set to false, also sets all z-values of mesh to zero.
    /// </summary>
    /// <param name="active"></param>
    public void SetActive(bool active)
    {
        IsActive = active;
        if (!active)
        {
            for (int i = 0; i < m_kinectDepthMesh.verts.Length; i++)
                m_kinectDepthMesh.verts[i].z = 0;

            m_kinectDepthMesh.Apply();
        }
        OnActivationChanged?.Invoke(active);
    }
}
