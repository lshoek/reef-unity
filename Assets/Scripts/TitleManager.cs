using UnityEngine;
using System.Linq;

public class TitleManager : Show
{
    public int Depth = 3;

    private Camera m_camera;
    private BeatInfoManager m_beatInfoManager;

    public Texture[] m_titleTextures;
    private VideoPlane m_titlePlane;

    public float LevelDamping = 0.03125f;
    private Vector3 cachedScale;

    public bool TitleQueued { get; private set; } = false;

    void Start()
    {
        m_camera = Application.Instance.MainCamera;
        m_beatInfoManager = Application.Instance.BeatInfoManager;
        m_beatInfoManager.OnNormalizedAudioLevelInputLP += (x) => NormalizedAudioLevelInput(x);

        GameObject ob = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ob.transform.SetParent(m_camera.transform);

        m_titlePlane = ob.AddComponent<VideoPlane>();
        m_titlePlane.Init(Depth);
        m_titlePlane.PlaneMaterial.SetFloat("_Alpha", 0);
        cachedScale = m_titlePlane.transform.localScale;
    }

    public void QueueTitle(string id)
    {
        for (int i = 0; i < m_titleTextures.Length; i++)
        {
            if (m_titleTextures[i].name.Equals(id))
            {
                m_titlePlane.SetVideoTexture(m_titleTextures[i]);
                TitleQueued = true;
                return;
            }
        }
    }

    private void NormalizedAudioLevelInput(float level)
    {
        m_titlePlane.transform.localScale = cachedScale * (1f + level * LevelDamping); //new Vector3(level * LevelDamping, 1f, level * LevelDamping);
    }

    public override void Cancel()
    {
        StartCoroutine(ReefHelper.FadeNormalized(ReefHelper.FadeType.Out, 3f,
            (x) => m_titlePlane.PlaneMaterial.SetFloat("_Alpha", x),
            () => {
                base.Cancel();
                TitleQueued = false;
            }
        ));
        // do cancel until after fade is finished
    }

    public override void Renew()
    {
        m_beatInfoManager.SetActive(true);
        if (!TitleQueued)
        {
            m_titlePlane.SetVideoTexture(m_titleTextures[0]);
        }
        StartCoroutine(ReefHelper.FadeNormalized(ReefHelper.FadeType.In, 3f,
            (x) => m_titlePlane.PlaneMaterial.SetFloat("_Alpha", x),
            null));

        // call renew during fade
        base.Renew();
    }
}
