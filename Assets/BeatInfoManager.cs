using System;
using UnityEngine;

public class BeatInfoManager : MonoBehaviour
{
    [Range(0, 1)] public float NormalizedAudioPeakThreshold = 0.75f;
    private bool beatToggle = true;
    private float lastPeak;

    public event Action<float> OnNormalizedAudioLevelInput;
    public event Action OnAudioBeat;

    public void NormalizedLevelInput(float level)
    {
        // max 200bpm
        if (Time.time > lastPeak + 60 / 200f)
        {
            if (level > NormalizedAudioPeakThreshold)
            {
                if (beatToggle) OnAudioBeat?.Invoke();
                beatToggle = !beatToggle;

                lastPeak = Time.time;
            }
        }
        OnNormalizedAudioLevelInput?.Invoke(level);
    }
}
