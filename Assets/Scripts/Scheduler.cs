using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scheduler : MonoBehaviour
{
    public float InitialDelay = 1f;
    public float ShowDuration = 30f;

    public float TimeBetweenShows = 10f;
    private float cachedTimeBetweenShows;

    public float[] WeightSettings;
    private float[] showWeights;

    private StaticManager staticManager;
    private CreatureManager creatureManager;
    private BoidManager boidManager;
    private TitleManager titleManager;

    private List<Show> availableShows;
    [SerializeField] private Show currentShow;
    private Show queuedShow;

    private Coroutine activeRoutine;

    public bool Active = true;
    public bool ActionQueued { get; private set; } = false;
    public bool WaitForAction { get; set; } = false;

    [SerializeField] private ReefHelper.AquaticLayerMode m_currentAquaticLayerMode;
    public ReefHelper.AquaticLayerMode CurrentAquaticLayerMode
    {
        get => m_currentAquaticLayerMode;
        set
        {
            m_currentAquaticLayerMode = value;
            foreach (Show show in availableShows)
                show.CurrentAquaticLayerMode = value;
        }
    }

    void Start()
    {
        availableShows = new List<Show>();

        staticManager = Application.Instance.StaticManager;
        creatureManager = Application.Instance.CreatureManager;
        boidManager = Application.Instance.BoidManager;
        titleManager = Application.Instance.TitleManager;

        CurrentAquaticLayerMode = 0;
        cachedTimeBetweenShows = TimeBetweenShows;
        StartCoroutine(SchedulerRoutine());
    }

    /// <summary>
    /// Only support queing titles for now
    /// </summary>
    public void QueueAction(string args, bool endCurrentShow)
    {
        // temporary solution against abuse
        if (currentShow == titleManager) return;

        ActionQueued = true;
        titleManager.SetTitle(args);
        queuedShow = titleManager;

        if (endCurrentShow)
        {
            TimeBetweenShows = 0f;
            currentShow.StopCoroutine(currentShow.CurrentRoutine);
            if (currentShow.Active && !currentShow.EndOfSequence)
                currentShow.EndOfSequence = true;
        }
    }

    private IEnumerator SchedulerRoutine()
    {
        yield return new WaitForSeconds(InitialDelay);

        if (staticManager != null) availableShows.Add(staticManager);
        if (creatureManager != null) availableShows.Add(creatureManager);
        if (boidManager != null) availableShows.Add(boidManager);
        if (titleManager != null) availableShows.Add(titleManager);

        foreach (Show show in availableShows)
            show.Duration = ShowDuration;

        showWeights = new float[availableShows.Count-1];
        int numWeightSettings = WeightSettings.Length;
        for (int i = 0; i < showWeights.Length; i++)
        {
            showWeights[i] = (i < numWeightSettings) ? WeightSettings[i] : 0;
        }

        while (Active)
        {
            currentShow = ActionQueued ? queuedShow : availableShows[GetRandomWeightedIndex(showWeights)];
            ActionQueued = false;
            TimeBetweenShows = cachedTimeBetweenShows;

            currentShow.Renew();
            yield return new WaitUntil(() => currentShow.EndOfSequence);

            currentShow.Cancel();
            yield return new WaitUntil(() => !currentShow.Active);

            yield return new WaitForSeconds(TimeBetweenShows);
        }
        availableShows.Clear();
    }

    private int GetRandomWeightedIndex(float[] weights)
    {
        float total = 0;
        for (int i = 0; i < weights.Length; i++)
            total += weights[i];

        float r = Random.value;
        float s = 0f;
        for (int i = 0; i < weights.Length; i++)
        {
            s += weights[i] / total;
            if (s >= r) return i;
        }
        return 0;
    }
}
