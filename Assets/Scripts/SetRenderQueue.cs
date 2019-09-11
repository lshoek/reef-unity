using UnityEngine;
using UnityEngine.Rendering;

class SetRenderQueue : MonoBehaviour
{
    public RenderQueue Value = RenderQueue.Overlay;

    void Start()
    {
        gameObject.GetComponent<Renderer>().material.renderQueue = (int)Value;
    }
}
