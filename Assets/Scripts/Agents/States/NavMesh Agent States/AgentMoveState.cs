using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class AgentMoveState : MonoBehaviour
{
    //[SerializeField] float movementSpeed = 100;

    private GameObject highlight;

    private NavMeshAgent agent;
    private NavMeshPath path;
    private Rigidbody body;
    private int waypointNum;
    //private Vector3 waypoint;
    private Vector3 targetPos;

    private UnitController unit;
    private UnitState state;    

    // Start is called before the first frame update
    void Awake()
    {
        unit = GetComponent<UnitController>();
        path = new NavMeshPath();
        agent = GetComponent<NavMeshAgent>();
        highlight = GetComponent<UnitController>().selectionHighlight;
        targetPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        //state = GetComponent<UnitController>().State;

        /*
        if (state == UnitState.Moving)
        {
            for (int i = 0; i < path.corners.Length - 1; i++)
                Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);

            MoveTank();
        }*/

        if (unit.State == UnitState.Moving)
        {
            agent.destination = targetPos;

            if (Vector3.Distance(transform.position, agent.destination) < 0.2f)
            {                
                agent.isStopped = true;
                unit.ChangeState(UnitState.Idle);
            }
        }
    }    

    public void MoveTo(Vector3 position)
    {
        targetPos = position;
        agent.SetDestination(targetPos);
        agent.isStopped = false;
    }

    private void OnDrawGizmos()
    {        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPos, 1);        

        UnityEditor.Handles.Label(transform.position + Vector3.up * 1, "Moving");
    }

}
