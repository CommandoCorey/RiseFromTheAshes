using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleAvoidance : AgentBehaviour
{
    float maxAvoidForce = 10.0f;
    float boxSize = 10;
    float radius = 3;

    public override Steering GetSteering()
    {
        Steering steer = new Steering();
        steer.linearVelocity = CollisionAvoidance();

        return steer;
    }

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

    private Transform FindMostThreateningObstacle(Vector3 ahead1, Vector3 ahead2)
    {
        Transform mostThreatening = null;
        Vector3 position = transform.position;

        var obstacles = Physics.OverlapBox(transform.position, new Vector3(boxSize / 2, 1, boxSize / 2));

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

    private bool LineIntersectsCircle(Vector3 ahead1, Vector3 ahead2, Transform obstacle)
    {
        float ahead1Distance = Vector3.Distance(obstacle.position, ahead1);
        float ahead2Distance = Vector3.Distance(obstacle.position, ahead2);        

        return (ahead1Distance <= radius || ahead2Distance <= radius);
    }
}