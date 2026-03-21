using System;
using UnityEngine;

public class Trial : MonoBehaviour
{
    [Header("Actions")]
    public Action OnTrialEnd;

    public virtual void StartTrial() { }
    public virtual void EndTrial() { }
}
