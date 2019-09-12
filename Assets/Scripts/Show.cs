using System;
using UnityEngine;

public abstract class Show : MonoBehaviour
{
    public virtual bool Active { get; private set; } = false;

    public virtual void Cancel() { Active = false; }
    public virtual void Renew() { Active = true; }
}
