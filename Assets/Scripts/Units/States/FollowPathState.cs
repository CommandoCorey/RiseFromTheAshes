using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FollowPathState : State
{
    [SerializeField]
    Transform[] patrolRoute;

    [SerializeField] Transform nextWaypoint;

    private int waypointNum = 0;
    [SerializeField] private Vector3 waypoint;
    private NavMeshAgent agent;


    public bool LoopPath { get; set; } = false;

    protected override void Awake()
    {        
        unit = GetComponent<UnitController>();
        agent = unit.body.GetComponent<NavMeshAgent>();        
    }

    private void Start()
    {
        if(LoopPath)
            unit.statusText.text = "Patrolling";
        else
            unit.statusText.text = "Following Path";
    }

    // Update is called once per frame
    protected override void Update()
    {
        // if we are at the wapoint move to the next one
        if(Vector3.Distance(unit.body.position, waypoint) < 0.2f)
        {
            waypointNum++;

            if (waypointNum > patrolRoute.Length - 1)
            {
                if (LoopPath)
                {
                    waypointNum = 0;
                }
                else
                {
                    unit.ChangeState(UnitState.Idle);
                    return;
                }
            }

            nextWaypoint = patrolRoute[waypointNum];
            waypoint = patrolRoute[waypointNum].position;

            agent.destination = waypoint;
        }

        HandleEnemies(); // could cause state to change
    }

    /// <summary>
    /// Sets the route for the unit to follow whole in Patrol State
    /// </summary>
    /// <param name="route">Array of transforms used as waypoints</param>
    public void SetRoute(Transform[] route)
    {
        patrolRoute = route;
        nextWaypoint = patrolRoute[waypointNum];
        waypoint = patrolRoute[waypointNum].position;

        agent.SetDestination(waypoint);
        agent.isStopped = false;
    }
}