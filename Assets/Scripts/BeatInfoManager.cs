using Lasp;
using System;
using UnityEngine;

public class BeatInfoManager : MonoBehaviour
{
    AudioLevelTracker m_audioLevelTracker;

    [Range(0, 1)] public float NormalizedAudioPeakThreshold = 0.75f;
    private bool beatToggle = true;
    private float lastPeak;

    public event Action<float> OnNormalizedAudioLevelInput;
    public event Action OnAudioBeat;

    void Start()
    {
        m_audioLevelTracker = GetComponent<AudioLevelTracker>();
    }

    public void NormalizedLevelInput(float level)
    {
        // failsafe
        if (Time.time > lastPeak + 2f)
        {
            m_audioLevelTracker.enabled = false;
            Debug.LogWarning("Default audio device could not be opened. Audio Level Tracker was disabled.");
        }
        if (float.IsNaN(level)) return;

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
