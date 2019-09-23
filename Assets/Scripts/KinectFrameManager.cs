using System.Runtime.InteropServices;
using UnityEngine;
using Windows.Kinect;

public class KinectFrameManager : MonoBehaviour
{
    private MultiSourceFrameReader m_pMultiSourceFrameReader;

    private KinectSensor m_pKinectSensor;
    private CoordinateMapper m_pCoordinateMapper;

    private DepthSpacePoint[] m_pDepthCoordinates;
    private ColorSpacePoint[] m_pColorSpacePoints;

    private byte[] pColorBuffer;
    private ushort[] pDepthBuffer;

    private Texture2D m_pColorRGBX;
    private Texture2D m_pDepth;

    public bool EnableKinect = true;
    public bool EnableDepthTexture = true;

    public int DepthFrameWidth = 512;
    public int DepthFrameHeight = 424;
    public int ColorFrameWidth = 960;
    public int ColorFrameHeight = 540;

    public readonly int BalancedDownsampling = 8;
    public readonly int ColliderMeshDownsampling = 8;

    public int DepthWidthDownsampled { get { return DepthFrameWidth / ColliderMeshDownsampling; } }
    public int DepthHeightDownsampled { get { return DepthFrameHeight / ColliderMeshDownsampling; } }
    public int MaxDepthSamples { get { return DepthWidthDownsampled * DepthHeightDownsampled; } }

    private long frameCount = 0;
    public float UpwardsTranslation = 0;
    public ushort MaxReliableDistance = 4500;


    public ushort[] GetDepthData()
    {
        return pDepthBuffer;
    }

    public Texture2D GetColorTexture()
    {
        return m_pColorRGBX;
    }

    public Texture2D GetDepthTexture()
    {
        return m_pDepth;
    }

    public DepthSpacePoint[] GetDepthCoordinates()
    {
        return m_pDepthCoordinates;
    }

    public ColorSpacePoint[] GetColorSpacePointBuffer()
    {
        return m_pColorSpacePoints;
    }

    void Awake()
    {
        pColorBuffer = new byte[ColorFrameWidth * ColorFrameHeight * 4];
        pDepthBuffer = new ushort[DepthFrameWidth * DepthFrameHeight];

        m_pColorRGBX = new Texture2D(ColorFrameWidth, ColorFrameHeight, TextureFormat.RGBA32, false);
        m_pDepth = new Texture2D(DepthFrameWidth, DepthFrameHeight, TextureFormat.R16, false);

        m_pDepthCoordinates = new DepthSpacePoint[ColorFrameWidth * ColorFrameHeight];
        m_pColorSpacePoints = new ColorSpacePoint[DepthFrameWidth * DepthFrameHeight];

        InitDefaultSensor();
    }

    private void InitDefaultSensor()
    {
        m_pKinectSensor = KinectSensor.GetDefault();

        if (m_pKinectSensor != null)
        {
            m_pKinectSensor.Open();

            m_pCoordinateMapper = m_pKinectSensor.CoordinateMapper;
            MaxReliableDistance = m_pKinectSensor.DepthFrameSource.DepthMaxReliableDistance;

            if (m_pKinectSensor.IsOpen)
            {
                m_pMultiSourceFrameReader = m_pKinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth | FrameSourceTypes.Color);
            }
        }
        if (!EnableKinect)
        {
            m_pColorRGBX = Resources.Load("Textures/Dummies/oceanfloor") as Texture2D;
            m_pDepth = Resources.Load("Textures/Dummies/depthdummy") as Texture2D;

            for (int i = 0; i < pDepthBuffer.Length; i++)
            {
                pDepthBuffer[i] = (ushort)MaxReliableDistance;
            }
        }
    }

    void Update()
    {
        if (m_pMultiSourceFrameReader == null)
        {
            return;
        }
        
        MultiSourceFrame pMultiSourceFrame = m_pMultiSourceFrameReader.AcquireLatestFrame();
        if (pMultiSourceFrame != null)
        {
            frameCount++;
            using (DepthFrame pDepthFrame = pMultiSourceFrame.DepthFrameReference.AcquireFrame())
            {
                using (ColorFrame pColorFrame = pMultiSourceFrame.ColorFrameReference.AcquireFrame())
                {
                    // Get Depth Frame Data
                    if (pDepthFrame != null)
                    {
                        GCHandle pDepthData = GCHandle.Alloc(pDepthBuffer, GCHandleType.Pinned);
                        pDepthFrame.CopyFrameDataToIntPtr(pDepthData.AddrOfPinnedObject(), (uint)pDepthBuffer.Length * sizeof(ushort));
                        pDepthData.Free();
                        pDepthFrame.Dispose();
                    }

                    // Get Color Frame Data
                    if (pColorFrame != null)
                    {
                        GCHandle pColorData = GCHandle.Alloc(pColorBuffer, GCHandleType.Pinned);
                        pColorFrame.CopyConvertedFrameDataToIntPtr(pColorData.AddrOfPinnedObject(), (uint)pColorBuffer.Length, ColorImageFormat.Rgba);
                        pColorData.Free();
                        pColorFrame.Dispose();
                    }
                    ProcessFrame();
                }
            }
        }
    }

    void ProcessFrame()
    {
        GCHandle pDepthData = GCHandle.Alloc(pDepthBuffer, GCHandleType.Pinned);
        GCHandle pDepthCoordinatesData = GCHandle.Alloc(m_pDepthCoordinates, GCHandleType.Pinned);
        GCHandle pColorData = GCHandle.Alloc(m_pColorSpacePoints, GCHandleType.Pinned);

        m_pCoordinateMapper.MapColorFrameToDepthSpaceUsingIntPtr(
          pDepthData.AddrOfPinnedObject(), (uint)pDepthBuffer.Length * sizeof(ushort),
          pDepthCoordinatesData.AddrOfPinnedObject(), (uint)m_pDepthCoordinates.Length);

        m_pCoordinateMapper.MapDepthFrameToColorSpaceUsingIntPtr(
          pDepthData.AddrOfPinnedObject(), pDepthBuffer.Length * sizeof(ushort),
          pColorData.AddrOfPinnedObject(), (uint)m_pColorSpacePoints.Length);

        m_pDepth.LoadRawTextureData(pDepthData.AddrOfPinnedObject(), pDepthBuffer.Length * sizeof(ushort));
        m_pDepth.Apply();

        pColorData.Free();
        pDepthCoordinatesData.Free();
        pDepthData.Free();

        m_pColorRGBX.LoadRawTextureData(pColorBuffer);
        m_pColorRGBX.Apply();
    }

    void OnApplicationQuit()
    {
        pDepthBuffer = null;
        pColorBuffer = null;

        if (m_pDepthCoordinates != null)
        {
            m_pDepthCoordinates = null;
        }
        if (m_pMultiSourceFrameReader != null)
        {
            m_pMultiSourceFrameReader.Dispose();
            m_pMultiSourceFrameReader = null;
        }
        if (m_pKinectSensor != null)
        {
            m_pKinectSensor.Close();
            m_pKinectSensor = null;
        }
    }
}
