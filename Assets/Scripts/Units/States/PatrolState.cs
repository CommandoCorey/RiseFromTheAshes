using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolState : State
{
    Transform[] patrolRoute;

    private int waypointNum = 0;
    private Vector3 waypoint;
    private NavMeshAgent agent;

    protected override void Awake()
    {        
        unit = GetComponent<UnitController>();
        agent = unit.body.GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        // if we are at the wapoint move to the next one
        if(Vector3.Distance(unit.body.position, waypoint) < 0.2f)
        {
            waypointNum++;

            if (waypointNum > patrolRoute.Length-1)
                waypointNum = 0;

            waypoint = patrolRoute[waypointNum].position;

            agent.destination = waypoint;
        }

        //HandleEnemies(); // could cause state to change
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="route"></param>
    public void SetPatrolRoute(Transform[] route)
    {
        patrolRoute = route;
        waypoint = patrolRoute[waypointNum].position;
        agent.SetDestination(waypoint);
    }
}