using UnityEngine;
using Windows.Kinect;

public class KinectDepthColorView : MonoBehaviour
{
    public int Depth = 2;
    public Color AmbientColor = Color.white;
    [Range(0, 1f)] public float AmbientColorRate = 1f;
    [Range(0, 50f)] public float AmbientColorMult = 1f;
    [Range(0, 0.1f)] public float ShaderDepthMin = 0.0025f;
    [Range(0, 0.1f)] public float ShaderDepthMinRamp = 0.025f;
    [Range(0, 0.1f)] public float ShaderDepthMax = 0.01f;
    [Range(0, 0.1f)] public float ShaderDepthMaxRamp = 0.0025f;

    private int m_pColorDownsampleSize = 2;

    private KinectFrameManager m_frameManager;
    private KinectDepthManager m_depthManager;
    private Camera m_camera;

    private RenderTexture m_pKinectDepthColorRT;
    private ComputeBuffer m_pDepthComputeBuffer;
    private Vector2[] m_pColorToDepth;

    private Material m_pTextureMixMaterial;
    private Renderer m_pRenderer;
    private MeshFilter m_pMeshFilter;
    private VideoPlane m_pKinectVideoPlane;

    void Start()
    {
        m_frameManager = Application.Instance.KinectFrameManager;
        m_depthManager = Application.Instance.KinectDepthManager;

        m_pRenderer = GetComponent<Renderer>();
        m_pMeshFilter = GetComponent<MeshFilter>();
        m_camera = Application.Instance.MainCamera;

        m_pTextureMixMaterial = new Material(Resources.Load("Shaders/KinectDepthColorMixer") as Shader);
        m_pKinectDepthColorRT = new RenderTexture(m_frameManager.DepthFrameWidth, m_frameManager.DepthFrameHeight, 0, RenderTextureFormat.ARGB32);

        m_pColorToDepth = new Vector2[m_frameManager.DepthFrameWidth / m_pColorDownsampleSize * m_frameManager.DepthFrameHeight / m_pColorDownsampleSize];

        GameObject ob = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ob.transform.SetParent(m_camera.transform);

        float depthAspect = m_frameManager.DepthWidthDownsampled / m_frameManager.DepthHeightDownsampled;

        m_pKinectVideoPlane = ob.AddComponent<VideoPlane>();
        m_pKinectVideoPlane.Init(Depth);
        m_pKinectVideoPlane.SetWidth(ReefHelper.DisplayHeight * depthAspect);
        m_pKinectVideoPlane.SetVideoTexture(m_pKinectDepthColorRT);

        m_pTextureMixMaterial.SetTexture("_ColorTex", m_frameManager.GetColorTexture());
        m_pTextureMixMaterial.SetTexture("_DepthTex", m_frameManager.GetDepthTexture());
        m_pTextureMixMaterial.SetFloat("_DownsampleSize", m_pColorDownsampleSize);

        ReleaseBuffers();

        DepthSpacePoint[] depthPoints = m_frameManager.GetDepthCoordinates();
        if (depthPoints != null)
        {
            m_pDepthComputeBuffer = new ComputeBuffer(m_frameManager.GetColorSpacePointBuffer().Length, sizeof(float) * 2);
            m_pTextureMixMaterial.SetBuffer("DepthCoords", m_pDepthComputeBuffer);
        }
        m_depthManager.OnActivationChanged += (x) => { SetActive(x); };
    }

    void Update()
    {
        ColorSpacePoint[] colorSpacePoints = m_frameManager.GetColorSpacePointBuffer();

        for (int y = 0; y < m_frameManager.DepthFrameHeight; y += m_pColorDownsampleSize)
        {
            for (int x = 0; x < m_frameManager.DepthFrameWidth; x += m_pColorDownsampleSize)
            {
                int idx = x / m_pColorDownsampleSize;
                int idy = y / m_pColorDownsampleSize;

                int fullSampleIndex = y * m_frameManager.DepthFrameWidth + x;
                int downSampleIndex = (idy * (m_frameManager.DepthFrameWidth / m_pColorDownsampleSize)) + idx;

                ColorSpacePoint p = colorSpacePoints[fullSampleIndex];
                Vector2 texcoord = new Vector2();

                texcoord.x = p.X / m_frameManager.ColorFrameWidth;
                texcoord.y = p.Y / m_frameManager.ColorFrameHeight;
                m_pColorToDepth[downSampleIndex] = texcoord;
            }
        }
        m_pDepthComputeBuffer.SetData(m_pColorToDepth);
    }

    void LateUpdate()
    {
        m_pTextureMixMaterial.SetColor("_AmbientColor", AmbientColor);
        m_pTextureMixMaterial.SetFloat("_AmbientColorRate", AmbientColorRate);
        m_pTextureMixMaterial.SetFloat("_AmbientColorMult", AmbientColorMult);
        m_pTextureMixMaterial.SetFloat("_DepthMin", ShaderDepthMin);
        m_pTextureMixMaterial.SetFloat("_DepthMax", ShaderDepthMax);
        m_pTextureMixMaterial.SetFloat("_DepthMinRamp", ShaderDepthMinRamp);
        m_pTextureMixMaterial.SetFloat("_DepthMaxRamp", ShaderDepthMaxRamp);

        Graphics.Blit(null, m_pKinectDepthColorRT, m_pTextureMixMaterial);
    }

    void OnPostRender()
    {
        m_pDepthComputeBuffer.Release();
    }
    
    private void SetActive(bool active)
    {
        if (active)
            StartCoroutine(ReefHelper.FadeNormalized(ReefHelper.FadeType.In, 3f, 
                (x)=> m_pTextureMixMaterial.SetFloat("_Alpha", x),
                null));
        else
            StartCoroutine(ReefHelper.FadeNormalized(ReefHelper.FadeType.Out, 3f, 
                (x) => m_pTextureMixMaterial.SetFloat("_Alpha", x),
                null));
    }

    private void ReleaseBuffers()
    {
        if (m_pDepthComputeBuffer != null) m_pDepthComputeBuffer.Release();
        m_pDepthComputeBuffer = null;
    }

    void OnApplicationQuit()
    {
        ReleaseBuffers();
    }
}
