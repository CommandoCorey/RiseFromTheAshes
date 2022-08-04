using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Steering
{
    public float angularVelocity; // rotation 0->360
    public Vector3 linearVelocity; //instantaneous velocity

    public Steering(float angularVelocity = 0.0f, Vector3 linearVelocity = new Vector3())
    {
        this.angularVelocity = angularVelocity;
        this.linearVelocity = linearVelocity;
    }

}
