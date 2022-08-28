using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveState : MonoBehaviour
{       
    //[SerializeField] float movementSpeed = 100;

    GameObject highlight;

    private NavMeshPath path;
    private Rigidbody body;
    private int waypointNum;
    private Vector3 waypoint;

    private bool moving = false;

    UnitController unit;
    AgentMovement agent;
    UnitState state;

    // Start is called before the first frame update
    void Awake()
    {
        unit = GetComponent<UnitController>();
        agent = GetComponent<AgentMovement>();
        path = new NavMeshPath();
        body = GetComponent<Rigidbody>();
        highlight = GetComponent<UnitController>().selectionHighlight;        
    }

    // Update is called once per frame
    void Update()
    {        
        state = GetComponent<UnitController>().State;       

        if (state == UnitState.Moving)
        {
            for (int i = 0; i < path.corners.Length - 1; i++)
                Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);

            MoveTank();
        }

    }    

    public void MoveTo(Vector3 position)
    {
        body.velocity = Vector3.zero;
        path.ClearCorners();
        waypointNum = 1;

        NavMesh.CalculatePath(transform.position, position, NavMesh.AllAreas, path);
        waypoint = path.corners[waypointNum];

        //foreach (Vector3 coord in path.corners)
            //Debug.Log("Position: " + coord); 
    }

    private void MoveTank()
    {
        Vector3 direction = (waypoint - transform.position).normalized;

        transform.forward = direction; // face moving direction
        body.velocity = direction * agent.MaxSpeed * unit.Speed * Time.deltaTime; // moves the rigid body


        highlight.transform.transform.position = transform.position;

        // check if at waypoint
        if ((waypoint - transform.position).magnitude < 0.2f)
        {
            waypointNum++;

            if (waypointNum < path.corners.Length)
                waypoint = path.corners[waypointNum];
            else
            {                
                GetComponent<UnitController>().ChangeState(UnitState.Idle);

                waypointNum = 1;
                body.velocity = Vector3.zero;
            }
        }

    }

    private void OnDestroy()
    {
        body.velocity = Vector3.zero;
    }

}
