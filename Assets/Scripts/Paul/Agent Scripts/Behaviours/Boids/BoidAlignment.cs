using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidAlignment : AgentBehaviour
{
    [SerializeField] float neighbourDistance = 15.0f;
    public List<GameObject> targets;

    private Vector3 averageVelocity;

    protected override void Awake()
    {
        base.Awake();
        neighbourDistance = GetComponent<BehaviourManager>().CohesionDistance;
    }

    public override Steering GetSteering()
    {
        Steering steer = new Steering();
        averageVelocity = Vector3.zero;

        int count = 0;

        targets = agent.Neighbours;
        targets.Add(this.gameObject);

        // add all velocities together
        foreach (GameObject other in targets)
        {
            // look at all of the neighbours and find the distance from them
            float distance = (transform.position - other.transform.position).magnitude;

            if ((distance > 0) && (distance < neighbourDistance))
            {
                averageVelocity += other.GetComponent<AgentMovement>().Vecloity;
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

    private void OnDrawGizmos()
    {
        /*
        foreach (GameObject neighbour in targets)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, neighbour.transform.position);
        }*/
    }

}
