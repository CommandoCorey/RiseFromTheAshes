using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State : MonoBehaviour
{
    protected Vector3 target; 
    protected Vector3 formationTarget;

    public Vector3 Target { get => target; set => target = value; }
    public Vector3 FormationTarget { set => formationTarget = value; }

    //protected abstract void Init();

    //protected abstract void Exit();
}
