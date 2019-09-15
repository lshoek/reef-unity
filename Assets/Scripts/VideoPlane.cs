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

        transform.localScale = new Vector3(ReefHelper.DisplayWidth / m_camera.orthographicSize, 1.0f, ReefHelper.DisplayHeight / m_camera.orthographicSize);

        GetComponent<Renderer>().material = Resources.Load("Materials/VideoPlaneMaterial") as Material;
        PlaneMaterial = GetComponent<Renderer>().material;
    }

    public void SetWidth(float width)
    {
        Vector3 newScale = transform.localScale;
        newScale.x *= width/ReefHelper.DisplayWidth;
        transform.localScale = newScale;
    }

    public void SetVideoTexture(Texture tex)
    {
        PlaneMaterial.mainTexture = tex;
    }

    void Update()
    {
        transform.localPosition = new Vector3(0, 0, m_camera.farClipPlane - depth);
    }
}
