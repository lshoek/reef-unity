using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scheduler : MonoBehaviour
{
    public float InitialDelay = 1f;
    public float timeBetweenActions = 10f;

    private CreatureManager creatureManager;
    private BoidManager boidManager;
    private DepthMeshManager depthMeshManager;

    private List<Show> availableShows;
    private Show currentShow;

    private Coroutine activeRoutine;

    private bool active = true;

    void Start()
    {
        availableShows = new List<Show>();
        creatureManager = FindObjectOfType<CreatureManager>();
        boidManager = FindObjectOfType<BoidManager>();

        StartCoroutine(SchedulerRoutine());
    }

    private IEnumerator SchedulerRoutine()
    {
        yield return new WaitForSeconds(InitialDelay);

        if (creatureManager != null) availableShows.Add(creatureManager);
        if (boidManager != null) availableShows.Add(boidManager);

        while (active)
        {
            currentShow = availableShows[Random.Range(0, availableShows.Count)];

            currentShow.Renew();
            yield return new WaitForSeconds(timeBetweenActions);

            currentShow.Cancel();
            yield return new WaitUntil(() => !currentShow.Active);

            yield return new WaitForSeconds(timeBetweenActions);
        }
    }
}
