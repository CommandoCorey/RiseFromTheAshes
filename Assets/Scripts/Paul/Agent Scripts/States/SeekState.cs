using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class SeekState : State
{
    [SerializeField]
    float minSpeedWhenStopping = 1.6f;

    // external scripts
    GameManager gameManager;
    UnitController unit;
    AgentMovement agent;
    Steering steer;
    BehaviourManager behaviours;

    // Behaviour classes
    SeekBehaviour seek;
    SeekDecelerateBehaviour decelerate;

    private NavMeshPath path;
    private float distanceFromTarget;
    private float decelerateDistance;
    private Vector3 initialWaypoint;
    private Vector3 finalWaypoint;

    private int waypointNum;

    // Start is called before the first frame update
    void Start()
    {
        unit = GetComponent<UnitController>();
        agent = GetComponent<AgentMovement>();
        behaviours = GetComponent<BehaviourManager>();

        //gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();

        gameManager = GameObject.FindObjectOfType<GameManager>();

        MoveTo(target);

        seek = gameObject.AddComponent<SeekBehaviour>();
        seek.target = initialWaypoint;
        seek.weight = behaviours.SeekWeight;
        seek.enabled = true;

        decelerate = gameObject.AddComponent<SeekDecelerateBehaviour>();
        decelerate.target = target;
        decelerate.weight = behaviours.SeekWeight;
        decelerate.enabled = false;

        steer = seek.GetSteering();
        agent.AddSteering(steer, seek.weight);

        agent.SetPath(target);
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
            return;

        if(path!= null)
            DrawPath();

        distanceFromTarget = Vector3.Distance(seek.target, transform.position);
        decelerateDistance = GetDecelerateDistance();      

        // begin deccelerating towards final position
        if (seek.enabled && distanceFromTarget <= decelerateDistance)
        {
            decelerate.enabled = true;
            seek.enabled = false;
        }

        /*else if (distanceFromTarget <= agent.MinDistanceFromTarget && seek.target != finalWaypoint)
        {
            waypointNum++;           
            seek.target = path.corners[waypointNum];
        }*/

        // check if the agent has reached minimum speed
        if (decelerate.enabled && (agent.Vecloity.magnitude <= minSpeedWhenStopping || distanceFromTarget <= agent.MinDistanceFromTarget))
        {     
            if (waypointNum < path.corners.Length-1)
            {
                waypointNum++;
                agent.StopMoving();                

                decelerate.enabled = false;
                seek.enabled = true;
                seek.target = path.corners[waypointNum];
            }
            else
            {
                decelerate.enabled = false;
                agent.StopMoving();

                unit.ChangeState(UnitState.Idle);                
            }

        }

        //Debug.Log("Veclocity = " + agent.Vecloity.magnitude);            
    }

    public void MoveTo(Vector3 position)
    {
        path = new NavMeshPath();
        bool pathFound = NavMesh.CalculatePath(transform.position, position, NavMesh.AllAreas, path);

        if (!pathFound)
            return;

        waypointNum = 1;
        initialWaypoint = path.corners[waypointNum];
        finalWaypoint = path.corners[path.corners.Length - 1];
    }

    public void EndState()
    {
        seek.enabled = false;
    }

    private void OnDestroy()
    {        
        Destroy(seek);
    }    

    private float GeAccelerateDistance()
    {
        float framesUntilStopped = agent.MaxSpeed / agent.Acceleration;
        return agent.MaxSpeed * framesUntilStopped +(0.5f * agent.Acceleration / Mathf.Pow(framesUntilStopped, 2));
    }

    private float GetDecelerateDistance()
    {
        return (agent.CurrentSpeed * agent.CurrentSpeed) / (2 * agent.Deceleration);
    }

    private void OnDrawGizmos()
    {
        if (path != null)
        {
            // Draws the path
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
            }
        }

    }

    private void DrawPath()
    {
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Debug.DrawLine(path.corners[i], path.corners[i + 1]);
        }
    }


}