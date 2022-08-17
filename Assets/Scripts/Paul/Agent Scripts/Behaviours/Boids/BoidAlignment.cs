using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidAlignment : AgentBehaviour
{
    [SerializeField] float neighbourDistance = 15.0f;
    public List<GameObject> targets;

    private Vector3 averageVelocity;

    private void Start()
    {
        neighbourDistance = GetComponent<BehaviourManager>().CohesionDistance;
    }

    public override Steering GetSteering()
    {
        Steering steer = new Steering();
        averageVelocity = Vector3.zero;

        int count = 0;

        // add all velocities together
        foreach (GameObject other in targets)
        {
            // look at all of the neighbours and find the distance from them
            float distance = (transform.position - other.transform.position).magnitude;

            if ((distance > 0) && (distance < neighbourDistance))
            {
                averageVelocity += other.GetComponent<Agent>().Vecloity;
                count++;
            }
        }

        if (count > 0)
        {
            // divide total velocity by target count to get average velocity
            averageVelocity /= targets.Count;

            steer.linearVelocity = averageVelocity;
        }

        return steer;
    }

}
