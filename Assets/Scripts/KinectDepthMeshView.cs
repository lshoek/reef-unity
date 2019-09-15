using UnityEngine;

public class KinectDepthMeshView : MonoBehaviour
{
    KinectDepthManager m_depthManager;
    KinectFrameManager m_frameManager;

    DepthMesh m_depthMesh;
    MeshCollider m_meshCollider;

    public bool IsActive { get; private set; } = false;

    void Start()
    {
        m_depthManager = Application.Instance.KinectDepthManager;
        m_frameManager = Application.Instance.KinectFrameManager;

        m_meshCollider = GetComponent<MeshCollider>();

        float depthAspect = m_frameManager.DepthWidthDownsampled / m_frameManager.DepthHeightDownsampled;
        transform.parent.localPosition = new Vector3(-ReefHelper.DisplayHeight * depthAspect / 2, ReefHelper.DisplayHeight / 2);

        m_depthManager.OnActivationChanged += (x) => { SetActive(x); };
    }

    void Update()
    {
        if (IsActive)
        {
            m_meshCollider.sharedMesh = m_depthMesh.mesh;
        }
    }

    private void SetActive(bool active)
    {
        IsActive = active;

        if (active)
            m_depthMesh = m_depthManager.GetDepthMesh();
        else
            m_meshCollider.sharedMesh = null;
    }
}
