using System.Collections;
using UnityEngine;

public abstract class Show : MonoBehaviour
{
    public virtual float Duration { get; set; }

    public virtual bool EndOfSequence { get; set; } = false;
    public virtual bool Active { get; private set; } = false;

    public ReefHelper.AquaticLayerMode CurrentAquaticLayerMode { get; set; }
    public Coroutine CurrentRoutine;

    public virtual void Cancel()
    {
        Active = false;
        EndOfSequence = false;
    }

    public virtual void Renew()
    {
        Active = true;
        CurrentRoutine = StartCoroutine(EndSequenceAfterDuration(Duration));
    }

    private IEnumerator EndSequenceAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        EndOfSequence = true;
    }
}
