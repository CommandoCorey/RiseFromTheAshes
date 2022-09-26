using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidBehaviour : FleeBehaviour
{
    RaycastHit hit;// fowardHit, leftHit, rightHit;

    public override Steering GetSteering()
    {
        Steering steer = base.GetSteering();

        if (ObstacleAhead(out hit))// || ObstacleOnLeft(out hit) || ObstacleOnRight(out hit))
        {
            target = hit.point;
            steer = base.GetSteering();            
        }
        else
        {
            steer = new Steering();            
        }

        return steer;
    }

    // Check if there is an obstacle within range ahead of the agent
    private bool ObstacleAhead(out RaycastHit hit)
    {
        if(Physics.Raycast(behaviours.FrontPoint, transform.forward, out hit, behaviours.AheadDistance,
            behaviours.ObstacleLayers))         
        {
            behaviours.FrontObstacle = true;
            return true;
        }

        behaviours.FrontObstacle = false;
        return false;
    }

    private bool ObstacleOnLeft(out RaycastHit hit)
    {
        Vector3 upLeft = (transform.forward - transform.right).normalized;

        if (Physics.Raycast(behaviours.FrontPoint, upLeft, out hit, behaviours.AheadDistance,
            behaviours.ObstacleLayers))
        {
            behaviours.UpLeftObstacle = true;
            return true;
        }

        behaviours.UpLeftObstacle = false;
        return false;
    }

    private bool ObstacleOnRight(out RaycastHit hit)
    {
        Vector3 upRight = (transform.forward + transform.right).normalized;

        if (Physics.Raycast(behaviours.FrontPoint, upRight, out hit, behaviours.AheadDistance,
            behaviours.ObstacleLayers))
        {
            behaviours.UpRightObstacle = true;
            return true;
        }

        behaviours.UpRightObstacle = false;
        return false;
    }

    private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return ((layerMask.value & (1 << obj.layer)) > 0);
    }

}