using Lasp;
using System;
using System.Collections;
using UnityEngine;

public class BeatInfoManager : MonoBehaviour
{
    AudioLevelTracker[] m_audioLevelTrackers;

    public bool UseAudioInterface = true;

    [Range(0, 1)] public float NormalizedAudioPeakThreshold = 0.75f;
    [SerializeField] private float beatTimeout = 120f;
    private bool beatToggle = true;
    private float lastPeak;

    public bool IsActive { get; private set; } = false;

    public event Action<float> OnNormalizedAudioLevelInputLP;
    public event Action<float> OnNormalizedAudioLevelInputHP;
    public event Action OnAudioBeat;

    void Start()
    {
        m_audioLevelTrackers = GetComponents<AudioLevelTracker>();
        EnableAudioInterface(false);
    }

    public void NormalizedLevelInputLP(float level)
    {
        // failsafe
        if (!IsActive) return;
        if (Time.time > lastPeak + beatTimeout)
        {
            SetActive(false);
            Debug.LogWarning("Default audio device could not be opened. Audio Level Tracker was disabled.");
        }
        if (float.IsNaN(level)) return;
        level = Mathf.Clamp01(level);

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
        OnNormalizedAudioLevelInputLP?.Invoke(level);
    }

    public void NormalizedLevelInputHP(float level)
    {
        if (!IsActive) return;
        if (float.IsNaN(level)) return;

        level = Mathf.Clamp01(level);
        OnNormalizedAudioLevelInputHP?.Invoke(level);
    }

    public void SetActive(bool active)
    {
        IsActive = active;
        lastPeak = Time.time;

        foreach (var alt in m_audioLevelTrackers)
            alt.enabled = active;

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

        string iface = UseAudioInterface ? "Line In 1-2" : "Stereo Mix";
        string deviceName = enable ? iface : "Kinect v2 Microphone Array";
        System.Diagnostics.Process.Start("C:/nircmd-x64/nircmd.exe", $"setdefaultsounddevice \"{deviceName}\"");
    }
}
