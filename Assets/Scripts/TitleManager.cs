using UnityEngine;
using System.Linq;

public class TitleManager : Show
{
    public int Depth = 3;

    private Camera m_camera;

    public Texture[] m_titleTextures;
    private Texture m_currentTitleTexture; 

    private VideoPlane m_titlePlane;

    void Start()
    {
        m_titleTextures = Resources.LoadAll("Textures/Titles", typeof(Texture)).Cast<Texture>().ToArray();
        m_camera = Application.Instance.MainCamera;

        GameObject ob = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ob.transform.SetParent(m_camera.transform);

        m_titlePlane = ob.AddComponent<VideoPlane>();
        m_titlePlane.Init(Depth);
        m_titlePlane.SetVideoTexture(m_currentTitleTexture);
        m_titlePlane.PlaneMaterial.SetFloat("_Alpha", 0);
    }

    public void SetTitle(string id)
    {
        for (int i = 0; i < m_titleTextures.Length; i++)
        {
            if (m_titleTextures[i].name.Equals(id))
            {
                m_titlePlane.SetVideoTexture(m_titleTextures[i]);
                return;
            }
        }
    }

    public override void Cancel()
    {
        StartCoroutine(ReefHelper.FadeNormalized(ReefHelper.FadeType.Out, 3f,
            (x) => m_titlePlane.PlaneMaterial.SetFloat("_Alpha", x),
            () => base.Cancel()));
    }

    public override void Renew()
    {
        StartCoroutine(ReefHelper.FadeNormalized(ReefHelper.FadeType.In, 3f,
            (x) => m_titlePlane.PlaneMaterial.SetFloat("_Alpha", x),
            null));

        base.Renew();
    }
}
