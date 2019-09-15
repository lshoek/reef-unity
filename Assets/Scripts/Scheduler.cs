using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scheduler : MonoBehaviour
{
    public float InitialDelay = 1f;
    public float ShowDuration = 30f;

    public float TimeBetweenShows = 10f;
    private float cachedTimeBetweenShows;

    private CreatureManager creatureManager;
    private BoidManager boidManager;
    private TitleManager titleManager;

    private List<Show> availableShows;
    private Show currentShow;
    private Show queuedShow;

    private Coroutine activeRoutine;

    public bool Active = true;
    public bool ActionQueued { get; private set; } = false;
    public bool WaitForAction { get; set; } = false;

    void Start()
    {
        availableShows = new List<Show>();

        creatureManager = Application.Instance.CreatureManager;
        boidManager = Application.Instance.BoidManager;
        titleManager = Application.Instance.TitleManager;

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

        if (creatureManager != null) availableShows.Add(creatureManager);
        if (boidManager != null) availableShows.Add(boidManager);
        if (titleManager != null) availableShows.Add(titleManager);

        foreach (Show show in availableShows)
            show.Duration = ShowDuration;

        while (Active)
        {
            currentShow = ActionQueued ? queuedShow : availableShows[Random.Range(0, availableShows.Count-1)];
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
}
