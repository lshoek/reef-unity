using System.Linq;
using UnityEngine;
using UnityEngine.Video;

public class CreatureDataAccessor : MonoBehaviour
{
    public VideoClip[] Clips { get => _clips; }
    [SerializeField] private VideoClip[] _clips;

    public Texture[] Particles { get; private set; }

    void Awake()
    {
        Particles = Resources.LoadAll("Textures/Particles", typeof(Texture)).Cast<Texture>().ToArray();
    }

    public Texture GetRandomParticle()
    {
        return Particles[Random.Range(0, Particles.Length)];
    }

    public VideoClip GetRandomClip()
    {
        return Clips[Random.Range(0, Clips.Length)];
    }
}
