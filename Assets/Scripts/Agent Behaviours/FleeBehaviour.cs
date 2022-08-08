using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleeBehaviour : AgentBehaviour
{
    public override Steering GetSteering()
    {
        Steering steer = new Steering();
        steer.linearVelocity = transform.position - target.transform.position;
        steer.linearVelocity.Normalize();
        steer.linearVelocity = steer.linearVelocity * agent.MaxAccel;

        return steer;
    }


}
