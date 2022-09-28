using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public abstract class AiTask : ScriptableObject
{
    public float timeDelay;

    protected AiPlayer ai;

    public abstract int GetSteelCost();
    public abstract bool PerformTask();
}