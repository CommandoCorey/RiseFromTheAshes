using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FlockState : State
{
    UnitManager unitManager;
    new UnitController unit;
    AgentMovement agent;
    Steering steer;
    BehaviourManager behaviours;
    
    [SerializeField]
    float distanceFromTarget;

    // Behaviours
    SeekBehaviour seekAccel;
    SeekDecelerateBehaviour seekDecel;
    BoidCohesion cohesion;
    BoidAlignment alignment;
    BoidSeparation separation;
    ObstacleAvoidanceBehaviour avoid;
    
    private float decelerationDistance;

    private List<GameObject> neighbours;
    private Transform leader;    

    // pathfinding
    private Vector3[] path;
    private Vector3 waypoint;
    private int waypointNum;
    private float distanceFromWaypoint;

    // Start is called before the first frame update
    void Start()
    {
        unit = GetComponent<UnitController>();
        agent = GetComponent<AgentMovement>();
        behaviours = GetComponent<BehaviourManager>();

        unitManager = GameObject.FindObjectOfType<UnitManager>();

        neighbours = unitManager.GetNeighbourUnits(this.gameObject, agent.SquadNum);

        SetPath(target); // calcuates path to poisition

        // add seek acceleration
        seekAccel = gameObject.AddComponent<SeekBehaviour>();
        seekAccel.target = target;
        seekAccel.weight = behaviours.SeekWeight;
        seekAccel.enabled = true;

        steer = seekAccel.GetSteering();
        agent.AddSteering(steer, seekAccel.weight);

        // add seek deceleration
        seekDecel = gameObject.AddComponent<SeekDecelerateBehaviour>();
        seekDecel.target = target;
        seekDecel.weight = behaviours.SeekWeight;
        seekDecel.enabled = false;

        // Add the boid cohesion behaviour
        cohesion = gameObject.AddComponent<BoidCohesion>();
        cohesion.targets = neighbours;
        cohesion.weight = behaviours.CohesionWeight;
        cohesion.enabled = true;

        steer = cohesion.GetSteering();
        agent.AddSteering(steer, cohesion.weight);

        // add alignment behaviour
        alignment = gameObject.AddComponent<BoidAlignment>();
        alignment.targets = unitManager.GetUnitsInSquad(agent.SquadNum);
        alignment.weight = behaviours.AlignmentWeight;
        alignment.enabled = true;

        steer = alignment.GetSteering();
        agent.AddSteering(steer, cohesion.weight);

        // Add the boid seperation behaviour
        separation = gameObject.AddComponent<BoidSeparation>();
        separation.targets = neighbours;
        separation.weight = behaviours.SeparationWeight;
        separation.enabled = true;

        steer = separation.GetSteering();
        agent.AddSteering(steer, separation.weight);

        // Add obstacle avoidance
        avoid = gameObject.AddComponent<ObstacleAvoidanceBehaviour>();
        avoid.weight = behaviours.AvoidWeight;
        avoid.enabled = true;

        steer = avoid.GetSteering();
        agent.AddSteering(steer, avoid.weight);
    }

    public void SetPath(Vector3 endpoint)
    {
        // clear last path and calculate path to target
        agent.CreatePath(endpoint);        
        path = agent.Path;
        waypointNum = 0;
        finalTarget = endpoint;
        target = path[waypointNum];        
    }

    // Update is called once per frame
    protected override void Update()
    {
        //decelerationDistance = GetDecelerateDistance();

        distanceFromWaypoint = Vector3.Distance(target, transform.position);
        distanceFromTarget = Vector3.Distance(finalTarget, transform.position);

        // change waypoint upon reaching it
        if (target != finalTarget && distanceFromWaypoint < agent.MinDistanceFromWaypoint)
        {
            waypointNum++;
            target = path[waypointNum];
            seekAccel.target = target;
        }

        // check the distance from the final waypoint where formations are formed
        if (distanceFromTarget <= agent.MaxDistanceFromTarget)
        {
            if (cohesion.weight > 0)
                cohesion.weight -= behaviours.CohesionReduction;

            if (separation.weight > 0)
                separation.weight -= behaviours.SeparationReduction;

            if (alignment.weight > 0)
                alignment.weight -= behaviours.AlignmentReduction;

            if (avoid.weight > 0)
                avoid.weight -= behaviours.ObstacleAvoidanceReduction;
        }

        /* Example:
         * if (distance < 5) arriveWeight = 0;
         * else if (distance > 6) arriveWeight = 1;
         * else arriveWeight = distance - 5;
         * */

        // if the distance to start deccelerating has bene reached then change behaviours
        if (distanceFromTarget <= agent.MinDistanceFromTarget)
        {
            unit.ChangeState(UnitState.Idle, target);
            //seekDecel.enabled = true;
            //seekAccel.enabled = false;
        }

        // if the distance from a stationary target has been reached and the unit
        // is close enough to the target then start decelearting
        /*if (distanceFromTarget <= agent.MaxDistanceFromTarget && StationaryUnitInRange())
        {            
            seekDecel.enabled = true;
            seekAccel.enabled = false;
        }*/

        // check if the agent has reached minimum speed
        /*if (seekDecel.enabled && agent.Vecloity.magnitude <= agent.MinSpeedWhenStopping)
        {
            //agent.StopMoving();

            seekDecel.enabled = false;
            unit.ChangeState(UnitState.Idle);
        }*/
    }

    private void OnDestroy()
    {
        agent.StopMoving();
        unitManager.RemoveFromSquad(this.gameObject, agent.SquadNum);

        Destroy(seekAccel);
        Destroy(seekDecel);
        Destroy(cohesion);
        Destroy(alignment);
        Destroy(separation);
    }

    private bool StationaryUnitInRange()
    {
        var neighbours = unitManager.GetNeighbourUnits(this.gameObject, agent.SquadNum);

        foreach (GameObject neighbour in neighbours)
        {
            // check distance from neighbout
            if (Vector3.Distance(neighbour.transform.position, transform.position) <= agent.MinDistanceFromNeighbour)
                return true;
        }

        return false;
    }

    private float GetDecelerateDistance()
    {
        return (agent.CurrentSpeed * agent.CurrentSpeed) / (2 * agent.Deceleration);
    }

    private void OnDrawGizmos()
    {       
        float distance;
        Vector3 direction;

        neighbours = unitManager.GetNeighbourUnits(this.gameObject, agent.SquadNum);
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 3, "Flocking");
#endif
        // Draw the generated path
        if (behaviours.flockPath && path != null)
        {
            // Draws the path
            for (int i = 0; i < path.Length - 1; i++)
            {
                Gizmos.DrawLine(path[i], path[i + 1]);
            }
        }

        if (behaviours.neighbourDistance)
        {
            // draw connection to each neighbour
            foreach (GameObject neighbour in neighbours)
            {
                distance = Vector3.Distance(transform.position, neighbour.transform.position);
                direction = (neighbour.transform.position - transform.position).normalized;

                // sets line colour based on the range
                if (distance > behaviours.CohesionDistance)
                    Gizmos.color = Color.yellow;
                else if (distance < behaviours.DesiredSeparation)
                    Gizmos.color = Color.red;
                else
                    Gizmos.color = Color.green;

                Gizmos.DrawLine(transform.position, neighbour.transform.position);

#if UNITY_EDITOR
                if (neighbour != this.gameObject)
                    UnityEditor.Handles.Label(transform.position + direction * (distance / 2), Math.Round(distance, 2).ToString());
#endif
            }
        }
    }

}