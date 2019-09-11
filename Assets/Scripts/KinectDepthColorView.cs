using UnityEngine;

public class KinectDepthColorView : MonoBehaviour
{
    KinectFrameManager m_kinectFrameManager;

    [Range(1.0f, 400.0f)] [SerializeField] float depthRescale = 400.0f;

    private const int ColliderMeshDownsampling = 8;
    private const int MaxDepth = 4500;

    private DepthMesh kinectDepthMesh;
    private MeshFilter kinectMeshFilter;
    private MeshCollider kinectMeshCollider;
    private Renderer kinectMeshRenderer;

    void Start()
    {
        m_kinectFrameManager = FindObjectOfType<KinectFrameManager>();

        kinectMeshFilter = GetComponent<MeshFilter>();
        kinectMeshCollider = GetComponent<MeshCollider>();
        kinectMeshRenderer = GetComponent<Renderer>();

        // downsample to lower resolution
        kinectDepthMesh = new DepthMesh(m_kinectFrameManager.DepthFrameWidth / ColliderMeshDownsampling, m_kinectFrameManager.DepthFrameHeight / ColliderMeshDownsampling);

        kinectMeshFilter.mesh = new Mesh();
        kinectMeshFilter.mesh.MarkDynamic();
    }

    void Update()
    {
        UpdateDepthMesh(kinectDepthMesh, m_kinectFrameManager.GetDepthData(), depthRescale, ColliderMeshDownsampling);

        kinectMeshCollider.sharedMesh = kinectDepthMesh.mesh;
        kinectMeshRenderer.material.mainTexture = m_kinectFrameManager.GetColorTexture();
    }

    private void UpdateDepthMesh(DepthMesh depthMesh, ushort[] depthData, float scale, int downSampleSize)
    {
        for (int y = 0; y < m_kinectFrameManager.DepthFrameHeight; y += downSampleSize)
        {
            for (int x = 0; x < m_kinectFrameManager.DepthFrameWidth; x += downSampleSize)
            {
                int idx = x / downSampleSize;
                int idy = y / downSampleSize;

                int fullIndex = y * m_kinectFrameManager.DepthFrameWidth + x;
                int smallIndex = (idy * (m_kinectFrameManager.DepthFrameWidth / downSampleSize)) + idx;

                float depth = depthData[fullIndex];
                depth = (depth < MaxDepth) ? depth : MaxDepth;
                depth = (depth == 0) ? MaxDepth : depth;
                depth = (depth / MaxDepth) * scale;

                depthMesh.OrigVerts[smallIndex].z = depth;
            }
        }
        depthMesh.Apply();
    }
}
