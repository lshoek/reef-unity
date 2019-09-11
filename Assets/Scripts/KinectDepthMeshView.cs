using UnityEngine;
using Windows.Kinect;

public class KinectDepthMeshView : MonoBehaviour
{
    KinectFrameManager m_frameManager;
    Camera m_camera;

    [SerializeField] float depthRescale = 100f;
    private int maxDepth;

    private DepthMesh kinectDepthMesh;
    private MeshCollider kinectMeshCollider;
    private MeshFilter kinectMeshFilter;

    void Start()
    {
        m_frameManager = FindObjectOfType<KinectFrameManager>();
        m_camera = Camera.main;

        kinectMeshCollider = GetComponent<MeshCollider>();
        kinectMeshFilter = GetComponent<MeshFilter>();

        // downsample to lower resolution
        kinectDepthMesh = new DepthMesh(m_frameManager.DepthWidthDownsampled, m_frameManager.DepthHeightDownsampled);

        float depthAspect = m_frameManager.DepthWidthDownsampled / m_frameManager.DepthHeightDownsampled;
        transform.parent.localPosition = new Vector3(-ReefHelper.DisplayHeight * depthAspect/2, ReefHelper.DisplayHeight / 2);

        Vector2 meshSize = new Vector2(ReefHelper.DisplayHeight * depthAspect, ReefHelper.DisplayHeight);
        kinectDepthMesh.SetOffset(new Vector2(0.5f, 0.5f));
        kinectDepthMesh.Init(meshSize, false);

        maxDepth = m_frameManager.MaxReliableDistance;
    }

    private void Update()
    {
        ushort[] depthData = m_frameManager.GetDepthData();
        UpdateDepthMesh(kinectDepthMesh, depthData, depthRescale, m_frameManager.ColliderMeshDownsampling);

        kinectMeshCollider.sharedMesh = kinectDepthMesh.mesh;
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

                float depth = depthData[fullSampleIndex + downSampleSize / 2];
                depth = (depth < maxDepth) ? depth : maxDepth;
                depth = (depth == 0) ? maxDepth : depth;
                depth = depth / maxDepth * scale;

                depthMesh.verts[downSampleIndex].z = depth;
            }
        }
        depthMesh.Apply();
    }
}
