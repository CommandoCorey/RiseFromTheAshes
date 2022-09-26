using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidCohesion : AgentBehaviour
{
    [SerializeField] float neighbourDistance = 15.0f;
    public List<GameObject> targets;
    private int count = 0;

    protected override void Awake()
    {
        base.Awake();
        neighbourDistance = GetComponent<BehaviourManager>().CohesionDistance;
    }
    public override Steering GetSteering()
    {
        Steering steer = new Steering();
        count = 0;

        foreach(GameObject other in agent.Neighbours)
        {
            if(other != null && other != this.gameObject)
            {
                // look at all of the neighbours and find the aggregate center of them
                float distance = (transform.position - other.transform.position).magnitude;

                if((distance > 0) && (distance < neighbourDistance))
                {
                    steer.linearVelocity += other.transform.position;
                    count++;
                }
            }
        }


        if(count > 0) // atleast one neighbout was found
        {
            // makes the velocity vector relative based on the number of neighbours
            steer.linearVelocity /= count;
            steer.linearVelocity -= transform.position;
        }

        return steer;
    }   

}
