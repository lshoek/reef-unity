using Lasp;
using System;
using System.Collections;
using UnityEngine;

public class BeatInfoManager : MonoBehaviour
{
    AudioLevelTracker m_audioLevelTracker;

    [Range(0, 1)] public float NormalizedAudioPeakThreshold = 0.75f;
    [SerializeField] private float beatTimeout = 120f;
    private bool beatToggle = true;
    private float lastPeak;

    public bool IsActive { get; private set; } = false;

    public event Action<float> OnNormalizedAudioLevelInput;
    public event Action OnAudioBeat;

    void Start()
    {
        m_audioLevelTracker = GetComponent<AudioLevelTracker>();
        EnableAudioInterface(false);
    }

    public void NormalizedLevelInput(float level)
    {
        // failsafe
        if (!IsActive) return;
        if (Time.time > lastPeak + beatTimeout)
        {
            SetActive(false);
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

    public void SetActive(bool active)
    {
        IsActive = active;
        m_audioLevelTracker.enabled = active;
        lastPeak = Time.time;

        EnableAudioInterface(active);

        StartCoroutine(WaitForInputDevice());
    }

    private IEnumerator WaitForInputDevice()
    {
        yield return new WaitForSeconds(1f);
        MasterInput.Terminate();
        MasterInput.Initialize();
    }

    private void EnableAudioInterface(bool enable)
    {
        // input device
        // Kinect + MOTU = latency city, my workaround is to disable the the audio interface whenever the Kinect is activated.
        // Kinect v2 Microphone Array fails, which actually seems to be convenient when trying to terminate LASP.

        string deviceName = enable ? "Line In 1-2" : "Kinect v2 Microphone Array";
        System.Diagnostics.Process.Start("C:/nircmd-x64/nircmd.exe", $"setdefaultsounddevice \"{deviceName}\"");
    }
}
