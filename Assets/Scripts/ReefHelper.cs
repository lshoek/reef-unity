using UnityEngine;

public static class ReefHelper
{
    public static float DisplayWidth;
    public static float DisplayHeight;

    private static Camera m_camera;

    static ReefHelper()
    {
        m_camera = Application.Instance.MainCamera;

        DisplayHeight = 2f * m_camera.orthographicSize;
        DisplayWidth = DisplayHeight * m_camera.aspect;
    }
}