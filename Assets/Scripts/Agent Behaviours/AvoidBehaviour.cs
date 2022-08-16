using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidBehaviour : AgentBehaviour
{
    RaycastHit hit;

    Vector3 agentFront;

    public override Steering GetSteering()
    {
        Steering steer = new Steering();

        


        return steer;
    }

    /*
    private bool ObstacleAhead()
    {
        if(Physics.Raycast(agentFront, transform.forward, ))

    }*/

}
