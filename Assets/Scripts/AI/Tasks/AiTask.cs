using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;


public abstract class AiTask : ScriptableObject, ICloneable
{
    public float timeDelay;    

    //protected AiPlayer ai;
    //protected SimpleAiPlayer simpleAi;

    protected string taskStatus;

    public bool Completed { get; set; }

    public abstract string TaskDescription { get; }
    public abstract string ActiveTaskDescription { get; }
    public string TaskStatus { get => taskStatus; set => taskStatus = value; }
    
    // abstract functions
    public abstract int GetSteelCost();
    public abstract bool PerformTask();
    public abstract bool IsComplete();

    public abstract object Clone();

}