using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleeBehaviour : AgentBehaviour
{
    public override Steering GetSteering()
    {
        Steering steer = new Steering();
        steer.linearVelocity = transform.position - target;
        steer.linearVelocity.Normalize();
        steer.linearVelocity = steer.linearVelocity * agent.Acceleration;

        return steer;
    }


}