using UnityEngine;
using UnityEngine.Video;

public class CreatureClips : MonoBehaviour
{
    public VideoClip[] Clips { get => _clips; }
    [SerializeField] private VideoClip[] _clips;

    public VideoClip GetRandom()
    {
        return Clips[Random.Range(0, Clips.Length)];
    }
}
