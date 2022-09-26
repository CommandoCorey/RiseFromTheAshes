using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleAvoidanceBehaviour : AgentBehaviour
{
    float maxAvoidForce = 10.0f;
    float radius = 3;

    Collider[] obstacles;

    public override Steering GetSteering()
    {       
        Steering steer = new Steering();
        steer.linearVelocity = CollisionAvoidance();

        return steer;
    }

    // 
    private Vector3 CollisionAvoidance()
    {
        // calculate the ahead and ahead2 vectors
        Vector3 ahead = transform.position + agent.Velocity.normalized * behaviours.AheadDistance;
        Vector3 ahead2 = transform.position + agent.Velocity.normalized * behaviours.AheadDistance * 0.5f;

        Transform mostThreatening = FindMostThreateningObstacle(ahead, ahead2);
        Vector3 avoidance = Vector3.zero;

        if (mostThreatening != null)
        {
            avoidance.x = ahead.x - mostThreatening.position.x;
            avoidance.z = ahead.z - mostThreatening.position.z;

            avoidance.Normalize();
            avoidance *= maxAvoidForce;
        }
        else
        {
            avoidance *= 0;  // nullify the avoidance force
        }

        return avoidance;
    }

    // 
    private Transform FindMostThreateningObstacle(Vector3 ahead1, Vector3 ahead2)
    {
        Transform mostThreatening = null;
        Vector3 position = transform.position;

        obstacles = Physics.OverlapBox(transform.position + transform.TransformDirection(behaviours.BoxOffset),
                                           behaviours.BoxSize / 2, transform.rotation);

        for (int i = 0; i < obstacles.Length; i++)
        {
            bool collision = LineIntersectsCircle(ahead1, ahead2, obstacles[i].transform);

            // "position" is the character's current position
            if (collision && (mostThreatening == null ||
                Vector3.Distance(transform.position, obstacles[i].transform.position) <
                Vector3.Distance(transform.position, mostThreatening.position)))
            {
                mostThreatening = obstacles[i].transform;
            }
        }

        return mostThreatening;
    }

    // 
    private bool LineIntersectsCircle(Vector3 ahead1, Vector3 ahead2, Transform obstacle)
    {
        float ahead1Distance = Vector3.Distance(obstacle.position, ahead1);
        float ahead2Distance = Vector3.Distance(obstacle.position, ahead2);

        if (ahead1Distance <= radius)
            Debug.DrawLine(obstacle.position, ahead1, Color.blue, 0.2f);

        if(ahead2Distance < radius)
            Debug.DrawLine(obstacle.position, ahead2, Color.blue, 0.2f);

        return (ahead1Distance <= radius || ahead2Distance <= radius);
    }

    private void OnDrawGizmos()
    {
        if (obstacles == null)
            return;

        Gizmos.color = Color.red;
        foreach(var obstacle in obstacles)
        {
            Gizmos.DrawWireSphere(obstacle.transform.position, radius);
        }
    }

}