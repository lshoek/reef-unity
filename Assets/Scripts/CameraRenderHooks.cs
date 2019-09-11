using System;
using UnityEngine;

public class CameraRenderHooks : MonoBehaviour
{
    public event Action OnPreRenderEvent;
    public event Action OnPostRenderEvent;

    void OnPreRender()
    {
        if (OnPreRenderEvent != null)
            OnPreRenderEvent();
    }

    void OnPostRender()
    {
        if (OnPostRenderEvent != null)
            OnPostRenderEvent();
    }
}
