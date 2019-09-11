using System.Runtime.InteropServices;
using UnityEngine;
using Windows.Kinect;

public class KinectFrameManager : MonoBehaviour
{
    private ColorFrameReader m_pColorFrameReader;
    private DepthFrameReader m_pDepthFrameReader;

    private KinectSensor m_pKinectSensor;
    private CoordinateMapper m_pCoordinateMapper;

    private DepthSpacePoint[] m_pDepthCoordinates;
    private ColorSpacePoint[] m_pColorSpacePoints;

    private byte[] pColorBuffer;
    private ushort[] pDepthBuffer;

    private Texture2D m_pColorRGBX;

    public int DepthFrameWidth = 512;
    public int DepthFrameHeight = 424;
    public int ColorFrameWidth = 1920;
    public int ColorFrameHeight = 1080;

    private long frameCount = 0;
    public float UpwardsTranslation = 0.0f;

    public ushort[] GetDepthData()
    {
        return pDepthBuffer;
    }

    public Texture2D GetColorTexture()
    {
        return m_pColorRGBX;
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

        m_pDepthCoordinates = new DepthSpacePoint[ColorFrameWidth * ColorFrameHeight];
        m_pColorSpacePoints = new ColorSpacePoint[DepthFrameWidth * DepthFrameHeight];

        InitDefaultSensor();

        if (m_pKinectSensor != null)
        {
            m_pColorSpacePoints = new ColorSpacePoint[m_pKinectSensor.DepthFrameSource.FrameDescription.LengthInPixels];
        }
    }

    private void InitDefaultSensor()
    {
        m_pKinectSensor = KinectSensor.GetDefault();

        if (m_pKinectSensor != null)
        {
            m_pKinectSensor.Open();
            m_pCoordinateMapper = m_pKinectSensor.CoordinateMapper;

            if (m_pKinectSensor.IsOpen)
            {
                m_pColorFrameReader = m_pKinectSensor.ColorFrameSource.OpenReader();
                m_pDepthFrameReader = m_pKinectSensor.DepthFrameSource.OpenReader();
            }
        }
        if (m_pKinectSensor == null)
        {
            Debug.LogError("No ready Kinect found!");
        }
    }

    void Update()
    {
        if (m_pDepthFrameReader == null | m_pColorFrameReader == null)
        {
            return;
        }

        frameCount++;
        using (DepthFrame pDepthFrame = m_pDepthFrameReader.AcquireLatestFrame())
        {
            using (ColorFrame pColorFrame = m_pColorFrameReader.AcquireLatestFrame())
            {
                // Get Depth Frame Data.
                if (pDepthFrame != null)
                {
                    GCHandle pDepthData = GCHandle.Alloc(pDepthBuffer, GCHandleType.Pinned);
                    pDepthFrame.CopyFrameDataToIntPtr(pDepthData.AddrOfPinnedObject(), (uint)pDepthBuffer.Length * sizeof(ushort));
                    pDepthData.Free();
                }

                // Get Color Frame Data
                if (pColorFrame != null)
                {
                    FrameDescription fd = pColorFrame.FrameDescription;
                    Debug.Log($"{fd.Width}x{fd.Height} {fd.LengthInPixels} {fd.BytesPerPixel}");

                    GCHandle pColorData = GCHandle.Alloc(pColorBuffer, GCHandleType.Pinned);
                    pColorFrame.CopyConvertedFrameDataToIntPtr(pColorData.AddrOfPinnedObject(), (uint)pColorBuffer.Length, ColorImageFormat.Rgba);
                    pColorData.Free();
                }
            }
        }
        //ProcessFrame();
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
          pColorData.AddrOfPinnedObject(),(uint)m_pColorSpacePoints.Length);

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
        if (m_pDepthFrameReader != null)
        {
            m_pDepthFrameReader.Dispose();
            m_pDepthFrameReader = null;
        }
        if (m_pColorFrameReader != null)
        {
            m_pColorFrameReader.Dispose();
            m_pColorFrameReader = null;
        }
        if (m_pKinectSensor != null)
        {
            m_pKinectSensor.Close();
            m_pKinectSensor = null;
        }
    }
}
