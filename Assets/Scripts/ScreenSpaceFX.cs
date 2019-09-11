using UnityEngine;

public class ScreenSpaceFX : MonoBehaviour
{
    Material ScreenSpaceFXMaterial;

    void Start()
    {
        ScreenSpaceFXMaterial = new Material(Resources.Load("Shaders/WaterDisplacement") as Shader);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        Graphics.Blit(src, dst, ScreenSpaceFXMaterial);
    }
}
