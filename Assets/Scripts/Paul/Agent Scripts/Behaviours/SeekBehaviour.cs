using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekBehaviour : AgentBehaviour
{
    private void Awake()
    {
        agent = GetComponent<AgentMovement>();
    }

    /// <summary>
    /// Moves agent towards a target
    /// </summary>
    /// <returns>The seek steering behaviour</returns>
    public override Steering GetSteering()
    {
        Steering steer = new Steering();
        steer.linearVelocity = target - transform.position;
        steer.linearVelocity.Normalize();
        steer.linearVelocity = steer.linearVelocity * agent.Acceleration;

        Vector3 direction = steer.linearVelocity;
        Vector3 rotation = Vector3.RotateTowards(transform.forward, direction, agent.MaxRotation, 0);
        steer.angularVelocity = -rotation.y;

        return steer;
    }

}
