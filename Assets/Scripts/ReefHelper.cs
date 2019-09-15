using System;
using System.Collections;
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

    public enum FadeType { In, Out };

    public static IEnumerator FadeNormalized(FadeType fadeType, float duration, Action<float> callback, Action finished)
    {
        float time = 0;
        if (fadeType == FadeType.In)
        {
            while (time <= duration)
            {
                time += Time.deltaTime;
                callback(Mathf.Clamp((time / duration), 0, 1f));
                yield return null;
            }
            callback(1f);
        }
        else if (fadeType == FadeType.Out)
        {
            while (time <= duration)
            {
                time += Time.deltaTime;
                callback(Mathf.Clamp(1.0f - (time / duration), 0, 1f));
                yield return null;
            }
            callback(0);
        }
        finished?.Invoke();
    }
}