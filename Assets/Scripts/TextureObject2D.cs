using UnityEngine;

public class TextureObject2D : MonoBehaviour
{
    public Renderer Renderer { get; private set; }

    void Awake()
    {
        Renderer = GetComponentInChildren<Renderer>();
    }

    void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0, Time.time * 32);
    }
}
