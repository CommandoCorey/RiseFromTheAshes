using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State : MonoBehaviour
{
    protected Vector3 target; 

    public Vector3 Target { get => target; set => target = value; }

    //protected abstract void Init();

    //protected abstract void Exit();
}
